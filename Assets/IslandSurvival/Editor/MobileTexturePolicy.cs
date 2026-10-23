using UnityEditor;

namespace IslandSurvival.Editor
{
    public sealed class MobileTexturePolicy : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            Apply(importer, "iPhone", 2048, TextureImporterFormat.ASTC_6x6);
            Apply(importer, "Android", 2048, TextureImporterFormat.ETC2_RGBA8);
            Apply(importer, "WebGL", 1024, TextureImporterFormat.Automatic);
        }

        static void Apply(TextureImporter importer, string platform, int maxSize, TextureImporterFormat format)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true; settings.maxTextureSize = maxSize; settings.format = format;
            settings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
