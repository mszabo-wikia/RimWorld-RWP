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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RWP.Cache;

namespace RWP.Tests.Cache
{
    [TestClass]
    public class LRUCacheTests
    {
        [TestMethod]
        public void TestLRUCache_WillEvictLeastRecentlyAddedItem()
        {
            var cache = new LRUCache<string, string>(3);
            cache.Add("item-1", "foo");
            cache.Add("item-2", "bar");
            cache.Add("item-3", "baz");

            cache.Add("item-4", "test");

            Assert.IsNull(cache.Get("item-1"));
            Assert.AreEqual("bar", cache.Get("item-2"));
        }

        [TestMethod]
        public void TestLRUCache_WillEvictLeastRecentlyUsedItem()
        {
            var cache = new LRUCache<string, string>(3);
            cache.Add("item-1", "foo");
            cache.Add("item-2", "bar");
            cache.Add("item-3", "baz");

            cache.Get("item-1");

            cache.Add("item-4", "test");

            Assert.IsNull(cache.Get("item-2"));
            Assert.AreEqual("foo", cache.Get("item-1"));
        }

        [TestMethod]
        public void TestLRUCache_WillRemoveGivenItem()
        {
            var cache = new LRUCache<string, string>(3);
            cache.Add("item-1", "foo");
            cache.Add("item-2", "bar");

            cache.Remove("item-1");

            Assert.IsNull(cache.Get("item-1"));
            Assert.AreEqual("bar", cache.Get("item-2"));
        }
    }
}
