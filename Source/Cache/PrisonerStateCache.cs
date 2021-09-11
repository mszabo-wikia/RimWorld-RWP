// Copyright 2021 Máté Szabó
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RWP.Events;
using Verse;
using Verse.AI;

namespace RWP.Cache
{
    /// <summary>
    /// Per-map cache for prisoner status that allows wardening work givers to quickly check
    /// if there are prisoners they might be able to give jobs for.
    /// </summary>
    public class PrisonerStateCache : ITickStartedEventListener
    {
        /// <summary>
        /// Extra hunger offset required for a warden to consider delivering food to a prisoner.
        /// </summary>
        private const double PrisonerHungerToleranceThreshold = 0.02;

        private readonly IDictionary<int, PrisonerState> prisonerStateByMap = new Dictionary<int, PrisonerState>();

        public void OnTickStarted() => this.prisonerStateByMap.Clear();

        public PrisonerState GetPrisonerStateFor(Pawn forWorker)
        {
            int mapId = forWorker.Map.uniqueID;
            if (this.prisonerStateByMap.TryGetValue(mapId, out var cachedPrisonerState))
            {
                return cachedPrisonerState;
            }

            PrisonerState prisonerState = this.InitPrisonerStateFor(forWorker.Map);
            this.prisonerStateByMap[mapId] = prisonerState;
            return prisonerState;
        }

        private PrisonerState InitPrisonerStateFor(Map map)
        {
            PrisonerState state = new PrisonerState();

            foreach (var prisoner in map.mapPawns.PrisonersOfColonySpawned)
            {
                bool isHungry = prisoner.needs.food.CurLevelPercentage <= prisoner.needs.food.PercentageThreshHungry + PrisonerHungerToleranceThreshold;
                bool shouldBeFed = WardenFeedUtility.ShouldBeFed(prisoner);

                if (this.IsInteractablePrisoner(prisoner))
                {
                    state.HasInteractablePrisoner = true;
                }

                if (isHungry && !shouldBeFed && !this.CanUseWorkingPasteDispenser(prisoner))
                {
                    state.HasHungryPrisoner = true;
                }

                if (isHungry && shouldBeFed)
                {
                    state.HasPrisonerToBeFed = true;
                }

                if (!prisoner.Downed && prisoner.guest.interactionMode == PrisonerInteractionModeDefOf.Release)
                {
                    state.HasPrisonerForRelease = true;
                }

                if (prisoner.guest.interactionMode == PrisonerInteractionModeDefOf.Execution)
                {
                    state.HasPrisonerForExecution = true;
                }

                if (!this.OwnsValidBed(prisoner))
                {
                    state.HasPrisonerWithoutBed = true;
                }
            }

            return state;
        }

        /// <summary>
        /// Determine if a given prisoner can be chatted with, as done by <see cref="WorkGiver_Warden_Chat.JobOnThing(Pawn, Thing, bool)"/>.
        /// </summary>
        /// <param name="prisoner">Prisoner to check.</param>
        /// <returns>Whether this prisoner may be interacted with.</returns>
        private bool IsInteractablePrisoner(Pawn prisoner)
        {
            PrisonerInteractionModeDef interactionMode = prisoner.guest.interactionMode;

            // No reason to chat with this prisoner
            if (interactionMode != PrisonerInteractionModeDefOf.AttemptRecruit && interactionMode != PrisonerInteractionModeDefOf.ReduceResistance)
            {
                return false;
            }

            // Can't reduce resistance any further
            if (interactionMode == PrisonerInteractionModeDefOf.ReduceResistance && prisoner.guest.resistance <= 0.0)
            {
                return false;
            }

            // Can't talk, or now is not the right time
            if (!prisoner.guest.ScheduledForInteraction || !prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return false;
            }

            return prisoner.Awake() && (!prisoner.Downed || prisoner.InBed());
        }

        private bool OwnsValidBed(Pawn prisoner)
        {
            var ownedBed = prisoner.ownership?.OwnedBed;
            return ownedBed != null && RestUtility.IsValidBedFor(ownedBed, prisoner, prisoner, true, true);
        }

        /// <summary>
        /// Determine if the given prisoner has access to a working nutrient paste dispenser and is able to operate it.
        /// </summary>
        /// <remarks>
        /// This optimizes for the common case of people feeding their prisoners with nutrient paste, in which case
        /// they require no food deliveries from wardens.
        /// </remarks>
        private bool CanUseWorkingPasteDispenser(Pawn prisoner)
        {
            if (!prisoner.RaceProps.ToolUser || !prisoner.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return false;
            }

            Room room = prisoner.GetRoom();

            return room.Regions.SelectMany(region => region.ListerThings.ThingsOfDef(ThingDefOf.NutrientPasteDispenser))
                .OfType<Building_NutrientPasteDispenser>()
                .Any(dispenser => prisoner.CanReach(dispenser.InteractionCell, PathEndMode.OnCell, Danger.Some) && dispenser.CanDispenseNow);
        }
    }
}
