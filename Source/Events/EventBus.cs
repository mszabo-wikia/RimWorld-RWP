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
using Verse;

namespace RWP.Events
{
    /// <summary>
    /// Central dispatcher for events that need to notify multiple clients.
    /// </summary>
    public class EventBus
    {
        private readonly IEnumerable<IPawnLostEventListener> pawnLostEventListeners;
        private readonly IEnumerable<IThingLostEventListener> thingLostEventListeners;
        private readonly IEnumerable<ITickStartedEventListener> tickStartedEventListeners;

        public EventBus(
            IEnumerable<IPawnLostEventListener> pawnLostEventListeners,
            IEnumerable<IThingLostEventListener> thingLostEventListeners,
            IEnumerable<ITickStartedEventListener> tickStartedEventListeners)
        {
            this.pawnLostEventListeners = pawnLostEventListeners;
            this.thingLostEventListeners = thingLostEventListeners;
            this.tickStartedEventListeners = tickStartedEventListeners;
        }

        /// <summary>
        /// Fired when a pawn is deregistered from its parent map (possibly because it died or its faction affiliation changed).
        /// </summary>
        public void OnMapPawnDeRegistered(Pawn pawn)
        {
            foreach (IPawnLostEventListener listener in this.pawnLostEventListeners)
            {
                listener.OnPawnLost(pawn);
            }
        }

        /// <summary>
        /// Fired when a thing despawns from its map.
        /// </summary>
        public void OnThingDeSpawned(Thing thing)
        {
            foreach (IThingLostEventListener listener in this.thingLostEventListeners)
            {
                listener.OnThingLost(thing);
            }
        }

        /// <summary>
        /// Fired at the start of each new game tick.
        /// </summary>
        public void OnTickStarted()
        {
            foreach (ITickStartedEventListener listener in this.tickStartedEventListeners)
            {
                listener.OnTickStarted();
            }
        }
    }
}
