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

namespace RWP.Cache
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="Thing"/> and subtypes
    /// that uses the unique ID number of <see cref="Thing"/> instances for comparisons.
    /// </summary>
    /// <typeparam name="T"><see cref="Thing"/> (sub)type to compare.</typeparam>
    public class ThingByIdEqualityComparer<T> : IEqualityComparer<T>
        where T : Thing
    {
        public bool Equals(T x, T y)
        {
            if (x != null)
            {
                return y != null && (x.thingIDNumber == y.thingIDNumber);
            }

            return y == null;
        }

        public int GetHashCode(T obj) => obj.thingIDNumber.GetHashCode();
    }
}
