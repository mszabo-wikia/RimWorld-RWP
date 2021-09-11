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

namespace RWP.Cache
{
    /// <summary>
    /// Per-pawn cache for quest parts that disable particular work types.
    /// </summary>
    public class QuestPartWorkDisabledCache : IPawnLostEventListener
    {
        private readonly QuestManager questManager;
        private readonly IDictionary<int, IList<QuestPart_WorkDisabled>> questPartsByPawn = new Dictionary<int, IList<QuestPart_WorkDisabled>>();

        public QuestPartWorkDisabledCache(QuestManager questManager) => this.questManager = questManager;

        public IEnumerable<QuestPart_WorkDisabled> GetQuestPartsFor(Pawn pawn)
        {
            if (this.questPartsByPawn.TryGetValue(pawn.thingIDNumber, out var cachedQuestParts))
            {
                return cachedQuestParts;
            }

            var questParts = this.questManager.QuestsListForReading
                .Where(quest => quest.State == QuestState.Ongoing)
                .SelectMany(quest => quest.PartsListForReading)
                .OfType<QuestPart_WorkDisabled>()
                .Where(workDisabledPart => workDisabledPart.pawns.Contains(pawn))
                .ToList();

            this.questPartsByPawn[pawn.thingIDNumber] = questParts;

            return questParts;
        }

        public void RemoveQuestPart(QuestPart_WorkDisabled questPart)
        {
            foreach (var pawn in questPart.pawns)
            {
                this.RemovePawn(pawn);
            }
        }

        public void OnPawnLost(Pawn pawn) => this.RemovePawn(pawn);

        public void RemovePawn(Pawn pawn)
        {
            int pawnId = pawn.thingIDNumber;
            this.questPartsByPawn.Remove(pawnId);
        }
    }
}
