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
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public float[] M;
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
			public double TimeInSeconds;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrFovPort
		{
			float UpTan;
			float DownTan;
			float LeftTan;
			float RightTan;
		}

		public enum ovrHmdType
		{
			None = 0,
			DK1 = 3,
			DKHD = 4,
			DK2 = 6,
			Other,
		}

		public enum ovrHmdCaps
		{
			Present = 0x0001,
			Available = 0x0002,
			Captured = 0x0004,
			ExtendDesktop = 0x0008,
			NoMirrorToWindow = 0x2000,
			DisplayOff = 0x0040,
			LowPersistence = 0x0080,
			DynamicPrediction = 0x0200,
			DirectPentile = 0x0400,
			NoVSync = 0x1000,
			Writable_Mask = 0x32F0,
			Service_Mask = 0x22F0,
		}

		public enum ovrTrackingCaps
		{
			Orientation = 0x0010,
			MagYawCorrection = 0x0020,
			Position = 0x0040,
			Idle = 0x0100,
		}

		public enum ovrDistortionCaps
		{
			Chromatic = 0x01,
			TimeWarp = 0x02,
			Vigette = 0x08,
			NoRestore = 0x10,
			FlipInput = 0x20,
			SRGB = 0x40,
			Overdrive = 0x80,
			HqDistortion = 0x100,
			LinuxDevFullscreen = 0x200,
			Computeshader = 0x400,
			ProfileNoTimewarpSpinWaits = 0x10000,
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
			ovrHmdType Type;

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
			public uint DistortionCaps;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrFovPort[] DefaultEyeFov;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrFovPort[] MaxEyeFov;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public ovrEyeType[] EyeRenderOrder;
			public ovrSizei Resolution;
			public ovrVector2i WindowPos;

			[MarshalAs(UnmanagedType.LPStr)]
			public string DisplayDeviceName;
			int DisplayId;
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
			ovrVector3f Accelerometer;
			ovrVector3f Gyro;
			ovrVector3f Magnetometer;
			float Temerature;
			float TimeInSecond;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrTrackingState
		{
			public ovrPoseStatef HeadPose;
			public ovrPosef CameraPose;
			public ovrPosef LeveledCameraPose;
			public ovrSensorData RawSensorData;
			uint StatusFlags;
			double LastVisionProcessingTime;
			double LastVisionFrameLatency;
			uint LastCameraFrameCounter;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ovrFrameTiming
		{
			float DeltaSeconds;
			double ThisFrameSeconds;
			double TimewarpPointSeconds;
			double NextFrameSeconds;
			double ScanoutMidpointseconds;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			double[] EyeScanoutSeconds;
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

		[StructLayout(LayoutKind.Sequential, Pack=8)]
		public struct ovrRenderAPIConfigHeader
		{
			ovrRenderAPIType API;
			ovrSizei BackBufferSize;
			int Multisample;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct ovrRenderAPIConfig
		{
			ovrRenderAPIConfigHeader Header;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			IntPtr[] PlatformData;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct ovrTextureHeader
		{
			ovrRenderAPIType API;
			ovrSizei TextureSize;
			ovrRecti RenderViewport;
			uint _PAD0_;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct ovrTexture
		{
			ovrTextureHeader Header;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			IntPtr[] PlatformData;
		}

		[DllImport("libovr.dll")]
		public extern static bool ovr_InitializeRenderingShim();

		[DllImport("libovr.dll")]
		public extern static bool ovr_Initialize();

		[DllImport("libovr.dll")]
		public extern static void ovr_Shutdown();

		[DllImport("libovr.dll")]
		public extern static IntPtr ovr_GetVersionString();

		[DllImport("libovr.dll")]
		public extern static int ovrHmd_Detect();

		[DllImport("libovr.dll")]
		public extern static IntPtr ovrHmd_Create(int index);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_Destroy(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static IntPtr ovrHmd_CreateDebug(ovrHmdType type);

		[DllImport("libovr.dll")]
		public extern static IntPtr ovrHmd_GetLastError(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static bool ovrHmd_AttachToWindow(IntPtr hmd, IntPtr window, IntPtr destMirrorRect, IntPtr sourceRenderTargetRect);

		[DllImport("libovr.dll")]
		public extern static uint ovrHmd_GetEnabledCaps(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_SetEnabledCaps(IntPtr hmd, uint hmdCaps);

		[DllImport("libovr.dll")]
		public extern static bool ovrHmd_ConfigureTracking(IntPtr hmd, uint supportedtrackingCaps, uint requirdTrackingCaps);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_RecenterPose(IntPtr hmd);

		[DllImport("libovr.dll")]
		public extern static ovrTrackingState ovrHmd_GetTrackingState(IntPtr hmd, double absTime);

		[DllImport("libovr.dll")]
		public extern static ovrSizei ovrHmd_GetFovTextureSize(IntPtr hmd, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel);

		[DllImport("libovr.dll")]
		public extern static ovrSizei ovrHmd_ConfigureRendering(IntPtr hmd, IntPtr apiConfig, uint distortionCaps, ovrFovPort[] eyeFovIn, [Out] ovrEyeRenderDesc[] eyeRenderDescOut);

		[DllImport("libovr.dll")]
		public extern static ovrFrameTiming ovrHmd_BeginFrame(IntPtr hmd, uint frameIndex);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_EndFrame(IntPtr hmd, ovrPosef[] renderPose, ovrTexture[] eyeTexture);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_GetEyePoses(IntPtr hmd, uint frameIndex, ovrVector3f[] hmdToEyeViewOffset, [Out] ovrPosef outEyePoses, [Out] IntPtr outHmdTrackingState);

		[DllImport("libovr.dll")]
		public extern static ovrPosef ovrHmd_GetHmdPosePerEye(IntPtr hmd, ovrEyeType eye);
	}
}
