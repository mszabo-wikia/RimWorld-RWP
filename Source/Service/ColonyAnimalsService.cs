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
using RWP.Cache;
using RWP.Events;
using Verse;

namespace RWP.Service
{
    /// <summary>
    /// Manage caching for animals whose resources (e.g. milk or wool) are ready to be harvested by a pawn.
    /// </summary>
    public class ColonyAnimalsService : IPawnLostEventListener
    {
        private readonly MapScopedThingCache cache;

        public ColonyAnimalsService(MapScopedThingCache cache) => this.cache = cache;

        public void MarkAnimalAsReadyForHarvest(Pawn animal) => this.cache.Add(animal);

        public void RemoveAnimal(Pawn animal) => this.cache.RemoveThing(animal);

        public IEnumerable<Pawn> GetAnimalsReadyForHarvest<T>(Map map)
            where T : ThingComp => from Pawn animal in this.cache.GetThingsForMap(map)
                                   where animal.GetComp<T>() != null
                                   select animal;

        public void OnPawnLost(Pawn pawn) => this.RemoveAnimal(pawn);
    }
}
