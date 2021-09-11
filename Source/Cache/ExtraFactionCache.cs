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

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RWP.Events;
using Verse;

namespace RWP.Cache
{
    /// <summary>
    /// Per-pawn cache for extra/mini factions handed out to them by quests.
    /// </summary>
    public class ExtraFactionCache : IPawnLostEventListener
    {
        private readonly QuestManager questManager;
        private readonly IDictionary<ExtraFactionCacheKey, Faction> factionByPawn =
            new Dictionary<ExtraFactionCacheKey, Faction>(new ExtraFactionCacheKeyEqualityComparer());

        public ExtraFactionCache(QuestManager questManager) => this.questManager = questManager;

        public Faction GetExtraFactionFor(Pawn pawn, ExtraFactionType extraFactionType)
        {
            var cacheKey = this.MakeCacheKey(pawn, extraFactionType);
            if (this.factionByPawn.TryGetValue(cacheKey, out var cachedExtraFaction))
            {
                return cachedExtraFaction;
            }

            var extraFaction = this.DetermineExtraFaction(pawn, extraFactionType);
            this.factionByPawn[cacheKey] = extraFaction;
            return extraFaction;
        }

        public void OnPawnLost(Pawn pawn) => this.RemovePawn(pawn);

        public void RemovePawn(Pawn pawn)
        {
            foreach (ExtraFactionType extraFactionType in Enum.GetValues(typeof(ExtraFactionType)))
            {
                var cacheKey = this.MakeCacheKey(pawn, extraFactionType);
                this.factionByPawn.Remove(cacheKey);
            }
        }

        public void RemoveEntriesFor(QuestPart_ExtraFaction questPart)
        {
            foreach (var pawn in questPart.affectedPawns)
            {
                var cacheKey = this.MakeCacheKey(pawn, questPart.extraFaction.factionType);
                this.factionByPawn.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Find the first extra faction allotted to this pawn by a quest, as done by <see cref="QuestUtility.GetExtraFaction(Pawn, ExtraFactionType, Quest)"/> in core.
        /// </summary>
        /// <param name="pawn">Pawn to fetch extra faction for.</param>
        /// <param name="extraFactionType">Extra faction type to search for.</param>
        /// <returns>Faction from quest, or null if none exists.</returns>
        private Faction DetermineExtraFaction(Pawn pawn, ExtraFactionType extraFactionType) =>
            this.questManager.QuestsListForReading
                .Where(quest => quest.State == QuestState.Ongoing)
                .SelectMany(quest => quest.PartsListForReading)
                .OfType<QuestPart_ExtraFaction>()
                .Where(extraFactionPart =>
                        extraFactionPart.extraFaction.factionType.Equals(extraFactionType) &&
                        extraFactionPart.affectedPawns.Contains(pawn))
                .Select(extraFactionPart => extraFactionPart.extraFaction.faction)
                .FirstOrDefault();

        private ExtraFactionCacheKey MakeCacheKey(Pawn pawn, ExtraFactionType extraFactionType) => new ExtraFactionCacheKey(pawn.thingIDNumber, extraFactionType);

        /// <summary>
        /// Optimized cache key struct that avoids the equality comparison and hashing overhead of <see cref="System.Tuple"/>.
        /// </summary>
        private struct ExtraFactionCacheKey
        {
            internal readonly int PawnId;
            internal readonly ExtraFactionType ExtraFactionType;

            internal ExtraFactionCacheKey(int pawnId, ExtraFactionType extraFactionType)
            {
                this.PawnId = pawnId;
                this.ExtraFactionType = extraFactionType;
            }
        }

        /// <summary>
        /// Optimized equality comparison and hashing for <see cref="ExtraFactionCacheKey" /> instances.
        /// </summary>
        private class ExtraFactionCacheKeyEqualityComparer : IEqualityComparer<ExtraFactionCacheKey>
        {
            public bool Equals(ExtraFactionCacheKey key, ExtraFactionCacheKey otherKey) => (key.PawnId == otherKey.PawnId) && (key.ExtraFactionType == otherKey.ExtraFactionType);

            public int GetHashCode(ExtraFactionCacheKey key) => key.PawnId ^ (byte)key.ExtraFactionType;
        }
    }
}
