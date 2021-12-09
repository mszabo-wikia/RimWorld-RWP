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
using RimWorld;
using Verse;

namespace RWP.Cache
{
    /// <summary>
    /// Per-pawn cache for relationships with other pawns.
    /// </summary>
    public class PawnRelationsCache
    {
        private readonly LRUCache<PawnRelation, IList<PawnRelationDef>> cache = new LRUCache<PawnRelation, IList<PawnRelationDef>>(1024, new PawnRelationEqualityComparer());

        public void AddRelationsBetween(Pawn subject, Pawn related, IList<PawnRelationDef> relations)
        {
            if (!subject.RaceProps.Humanlike)
            {
                return;
            }

            var cacheKey = new PawnRelation(subject, related);
            this.cache.Add(cacheKey, relations);
        }

        public IList<PawnRelationDef> GetRelationsBetween(Pawn subject, Pawn related) => this.cache.Get(new PawnRelation(subject, related));

        public void ClearRelationsBetween(Pawn subject, Pawn otherPawn)
        {
            // Make sure to clear any cached relations in both directions
            var subjectKey = new PawnRelation(subject, otherPawn);
            var relatedKey = new PawnRelation(otherPawn, subject);

            this.cache.Remove(subjectKey);
            this.cache.Remove(relatedKey);
        }

        private readonly struct PawnRelation
        {
            internal readonly int SubjectId;
            internal readonly int RelatedId;

            internal PawnRelation(Pawn subject, Pawn related)
            {
                this.SubjectId = subject.thingIDNumber;
                this.RelatedId = related.thingIDNumber;
            }
        }

        private sealed class PawnRelationEqualityComparer : IEqualityComparer<PawnRelation>
        {
            public bool Equals(PawnRelation x, PawnRelation y) => (x.SubjectId == y.SubjectId) && (x.RelatedId == y.RelatedId);

            public int GetHashCode(PawnRelation obj) => obj.SubjectId ^ obj.RelatedId;
        }
    }
}
