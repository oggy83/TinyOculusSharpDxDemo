using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class represents a hmd device
	/// </summary>
	public class HmdDevice : IDisposable
	{
		#region properties

		/// <summary>
		/// get a native pointer due to call unwrapped sdk function
		/// </summary>
		public IntPtr NativePointer
		{
			get
			{
				return m_handle.Ptr;
			}
		}
		
		/// <summary>
		/// get the max rendering resolution 
		/// </summary>
		public Size Resolution
		{
			get
			{
				var resolution = m_handle.Value.Resolution;
				return new Size(resolution.w, resolution.h);
			}
		}

		/// <summary>
		/// get the ideal texture resolution for each eye 
		/// </summary>
		public Size EyeResolution
		{
			get
			{
				var eyeTypes = new LibOVR.ovrEyeType[2] { LibOVR.ovrEyeType.Left, LibOVR.ovrEyeType.Right };
				var resolutions = new Size[2];
				for (int index = 0; index < 2; ++index)
				{
					LibOVR.ovrSizei size = LibOVR.ovrHmd_GetFovTextureSize(m_handle.Ptr, eyeTypes[index], m_handle.Value.DefaultEyeFov[index], 1.0f);
					resolutions[index] = new Size(size.w, size.h);
				}

                Debug.Assert(resolutions[0] == resolutions[1], "eye resolutions may mutch");
				return resolutions[0];
			}
			
		}

		#endregion // properties

        public HmdDevice(CRef<LibOVR.ovrHmdDesc> handle)
		{
			m_handle = handle;
		}

        public void Dispose()
        {
            if (m_isDisposed)
            {
                return;
            }

            LibOVR.ovrHmd_Destroy(m_handle.Ptr);
            m_isDisposed = true;
        }

		/// <summary>
		/// set up device
		/// </summary>
		/// <param name="d3d">d3d data</param>
		/// <param name="renderTarget">render target of back buffer</param>
		public void Setup(DrawSystem.D3DData d3d, RenderTarget renderTarget)
		{
			uint hmdCaps = 
				(uint)LibOVR.ovrHmdCaps.LowPersistence
				| (uint)LibOVR.ovrHmdCaps.DynamicPrediction;
			LibOVR.ovrHmd_SetEnabledCaps(m_handle.Ptr, hmdCaps);

			uint trackingCaps = (uint)LibOVR.ovrTrackingCaps.Orientation | (uint)LibOVR.ovrTrackingCaps.MagYawCorrection
				| (uint)LibOVR.ovrTrackingCaps.Position;
			if (LibOVR.ovrHmd_ConfigureTracking(m_handle.Ptr, trackingCaps, 0) != 0)
			{
				MessageBox.Show("Failed to ConfigureTracking()");
				Application.Exit();
			}

            m_eyeDescArray = new LibOVR.ovrEyeRenderDesc[2];
            m_eyeDescArray[0] = LibOVR.ovrHmd_GetRenderDesc(m_handle.Ptr, LibOVR.ovrEyeType.Left, m_handle.Value.DefaultEyeFov[0]);
            m_eyeDescArray[1] = LibOVR.ovrHmd_GetRenderDesc(m_handle.Ptr, LibOVR.ovrEyeType.Right, m_handle.Value.DefaultEyeFov[1]);
		}

		public void BeginScene()
		{
            // update m_tmpEyePoses
            LibOVR.ovrFrameTiming ftiming = LibOVR.ovrHmd_GetFrameTiming(m_handle.Ptr, 0);
            LibOVR.ovrTrackingState hmdState = LibOVR.ovrHmd_GetTrackingState(m_handle.Ptr, ftiming.DisplayMidpointSeconds);

            var hmdToEyeOffsets = new LibOVR.ovrVector3f[] { m_eyeDescArray[0].HmdtoEyeViewOffset, m_eyeDescArray[1].HmdtoEyeViewOffset };
            LibOVR.ovr_CalcEyePoses(hmdState.HeadPose.ThePose, hmdToEyeOffsets, m_tmpEyePoses);
		}

		public Matrix[] GetEyePoses()
		{
			var result = new Matrix[2];
			for (int eyeIndex = 0; eyeIndex < 2; ++eyeIndex)
			{
				LibOVR.ovrPosef eyePose = m_tmpEyePoses[eyeIndex];

				// Posef is a right-hand system, so we convert to left-hand system
                var q = new Quaternion(-eyePose.Orientation.x, -eyePose.Orientation.y, eyePose.Orientation.z, eyePose.Orientation.w);
                var v = new Vector3(eyePose.Position.x, eyePose.Position.y, -eyePose.Position.z);
                
                var forward = Vector3.Transform(Vector3.ForwardLH, q);
                var up = Vector3.Transform(Vector3.Up, q);
                var M = Matrix.LookAtLH(v, v + forward, up);

				result[eyeIndex] = M;
			}

			return result;
		}

        public LibOVR.ovrFovPort[] GetEyeFovs()
        {
            var result = new LibOVR.ovrFovPort[2];
            for (int eyeIndex = 0; eyeIndex < 2; ++eyeIndex)
            {
                result[eyeIndex] = m_eyeDescArray[eyeIndex].Fov;
            }

            return result;
        }

		public void EndScene(HmdSwapTextureSet leftEyeSwapTextureSet, HmdSwapTextureSet rightEyeSwapTextureSet)
		{
            LibOVR.ovrLayerEyeFov layer = new LibOVR.ovrLayerEyeFov();
            layer.Header.Type = LibOVR.ovrLayerType.EyeFov;
            layer.Header.Flags = (uint)LibOVR.ovrLayerFlags.HighQuality;
            layer.ColorTexture_Left = leftEyeSwapTextureSet.Ptr;
            layer.ColorTexture_Right = rightEyeSwapTextureSet.Ptr;
            layer.Viewport_Left = ToOvrRecti(leftEyeSwapTextureSet.Resolution);
            layer.Viewport_Right = ToOvrRecti(rightEyeSwapTextureSet.Resolution);
            layer.Fov_Left = m_handle.Value.DefaultEyeFov[0];
            layer.Fov_Right = m_handle.Value.DefaultEyeFov[1];
            layer.RnderPose_Left = m_tmpEyePoses[0];
            layer.RnderPose_Right = m_tmpEyePoses[1];

            unsafe
            {
                void* layerList = &layer;
                int result = LibOVR.ovrHmd_SubmitFrame(m_handle.Ptr, 0, IntPtr.Zero, (IntPtr)(&layerList), 1);
                if (result != 0)
                {
                    // is invisible next frame @todo
                }
            }
		}

		public void ResetPose()
		{
			LibOVR.ovrHmd_RecenterPose(m_handle.Ptr);
		}

        public HmdSwapTextureSet CreateSwapTextureSet(Device device, int width, int height)
        {
            return new HmdSwapTextureSet(m_handle.Ptr, device, width, height);
        }

        public HmdMirrorTexture CreateMirrorTexture(Device device, int width, int height)
        {
            return new HmdMirrorTexture(m_handle.Ptr, device, width, height);
        }

        public void SwitchPerfDisplay()
        {
#if DEBUG
            int[] table = 
            {
                (int)LibOVR.ovrPerHudMode.Off, 
                (int)LibOVR.ovrPerHudMode.LatencyTiming, 
                (int)LibOVR.ovrPerHudMode.RenderTiming, 
            };

            m_perfModeIndex = (m_perfModeIndex + 1) % table.Length;
            LibOVR.ovrHmd_SetInt(m_handle.Ptr, "PerfHudMode", table[m_perfModeIndex]);
#endif
        }

        #region private types

        [StructLayout(LayoutKind.Sequential)]
        struct _D3D11_TEXTURE2D_DESC
        {
            public uint Width;
            public uint Height;
            public uint MipLevels;
            public uint ArraySize;
            public uint Format;
            public uint SampleDesc_Count;
            public uint SampleDesc_Quality;
            public uint Usage;
            public uint BindFlags;
            public uint CPUAccessFlags;
            public uint MiscFlags;
        };

        #endregion // private types

        #region private members

        private CRef<LibOVR.ovrHmdDesc> m_handle;
        private LibOVR.ovrEyeRenderDesc[] m_eyeDescArray = null;
        private LibOVR.ovrPosef[] m_tmpEyePoses = new LibOVR.ovrPosef[2];
        private bool m_isDisposed = false;
        private int m_perfModeIndex = 0;

        #endregion // private members

        #region private methods

        private static LibOVR.ovrSizei ToOvrSizei(Size size)
		{
			return new LibOVR.ovrSizei
			{
				w = size.Width,
				h = size.Height
			};
		}

		private static LibOVR.ovrRecti ToOvrRecti(Size size)
		{
			return new LibOVR.ovrRecti
			{
				Pos = new LibOVR.ovrVector2i() { x = 0, y = 0},
				Size = ToOvrSizei(size),
			};
		}

        #endregion // private methods

        
	}
}
