using IslandSurvival.Platform;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class PlatformProfileTests
    {
        [Test] public void WebProfileIsStrictlyLighterThanIOSForWorldBudgets()
        {
            var ios = PlatformProfile.IOS(); var web = PlatformProfile.WebMobile();
            Assert.Less(web.targetFrameRate, ios.targetFrameRate);
            Assert.Less(web.renderScale, ios.renderScale);
            Assert.Less(web.shadowDistance, ios.shadowDistance);
            Assert.Less(web.cameraFarClip, ios.cameraFarClip);
            Assert.Less(web.vegetationDensity, ios.vegetationDensity);
            Assert.Less(web.maxActiveWorldObjects, ios.maxActiveWorldObjects);
            Assert.Less(web.maxLoadedZones, ios.maxLoadedZones);
        }

        [Test] public void EveryMobileProfileKeepsThirtyFpsBaseline()
        {
            Assert.GreaterOrEqual(PlatformProfile.IOS().baselineFrameRate, 30);
            Assert.GreaterOrEqual(PlatformProfile.WebMobile().baselineFrameRate, 30);
            Assert.GreaterOrEqual(PlatformProfile.Android().baselineFrameRate, 30);
        }
    }
}
