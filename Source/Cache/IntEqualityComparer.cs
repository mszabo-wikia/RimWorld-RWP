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

namespace RWP.Cache
{
    /// <summary>
    /// Optimized <see cref="IEqualityComparer{T}"/> implementation for 32-bit integers.
    /// </summary>
    /// <remarks>
    /// This is intended as a micro-optimization for very highly accessed caches such as <see cref="LordsPawnsCache"/>.
    /// </remarks>
    public class IntEqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y) => x == y;

        public int GetHashCode(int obj) => obj;
    }
}
