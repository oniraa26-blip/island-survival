using IslandSurvival.Save;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class SaveStoreTests
    {
        sealed class MemoryStore : ISaveStore
        {
            string value;
            public bool Exists { get { return value != null; } }
            public bool TryRead(out string json) { json = value; return value != null; }
            public bool Write(string json) { value = json; return true; }
        }

        [Test] public void SaveStoreContractRoundTripsJsonWithoutFilesystemDependency()
        {
            ISaveStore store = new MemoryStore(); string loaded;
            Assert.IsFalse(store.Exists); Assert.IsTrue(store.Write("{\"version\":1}"));
            Assert.IsTrue(store.TryRead(out loaded)); Assert.AreEqual("{\"version\":1}", loaded);
        }
    }
}
