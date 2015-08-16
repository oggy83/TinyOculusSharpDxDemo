using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyOculusSharpDxDemo
{
	public static class LibOVR
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct ovrVector2i
		{
			public int x, y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrSizei
		{
			public int w, h;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrRecti
		{
			public ovrVector2i Pos;
			public ovrSizei Size;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrQuatf
		{
			public float x, y, z, w;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrVector2f
		{
			public float x, y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrVector3f
		{
			public float x, y, z;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrMatrix4f
		{
            public float M11;
            public float M12;
            public float M13;
            public float M14;
            public float M21;
            public float M22;
            public float M23;
            public float M24;
            public float M31;
            public float M32;
            public float M33;
            public float M34;
            public float M41;
            public float M42;
            public float M43;
            public float M44;

		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrPosef
		{
			public ovrQuatf Orientation;
			public ovrVector3f Position;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrPoseStatef
		{
			public ovrPosef ThePose;
			public ovrVector3f AngularVelocity;
			public ovrVector3f LinearVelocity;
			public ovrVector3f Angularacceleration;
			public ovrVector3f LinearAcceleration;
            private float _Padding;
			public double TimeInSeconds;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrFovPort
		{
			public float UpTan;
			public float DownTan;
			public float LeftTan;
			public float RightTan;
		}

		public enum ovrHmdType
		{
			None = 0,
			DK1 = 3,
			DKHD = 4,
			DK2 = 6,
            CB = 8,
			Other = 9,
		}

		public enum ovrHmdCaps
		{
            DebugDevice = 0x0010,
			LowPersistence = 0x0080,
			DynamicPrediction = 0x0200,
		}

		public enum ovrTrackingCaps
		{
			Orientation = 0x0010,
			MagYawCorrection = 0x0020,
			Position = 0x0040,
			Idle = 0x0100,
		}

		public enum ovrEyeType
		{
			Left = 0,
			Right = 1,

			Count = 2
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct ovrHmdDesc
		{
			public IntPtr Handle;
			public ovrHmdType Type;
            private int _Padding;

			[MarshalAs(UnmanagedType.LPStr)]
			public string ProductName;
			[MarshalAs(UnmanagedType.LPStr)]
			public string Manufacturer;

			short VendorId;
			short ProductId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
			public string SerialNumber;

			public short FirmwareMajor;
			public short FirmwareMinor;

			public float CameraFrustumHFovInRadians;
			public float CameraFrustumVFovInRadians;
			public float CameraFrustumNearZInMeters;
			public float CameraFrustumFarZInMeters;

			public uint HmdCaps;
			public uint TrackingCaps;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrFovPort[] DefaultEyeFov;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrFovPort[] MaxEyeFov;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrEyeType[] EyeRenderOrder;
			public ovrSizei Resolution;
		}

		public enum ovrStatusBits
		{
			OrientationTracked = 0x0001,
			PositionTracked = 0x0002,
			CameraPoseTracked = 0x0004,
			PositionConnected = 0x0020,
			HmdConnected = 0x0080,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrSensorData
		{
			public ovrVector3f Accelerometer;
			public ovrVector3f Gyro;
			public ovrVector3f Magnetometer;
			public float Temerature;
			public float TimeInSecond;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrTrackingState
		{
			public ovrPoseStatef HeadPose;
			public ovrPosef CameraPose;
			public ovrPosef LeveledCameraPose;
			public ovrSensorData RawSensorData;
			public uint StatusFlags;
			public uint LastCameraFrameCounter;
            private int _Padding;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrFrameTiming
		{
            public double DisplayMidpointSeconds;
            public double FrameIntervalSeconds;
            public uint AppFrameIndex;
            public uint DisplayFrameIndex;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrEyeRenderDesc
		{
			public ovrEyeType Eye;
			public ovrFovPort Fov;
			public ovrRecti DistortedViewport;
			public ovrVector2f PixelsPerTanAngleAtCenter;
			public ovrVector3f HmdtoEyeViewOffset;
		}

		public enum ovrRenderAPIType
		{
			None,
			OpenGL,
			Android_GLES,
			D3D9,
			D3D10,
			D3D11,
			
			Count,
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct ovrTextureHeader
		{
			public ovrRenderAPIType API;
			public ovrSizei TextureSize;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct ovrTexture
		{
			public ovrTextureHeader Header;
            uint _Padding;

			// DirectX11 config params
			public IntPtr Texture;
			public IntPtr View;
			IntPtr _PAD0_;
			IntPtr _PAD1_;
			IntPtr _PAD2_;
			IntPtr _PAD3_;
			IntPtr _PAD4_;
			IntPtr _PAD5_;
		}

        public enum ovrInitFlags
        {
            Debug = 0x00000001,
            ServerOptional = 0x00000002,
            RequestVersion = 0x00000004,
            ForceNoDebug = 0x00000008,
        }

        public enum ovrLogLevel
        {
            Debug = 0,
            Info = 1,
            Error = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrInitParams
        {
            public uint Flags;
            public uint RequestedMinorVersion;
            public IntPtr LogCallback;
            public uint ConnectionTimeoutMS;
            private uint _Padding;
        }

        public enum ovrProjectionModifier
        {
            /// <summary>
            /// default setting
            /// </summary>
            /// <remarks>
            /// * Left-handed
            /// * Near depth values stored in the depth buffer are smaller than far depth values
            /// * Both near and far are explicitly defined
            /// * With a clipping range that is (0 to w)
            /// </remarks>
            None = 0x00,

            RightHanded = 0x01,
            FarLessThanNear = 0x02,
            FarClipAtInfinity = 0x04,
            ClipRangeOpenGL = 0x08,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrSwapTextureSet
        {
            /// <summary>
            /// pointer of array of ovrTextures
            /// </summary>
            public IntPtr Textures;

            public int TextureCount;

            /// <summary>
            /// index specifies which of the Textures will be used by ovrHmd_SubmitFrame()
            /// </summary>
            public int CurrentIndex;
        }

        public enum ovrLayerType
        {
            Disabled = 0,
            EyeFov = 1,
            EyeFovDepth = 2,
            QuadInWorld = 3,
            QuadHeadLocked = 4,
            Direct = 6,
        }

        public enum ovrLayerFlags
        {
            HighQuality = 1,
            TextureOriginAtBottomLeft = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrLayerHeader
        {
            public ovrLayerType Type;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ovrLayerEyeFov
        {
            public ovrLayerHeader Header;

            /// <summary>
            /// pointer of ovrSwapTextureSet for left eye
            /// </summary>
            public IntPtr ColorTexture_Left;

            /// <summary>
            /// pointer of ovrSwapTextureSet for right eye
            /// </summary>
            public IntPtr ColorTexture_Right;

            public ovrRecti Viewport_Left;
            public ovrRecti Viewport_Right;
            public ovrFovPort Fov_Left;
            public ovrFovPort Fov_Right;
            public ovrPosef RnderPose_Left;
            public ovrPosef RnderPose_Right;
        }
 
        /*
        [DllImport("libovr.dll")]
        public extern static bool ovr_InitializeRenderingShimVersion(int requestedMinorVersion);

		[DllImport("libovr.dll")]
		public extern static bool ovr_InitializeRenderingShim();
        */

		[DllImport("libovr.dll")]
		public extern static int ovr_Initialize(IntPtr initParams);

		[DllImport("libovr.dll")]
		public extern static void ovr_Shutdown();

		[DllImport("libovr.dll")]
		public extern static IntPtr ovr_GetVersionString();

		[DllImport("libovr.dll")]
		public extern static int ovrHmd_Detect();

		[DllImport("libovr.dll")]
		public extern static int ovrHmd_Create(int index, out IntPtr outHmd);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_Destroy(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static int ovrHmd_CreateDebug(ovrHmdType type, out IntPtr outHmd);

        // change
		//[DllImport("libovr.dll")]
		//public extern static IntPtr ovrHmd_GetLastError(IntPtr hmd);

        //Improved error reporting, including adding the ovrResult type. Some API functions were changed to return ovrResult. ovrHmd_GetLastError was replaced with ovr_GetLastErrorInfo.

		[DllImport("libovr.dll")]
		public extern static uint ovrHmd_GetEnabledCaps(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_SetEnabledCaps(IntPtr hmd, uint hmdCaps);

		[DllImport("libovr.dll")]
		public extern static int ovrHmd_ConfigureTracking(IntPtr hmd, uint supportedtrackingCaps, uint requirdTrackingCaps);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_RecenterPose(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static ovrTrackingState ovrHmd_GetTrackingState(IntPtr hmd, double absTime);

		[DllImport("libovr.dll")]
		public extern static ovrSizei ovrHmd_GetFovTextureSize(IntPtr hmd, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel);

        [DllImport("libovr.dll")]
        public extern static int ovrHmd_CreateSwapTextureSetD3D11(IntPtr hmd, IntPtr device, IntPtr desc, out IntPtr outTextureSet);

        [DllImport("libovr.dll")]
        public extern static int ovrHmd_DestroySwapTextureSet(IntPtr hmd, IntPtr textureSet);

        //OVR_PUBLIC_FUNCTION(ovrResult) ovrHmd_CreateMirrorTextureD3D11(ovrHmd hmd,
        //                                                       ID3D11Device* device,
        //                                                       const D3D11_TEXTURE2D_DESC* desc,
        //                                                       ovrTexture** outMirrorTexture);

        //OVR_PUBLIC_FUNCTION(void) ovrHmd_DestroyMirrorTexture(ovrHmd hmd, ovrTexture* mirrorTexture);

        /*
        [DllImport("libovr.dll")]
		public extern static bool ovrHmd_ConfigureRendering(IntPtr hmd, IntPtr apiConfig, uint distortionCaps, ovrFovPort[] eyeFovIn, [Out] ovrEyeRenderDesc[] eyeRenderDescOut);
        */

        [DllImport("libovr.dll")]
        public extern static ovrEyeRenderDesc ovrHmd_GetRenderDesc(IntPtr hmd, ovrEyeType eyeType, ovrFovPort fov);

        [DllImport("libovr.dll")]
        public extern static int ovrHmd_SubmitFrame(IntPtr hmd, uint frameIndex, IntPtr viewScaleDesc, IntPtr layerPtrList, uint layerCount);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_GetEyePoses(IntPtr hmd, uint frameIndex, ovrVector3f[] hmdToEyeViewOffset, [Out] ovrPosef[] outEyePoses, [Out] IntPtr outHmdTrackingState);

		//[DllImport("libovr.dll")]
		//public extern static ovrPosef ovrHmd_GetHmdPosePerEye(IntPtr hmd, ovrEyeType eye);

		

        [DllImport("libovr.dll")]
        public extern static ovrMatrix4f ovrMatrix4f_Projection(ovrFovPort fov, float znear, float zfar, uint projectionModFlags);

	}
}
