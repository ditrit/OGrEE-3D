#pragma warning disable 672

using System;
using ICSharpCode.SharpZipLib.Zip;
using TriLibCore.Interfaces;
using TriLibCore.Utils;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a mapper class class used to load Textures from Zip files.</summary>
    public class ZipFileTextureMapper : TextureMapper
    {
        /// <inheritdoc />
        public override TextureLoadingContext Map(AssetLoaderContext assetLoaderContext, ITexture texture)
        {
            var zipLoadCustomContextData = assetLoaderContext.CustomData as ZipLoadCustomContextData;
            if (zipLoadCustomContextData == null)
            {
                throw new Exception("Missing custom context data.");
            }
            var zipFile = zipLoadCustomContextData.ZipFile;
            if (zipFile == null)
            {
                throw new Exception("Zip file instance is null.");
            }
            if (string.IsNullOrWhiteSpace(texture.Filename))
            {
                if (assetLoaderContext.Options.ShowLoadingWarnings)
                {
                    UnityEngine.Debug.LogWarning("Texture name is null.");
                }
                return null;
            }
            var shortFileName = FileUtils.GetShortFilename(texture.Filename).ToLowerInvariant();
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (!zipEntry.IsFile)
                {
                    continue;
                }
                var checkingFileShortName = FileUtils.GetShortFilename(zipEntry.Name).ToLowerInvariant();
                if (shortFileName == checkingFileShortName)
                {
                    string _;
                    var textureLoadingContext = new TextureLoadingContext
                    {
                        Context = assetLoaderContext,
                        Stream = AssetLoaderZip.ZipFileEntryToStream(out _, zipEntry, zipFile),
                        Texture = texture
                    };
                    return textureLoadingContext;
                }
            }
            return null;
        }
    }
}