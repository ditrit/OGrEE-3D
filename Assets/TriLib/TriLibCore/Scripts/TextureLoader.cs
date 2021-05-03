#pragma warning disable 618

using System;
using System.IO;
using StbImageSharp;
using TriLibCore.Extensions;
using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Textures
{
    /// <summary>
    /// Represents a class used to load Unity Textures.
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// Loads a new Unity Texture using the given context data.
        /// </summary>
        /// <param name="textureLoadingContext">Context containing Data from the Original and the Unity Texture.</param>
        public static void LoadTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.Context.LoadedTextureGroups.TryGetValue(textureLoadingContext.Texture, out var existingTextureLoadingContext))
            {
                textureLoadingContext.OriginalUnityTexture = existingTextureLoadingContext.OriginalUnityTexture;
                textureLoadingContext.UnityTexture = existingTextureLoadingContext.UnityTexture;
                textureLoadingContext.Context.LoadedTexturesCount++;
                return;
            }
            if (textureLoadingContext.Context.Options.TextureMapper != null)
            {
                textureLoadingContext.Context.Options.TextureMapper.Map(textureLoadingContext);
            }
            if (textureLoadingContext.UnityTexture == null && textureLoadingContext.Texture.Data != null)
            {
#if TRILIB_USE_UNITY_TEXTURE_LOADER
                UnityLoadFromContext(textureLoadingContext);
#else
                if (textureLoadingContext.Stream == null)
                {
                    textureLoadingContext.Stream = new MemoryStream(textureLoadingContext.Texture.Data);
                }
                StbLoadFromContext(textureLoadingContext, ColorComponents.RedGreenBlueAlpha);
#endif
            }
            if (textureLoadingContext.UnityTexture == null && textureLoadingContext.Stream != null)
            {
#if TRILIB_USE_UNITY_TEXTURE_LOADER
                UnityLoadFromContext(textureLoadingContext);
#else
                StbLoadFromContext(textureLoadingContext, ColorComponents.RedGreenBlueAlpha);
#endif
            }
            if (textureLoadingContext.UnityTexture == null && textureLoadingContext.Texture.Filename != null)
            {
                if (textureLoadingContext.Texture.ResolvedFilename == null)
                {
                    textureLoadingContext.Texture.ResolvedFilename = FileUtils.FindFile(textureLoadingContext.Context.BasePath, textureLoadingContext.Texture.Filename);
                }
                if (textureLoadingContext.Texture.ResolvedFilename != null)
                {
#if TRILIB_USE_UNITY_TEXTURE_LOADER
                    UnityLoadFromContext(textureLoadingContext);
#else
                    if (File.Exists(textureLoadingContext.Texture.ResolvedFilename))
                    {
                        textureLoadingContext.Stream = new FileStream(textureLoadingContext.Texture.ResolvedFilename, FileMode.Open, FileAccess.Read);
                        StbLoadFromContext(textureLoadingContext, ColorComponents.RedGreenBlueAlpha);
                    }
#endif
                }
            }
            if (textureLoadingContext.UnityTexture == null && textureLoadingContext.Context.Options.ShowLoadingWarnings)
            {
                Debug.LogWarning($"Could not load texture :{textureLoadingContext.Texture.Filename ?? "No-name"}");
            }
            textureLoadingContext.Context.LoadedTextureGroups[textureLoadingContext.Texture] = new TextureGroup(textureLoadingContext.OriginalUnityTexture, textureLoadingContext.UnityTexture, textureLoadingContext.Texture);
            textureLoadingContext.Context.LoadedTextures[textureLoadingContext.Texture] = textureLoadingContext; //kept for compatibility
            textureLoadingContext.Context.LoadedTexturesCount++;
        }

        private static void ScanForAlphaPixels(TextureLoadingContext textureLoadingContext)
        {
            var hasAlpha = false;
            if (textureLoadingContext.BytesPerPixel == 8)
            {
                for (var i = 3; i < textureLoadingContext.Data16.Length; i += 4)
                {
                    if (textureLoadingContext.Data16[i] < ushort.MaxValue)
                    {
                        hasAlpha = true;
                        break;
                    }
                }
            }
            else
            {
                for (var i = 3; i < textureLoadingContext.Data.Length; i += 4)
                {
                    if (textureLoadingContext.Data[i] < byte.MaxValue)
                    {
                        hasAlpha = true;
                        break;
                    }
                }
            }
            textureLoadingContext.HasAlpha = hasAlpha;
        }

#if TRILIB_USE_UNITY_TEXTURE_LOADER
        private static void UnityLoadFromContext(TextureLoadingContext textureLoadingContext)
        {
            Dispatcher.InvokeAsyncAndWait(TextureUtils.LoadTexture2D, textureLoadingContext);
            //todo: apply textures data
        }
#else
        private static void StbLoadFromContext(TextureLoadingContext textureLoadingContext, ColorComponents requiredComponents = ColorComponents.Default)
        {
            try
            {
                StbImage.FromContext(requiredComponents, textureLoadingContext);
                if (textureLoadingContext.UnityTexture != null)
                {
                    StbProcessTexture(textureLoadingContext);
                }
                textureLoadingContext.Stream.TryToDispose();
            }
            catch (Exception e)
            {
                if (textureLoadingContext.Context.Options.ShowLoadingWarnings)
                {
                    if (textureLoadingContext.Texture != null)
                    {
                        Debug.LogWarning($"Could not load texture {textureLoadingContext.Texture.Name ?? textureLoadingContext.Texture.Filename ?? "No-name"} :{e}");
                    }
                }
                textureLoadingContext.Stream.TryToDispose();
            }
        }

        private static void StbProcessTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.Context.Options.ScanForAlphaPixels)
            {
                ScanForAlphaPixels(textureLoadingContext);
            }
            StbImageApplyTextureData(textureLoadingContext);
            if (textureLoadingContext.Texture.TextureFormat == General.TextureFormat.UNorm && textureLoadingContext.Context.Options.FixNormalMaps)
            {
                StbImageFixNormalMap(textureLoadingContext);
                StbImageApplyTextureData(textureLoadingContext);
            }
        }

        private static void StbImageFixNormalMap(TextureLoadingContext textureLoadingContext)
        {
            Dispatcher.InvokeAsyncAndWait(TextureUtils.FixNormalMap, textureLoadingContext);
        }

        private static void StbImageApplyTextureData(TextureLoadingContext textureLoadingContext)
        {
            Dispatcher.InvokeAsyncAndWait(TextureUtils.ApplyTexture2D, textureLoadingContext);
        }
#endif
    }
}