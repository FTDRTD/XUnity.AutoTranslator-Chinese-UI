using System;
using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.Common.Compatibility;

namespace XUnity.ResourceRedirector
{
   /// <summary>
   /// Utility methods for AssetBundles.
   /// </summary>
   public static class AssetBundleHelper
   {
      internal static string PathForLoadedInMemoryBundle;

      /// <summary>
      /// Creates an empty AssetBundle with a randomly generated CAB identifier.
      /// </summary>
      /// <returns>The empty asset bundle with a random CAB identifier.</returns>
      public static AssetBundle CreateEmptyAssetBundle()
      {
         var buffer = Properties.Resources.empty;
         CabHelper.RandomizeCab( buffer );
         return AssetBundle.LoadFromMemory( buffer );
      }

      /// <summary>
      /// Creates an empty AssetBundle request with a randomly generated CAB identifier.
      /// </summary>
      /// <returns>The asset bundle request with a random CAB identifier.</returns>
      public static AssetBundleCreateRequest CreateEmptyAssetBundleRequest()
      {
         var buffer = Properties.Resources.empty;
         CabHelper.RandomizeCab( buffer );
         return AssetBundle.LoadFromMemoryAsync( buffer );
      }

      /// <summary>
      /// Convenience method to maintain a name of an asset bundle being loaded through
      /// memory for logging purposes.
      /// </summary>
      /// <param name="path">Path to the asset bundle being loaded. Only used for logging.</param>
      /// <param name="binary">Binary data of the asset bundle being loaded.</param>
      /// <param name="crc">Crc of the asset bundle.</param>
      /// <returns>The loaded asset bundle.</returns>
      public static AssetBundle LoadFromMemory( string path, byte[] binary, uint crc )
      {
         try
         {
            PathForLoadedInMemoryBundle = path;

            return AssetBundle.LoadFromMemory( binary, crc );
         }
         finally
         {
            PathForLoadedInMemoryBundle = null;
         }
      }

      /// <summary>
      /// Convenience method to maintain a name of an asset bundle being loaded through
      /// memory for logging purposes.
      /// </summary>
      /// <param name="path">Path to the asset bundle being loaded. Only used for logging.</param>
      /// <param name="binary">Binary data of the asset bundle being loaded.</param>
      /// <param name="crc">Crc of the asset bundle.</param>
      /// <returns>The request.</returns>
      public static AssetBundleCreateRequest LoadFromMemoryAsync( string path, byte[] binary, uint crc )
      {
         try
         {
            PathForLoadedInMemoryBundle = path;
            
            return AssetBundle.LoadFromMemoryAsync( binary, crc );
         }
         finally
         {
            PathForLoadedInMemoryBundle = null;
         }
      }

      /// <summary>
      /// Loads an asset bundle from a file. If loading fails, randomize the CAB and try again from memory.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="crc"></param>
      /// <param name="offset"></param>
      /// <returns></returns>
      public static AssetBundle LoadFromFileWithRandomizedCabIfRequired( string path, uint crc, ulong offset )
      {
         return LoadFromFileWithRandomizedCabIfRequired( path, crc, offset, true );
      }

      /// <summary>
      /// Unity 2022+兼容的AssetBundle加载方法
      /// </summary>
      /// <param name="path">AssetBundle路径</param>
      /// <param name="crc">CRC校验值</param>
      /// <param name="offset">偏移量</param>
      /// <param name="useNewAPI">是否使用Unity 2022+的新API</param>
      /// <returns>加载的AssetBundle</returns>
      public static AssetBundle LoadFromFileUnity2022Compatible( string path, uint crc, ulong offset, bool useNewAPI = true )
      {
         var context = ResourceLoadingContext.Create( path, ResourceLoadingContextType.AssetBundle );
         
         try
         {
            AssetBundle bundle = null;
            
            if( CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher && useNewAPI )
            {
               // Unity 2022+的新加载方式
               bundle = LoadFromFileUnity2022( path, crc, offset );
            }
            else
            {
               // 传统加载方式
               bundle = AssetBundle.LoadFromFile( path, crc, offset );
            }
            
            if( bundle == null && File.Exists( path ) )
            {
               // 如果加载失败，尝试随机化CAB
               bundle = LoadFromFileWithRandomizedCabIfRequired( path, crc, offset, true );
            }
            
            context.CompleteLoading( bundle );
            return bundle;
         }
         catch( Exception ex )
         {
            context.CompleteLoading( ex );
            throw;
         }
         finally
         {
            ResourceLoadingContext.RemoveContext( context.ContextId );
         }
      }

      /// <summary>
      /// Unity 2022+的AssetBundle加载实现
      /// </summary>
      private static AssetBundle LoadFromFileUnity2022( string path, uint crc, ulong offset )
      {
         try
         {
            // Unity 2022+可能有一些新的加载参数或方式
            // 这里可以根据实际的Unity 2022+ API进行调整
            
            // 目前使用标准API，但添加了额外的错误处理
            var bundle = AssetBundle.LoadFromFile( path, crc, offset );
            
            if( bundle == null )
            {
               XuaLogger.ResourceRedirector.Warn( $"Unity 2022+ AssetBundle加载失败: {path}" );
            }
            
            return bundle;
         }
         catch( Exception ex )
         {
            XuaLogger.ResourceRedirector.Error( ex, $"Unity 2022+ AssetBundle加载异常: {path}" );
            return null;
         }
      }

      /// <summary>
      /// Unity 2022+兼容的异步AssetBundle加载方法
      /// </summary>
      /// <param name="path">AssetBundle路径</param>
      /// <param name="crc">CRC校验值</param>
      /// <param name="offset">偏移量</param>
      /// <param name="useNewAPI">是否使用Unity 2022+的新API</param>
      /// <returns>AssetBundle创建请求</returns>
      public static AssetBundleCreateRequest LoadFromFileAsyncUnity2022Compatible( string path, uint crc, ulong offset, bool useNewAPI = true )
      {
         var context = ResourceLoadingContext.Create( path, ResourceLoadingContextType.AssetBundle );
         
         try
         {
            AssetBundleCreateRequest request = null;
            
            if( CompatibilityHelper.CompatibilityInfo.IsUnity2022OrHigher && useNewAPI )
            {
               // Unity 2022+的新异步加载方式
               request = LoadFromFileAsyncUnity2022( path, crc, offset );
            }
            else
            {
               // 传统异步加载方式
               request = AssetBundle.LoadFromFileAsync( path, crc, offset );
            }
            
            // 注意：异步加载的完成状态需要外部处理
            return request;
         }
         catch( Exception ex )
         {
            context.CompleteLoading( ex );
            ResourceLoadingContext.RemoveContext( context.ContextId );
            throw;
         }
      }

      /// <summary>
      /// Unity 2022+的异步AssetBundle加载实现
      /// </summary>
      private static AssetBundleCreateRequest LoadFromFileAsyncUnity2022( string path, uint crc, ulong offset )
      {
         try
         {
            // Unity 2022+的异步加载实现
            var request = AssetBundle.LoadFromFileAsync( path, crc, offset );
            
            if( request == null )
            {
               XuaLogger.ResourceRedirector.Warn( $"Unity 2022+ 异步AssetBundle加载失败: {path}" );
            }
            
            return request;
         }
         catch( Exception ex )
         {
            XuaLogger.ResourceRedirector.Error( ex, $"Unity 2022+ 异步AssetBundle加载异常: {path}" );
            return null;
         }
      }

      internal static AssetBundle LoadFromFileWithRandomizedCabIfRequired( string path, uint crc, ulong offset, bool confirmFileExists )
      {
         var bundle = AssetBundle.LoadFromFile( path, crc, offset );
         if( bundle == null && ( !confirmFileExists || File.Exists( path ) ) )
         {
            byte[] buffer;
            using( var stream = new FileStream( path, FileMode.Open, FileAccess.Read ) )
            {
               var fullLength = stream.Length;
               var longOffset = (long)offset;
               var lengthToRead = fullLength - longOffset;
               stream.Seek( longOffset, SeekOrigin.Begin );
               buffer = stream.ReadFully( (int)lengthToRead );
            }

            CabHelper.RandomizeCabWithAnyLength( buffer );

            XuaLogger.ResourceRedirector.Warn( $"Randomized CAB for '{path}' in order to load it because another asset bundle already uses its CAB-string. You can ignore the previous error message, but this is likely caused by two mods incorrectly using the same CAB-string." );

            return AssetBundle.LoadFromMemory( buffer );
         }
         else
         {
            return bundle;
         }
      }
   }
}
