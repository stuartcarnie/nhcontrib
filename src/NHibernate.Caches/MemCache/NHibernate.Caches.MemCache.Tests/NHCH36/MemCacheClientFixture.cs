using System.Collections.Generic;
using System.Threading;
using log4net.Config;
using NHibernate.Cache;
using NUnit.Framework;

namespace NHibernate.Caches.MemCache.Tests.NHCH36
{
    /// <summary>
    /// issue http://216.121.112.228/browse/NHCH-36
    /// </summary>
    [TestFixture]
    public class MemCacheClientFixture
    {
        private MemCacheProvider provider;
        private Dictionary<string, string> props;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            XmlConfigurator.Configure();
            props = new Dictionary<string, string> { { "compression_enabled", "false" }, { "expiration", "20" } };
            provider = new MemCacheProvider();
            provider.Start(props);
        }

        [TestFixtureTearDown]
        public void FixtureStop()
        {
            provider.Stop();
        }

        [Test]
        public void NoCollisionsWithCompositeEntityKey()
        {
            var keyVals = new[]
                              {
                                  new KeyValuePair<EntityKey, string>(new EntityKey("aaa", "bbb"), "value1"),
                                  new KeyValuePair<EntityKey, string>(new EntityKey("aaa", "ccc"), "value2"),
                                  new KeyValuePair<EntityKey, string>(new EntityKey("aaa", "ddd"), "value3"),
                              };

            var cache = provider.BuildCache("nunit", props);
            Assume.That(cache, Is.Not.Null, "no cache returned");

            // add the items
            foreach (var keyValuePair in keyVals)
            {
                cache.Put(keyValuePair.Key, keyValuePair.Value);
            }
            Thread.Sleep(500);

            // make sure it's there
            foreach (var t in keyVals)
            {
                var item = cache.Get(t.Key);
                Assert.That(item, Is.EqualTo(t.Value));
            }
        }

        class EntityKey
        {
            private readonly string _a;
            private readonly string _b;

            public EntityKey(string a, string b)
            {
                _a = a;
                _b = b;
            }

            public override int GetHashCode()
            {
                int hash = 37;
                hash ^= _a.GetHashCode();
                hash ^= 17;
                hash ^= _b.GetHashCode();
                return hash;
            }
        }
    }
}