using UnityEditor;

namespace IslandSurvival.Editor
{
    public sealed class MobileTexturePolicy : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            var supportsExplicitColorCompression = IsColorTexture2D(importer);
            Apply(importer, "iPhone", 2048, supportsExplicitColorCompression ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.Automatic);
            Apply(importer, "Android", 2048, supportsExplicitColorCompression ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.Automatic);
            Apply(importer, "WebGL", 1024, TextureImporterFormat.Automatic);
        }

        static bool IsColorTexture2D(TextureImporter importer)
        {
            if (importer.textureShape != TextureImporterShape.Texture2D) return false;
            return importer.textureType == TextureImporterType.Default
                || importer.textureType == TextureImporterType.Sprite;
        }

        static void Apply(TextureImporter importer, string platform, int maxSize, TextureImporterFormat format)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            // Automatic is intentional for SingleChannel, NormalMap, Lightmap and other
            // specialized types: Unity selects a format valid for the type and platform.
            settings.overridden = true; settings.maxTextureSize = maxSize; settings.format = format;
            settings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
