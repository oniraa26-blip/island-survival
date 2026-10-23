using IslandSurvival.World;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class WorldGeometryTests
    {
        [Test]
        public void DemoTerrainCoversSouthernAndCentralRegion()
        {
            var terrain = StylizedGeometry.Terrain();
            Assert.Greater(terrain.vertexCount, 500);
            Assert.Greater(terrain.triangles.Length, 1500);
            Assert.Greater(terrain.bounds.max.z, 320f);
            Assert.Less(terrain.bounds.min.z, 0f);
            Assert.Greater(terrain.bounds.extents.x, 175f);
        }

        [Test]
        public void SharedPrototypeMeshesHaveRenderableGeometry()
        {
            Assert.Greater(StylizedGeometry.PalmTrunk().triangles.Length, 0);
            Assert.Greater(StylizedGeometry.PalmLeaf().triangles.Length, 0);
            Assert.Greater(StylizedGeometry.Rock(0).triangles.Length, 0);
            Assert.Greater(StylizedGeometry.Branch().triangles.Length, 0);
        }
    }
}
