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
    /// Cache implementation that stores items up to a specified maximum capacity.
    /// If the capacity is reached, it removes the least recently used item.
    /// </summary>
    /// <typeparam name="TKey">Cache key type.</typeparam>
    /// <typeparam name="TValue">Cached value type.</typeparam>
    public class LRUCache<TKey, TValue>
    {
        private readonly LinkedList<TKey> keysAccess = new LinkedList<TKey>();
        private readonly IDictionary<TKey, CacheEntry> cache;
        private readonly int maxSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRUCache{TKey, TValue}"/> class
        /// with the given maximum capacity using the default <see cref="IEqualityComparer{TKey}"/> implementation.
        /// </summary>
        /// <param name="maxSize">Maximum number of items to store in the cache.</param>
        public LRUCache(int maxSize)
            : this(maxSize, EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRUCache{TKey, TValue}"/> class
        /// with the given maximum capacity using the provided <see cref="IEqualityComparer{TKey}"/> implementation.
        /// </summary>
        /// <param name="maxSize">Maximum number of items to store in the cache.</param>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{TKey}"/> implementation to use.</param>
        public LRUCache(int maxSize, IEqualityComparer<TKey> equalityComparer)
        {
            this.maxSize = maxSize;
            this.cache = new Dictionary<TKey, CacheEntry>(equalityComparer);
        }

        /// <summary>
        /// Return the item stored in the cache under the given key,
        /// or the default type if it is not in the cache.
        /// </summary>
        /// <param name="key">Cache key to look up.</param>
        /// <returns>Cached value or default.</returns>
        public TValue Get(TKey key)
        {
            if (this.cache.TryGetValue(key, out CacheEntry cacheEntry))
            {
                this.keysAccess.Remove(cacheEntry.KeyNode);
                this.keysAccess.AddFirst(cacheEntry.KeyNode);
                return cacheEntry.Value;
            }

            return default;
        }

        /// <summary>
        /// Add a new item to the cache, evicting the least recently used item
        /// if it goes over capacity.
        /// </summary>
        /// <param name="key">Cache key to store the item under.</param>
        /// <param name="value">Cached value.</param>
        public void Add(TKey key, TValue value)
        {
            CacheEntry cacheEntry = new CacheEntry(key, value);
            this.cache[key] = cacheEntry;
            this.keysAccess.AddFirst(cacheEntry.KeyNode);

            if (this.cache.Count > this.maxSize)
            {
                TKey leastRecentlyUsed = this.keysAccess.Last.Value;
                this.cache.Remove(leastRecentlyUsed);
                this.keysAccess.RemoveLast();
            }
        }

        /// <summary>
        /// Remove an entry from the cache.
        /// </summary>
        /// <param name="key">Cache key to remove.</param>
        public void Remove(TKey key)
        {
            if (this.cache.TryGetValue(key, out CacheEntry cacheEntry))
            {
                this.keysAccess.Remove(cacheEntry.KeyNode);
                this.cache.Remove(key);
            }
        }

        private readonly struct CacheEntry
        {
            public readonly LinkedListNode<TKey> KeyNode;
            public readonly TValue Value;

            public CacheEntry(TKey key, TValue value)
            {
                this.KeyNode = new LinkedListNode<TKey>(key);
                this.Value = value;
            }
        }
    }
}
