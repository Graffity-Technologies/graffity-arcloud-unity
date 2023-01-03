using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Solver;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.Graffity.ARCloud
{
    public static class ARCloudUtils
    {
        public static unsafe void XrImageToPngBytes(ref XRCpuImage image, out byte[] outBuffer)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                // Get the entire image.
                inputRect = new RectInt(0, 0, image.width, image.height),
                // Can Downsample here.
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                // Flip across the vertical axis (mirror image).
                transformation = XRCpuImage.Transformation.MirrorY
            };
            
            var size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);

            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            var texture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);

            texture.LoadRawTextureData(buffer);
            texture.Apply();
            buffer.Dispose();
            outBuffer = texture.EncodeToPNG();
        }
        
        public static async Task<ByteString> XrImageToPngByteString(XRCpuImage image)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                // Get the entire image.
                inputRect = new RectInt(0, 0, image.width, image.height),
                // Can Downsample here.
                outputDimensions = new Vector2Int(image.width, image.height),
                // outputFormat = TextureFormat.RGBA32,
                outputFormat = TextureFormat.RGBA32,
                // Flip across the vertical axis (mirror image).
                transformation = XRCpuImage.Transformation.MirrorY
            };

            var imageConvertRequest = image.ConvertAsync(conversionParams);

            while (!imageConvertRequest.status.IsDone())
                await Task.Delay(50);

            if (imageConvertRequest.status != XRCpuImage.AsyncConversionStatus.Ready)
            {
                imageConvertRequest.Dispose();
                return null;
            }
            
            // var buffer = imageConvertRequest.GetData<byte>();
            // var byteArray = ImageConversion.EncodeNativeArrayToPNG(buffer,
            //     GraphicsFormat.R8G8B8A8_SRGB, (uint)image.width, (uint)image.height, 0).ToArray();
            // var result = ByteString.CopyFrom(byteArray);
            
            var buffer = imageConvertRequest.GetData<byte>();
            
            var texture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);
            
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            var result = ByteString.CopyFrom(texture.EncodeToPNG());
            Texture2D.Destroy(texture);
            
            imageConvertRequest.Dispose();
            return result;
        }
        
        public static unsafe Texture2D XrImageToTexture(ref XRCpuImage image)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                // Get the entire image.
                inputRect = new RectInt(0, 0, image.width, image.height),
                // Can Downsample here.
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                // Flip across the vertical axis (mirror image).
                transformation = XRCpuImage.Transformation.MirrorY
            };
            
            var size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);

            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            var texture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);

            texture.LoadRawTextureData(buffer);
            texture.Apply();
            buffer.Dispose();
            return texture;
        }
    }

    
    internal static class HiResDateTime
    {
        private static long lastTimeStamp = DateTime.UtcNow.Ticks;
        public static long UtcNowTicks
        {
            get
            {
                long original, newValue;
                do
                {
                    original = lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    newValue = Math.Max(now, original + 1);
                } while (Interlocked.CompareExchange
                    (ref lastTimeStamp, newValue, original) != original);

                return newValue;
            }
        }
    }

    public static class Vector3ARCloudExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 ToVec3(this Vector3 vec)
        {
            return new Vec3()
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z
            };
        }
    }
    
    public static class QuaternionARCloudExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 ToVec4(this Quaternion vec)
        {
            return new Vec4()
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z,
                W = vec.w
            };
        }
    }
    
    public static class Vec3Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vec3 vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }
    }

    public static class Vec4Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToVec4(this Vec4 vec)
        {
            return new Quaternion((float)vec.X, (float)vec.Y, (float)vec.Z, (float)vec.W);
        }
    }
}