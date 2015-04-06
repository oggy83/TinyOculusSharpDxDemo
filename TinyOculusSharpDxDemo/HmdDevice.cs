using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class represents a hmd device
	/// </summary>
	public class HmdDevice 
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
		/// get wheather this device support window mode rendering
		/// </summary>
		/// <remarks>Extend Desktop mode can't render as window</remarks>
		public bool IsEnableWindowMode
		{
			get
			{
				return (m_handle.Value.HmdCaps & (int)LibOVR.ovrHmdCaps.ExtendDesktop) == 1 ? false : true;
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
		public Size[] EyeResolutions
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

				return resolutions;
			}
			
		}


		#endregion // properties

		public HmdDevice(CRef<LibOVR.ovrHmdDesc> handle)
		{
			m_handle = handle;
		}

		/// <summary>
		/// attach hmd to draw system
		/// </summary>
		/// <param name="d3d">d3d data</param>
		/// <param name="renderTarget">render target of back buffer</param>
		public void Attach(DrawSystem.D3DData d3d, RenderTarget renderTarget)
		{
			if (!LibOVR.ovrHmd_AttachToWindow(m_handle.Ptr, d3d.WindowHandle, IntPtr.Zero, IntPtr.Zero))
			{
				MessageBox.Show("Failed to AttachToWindow()");
				Application.Exit();
			}

			uint hmdCaps = 
				(uint)LibOVR.ovrHmdCaps.LowPersistence
				//| (uint)LibOVR.ovrHmdCaps.NoMirrorToWindow
				//| (uint)LibOVR.ovrHmdCaps.NoVSync
				| (uint)LibOVR.ovrHmdCaps.DynamicPrediction;
			LibOVR.ovrHmd_SetEnabledCaps(m_handle.Ptr, hmdCaps);

			uint trackingCaps = (uint)LibOVR.ovrTrackingCaps.Orientation | (uint)LibOVR.ovrTrackingCaps.MagYawCorrection
				| (uint)LibOVR.ovrTrackingCaps.Position;
			if (!LibOVR.ovrHmd_ConfigureTracking(m_handle.Ptr, trackingCaps, 0))
			{
				MessageBox.Show("Failed to ConfigureTracking()");
				Application.Exit();
			}

			var apiConfig = new LibOVR.ovrRenderAPIConfig();
			apiConfig.Header.API = LibOVR.ovrRenderAPIType.D3D11;
			apiConfig.Header.BackBufferSize = m_handle.Value.Resolution;
			apiConfig.Header.Multisample = 1;
			apiConfig.Device = d3d.Device.NativePointer;
			apiConfig.DeviceContext = d3d.Device.ImmediateContext.NativePointer;
			apiConfig.BackBufferRT = renderTarget.TargetView.NativePointer;
			apiConfig.SwapChain = d3d.SwapChain.NativePointer;

			uint distCaps =
					(uint)LibOVR.ovrDistortionCaps.Chromatic
					| (uint)LibOVR.ovrDistortionCaps.Vigette
					| (uint)LibOVR.ovrDistortionCaps.TimeWarp
					| (uint)LibOVR.ovrDistortionCaps.Overdrive;

			m_eyeDescArray = new LibOVR.ovrEyeRenderDesc[2];

			unsafe
			{
				if (!LibOVR.ovrHmd_ConfigureRendering(m_handle.Ptr, (IntPtr)(&apiConfig), distCaps, m_handle.Value.DefaultEyeFov, m_eyeDescArray))
				{
					MessageBox.Show("failed to ovrHmd_ConfigureRendering");
					Application.Exit();
				}
			}

		}

		public void Detach()
		{
			// do nothing
		}
		
		public void BeginScene()
		{
			LibOVR.ovrFrameTiming timing = LibOVR.ovrHmd_BeginFrame(m_handle.Ptr, 0);
			var state = new LibOVR.ovrHSWDisplayState();
			unsafe
			{
				LibOVR.ovrHmd_GetHSWDisplayState(m_handle.Ptr, (IntPtr)(&state));
			}

			if (state.Displayed && LibOVR.ovrHmd_DismissHSWDisplay(m_handle.Ptr))
			{
				// start draw model
			}

			// update poses
			var hmdToEyeOffsets = new LibOVR.ovrVector3f[] { m_eyeDescArray[0].HmdtoEyeViewOffset, m_eyeDescArray[1].HmdtoEyeViewOffset };
			LibOVR.ovrHmd_GetEyePoses(m_handle.Ptr, 0, hmdToEyeOffsets, m_tmpEyePoses, IntPtr.Zero);
		}

		public Matrix[] GetEyePoses()
		{
			var result = new Matrix[2];
			for (int eyeIndex = 0; eyeIndex < 2; ++eyeIndex)
			{
				LibOVR.ovrPosef eyePose = m_tmpEyePoses[eyeIndex];

				// Posef is a right-hand system, so we convert to left-hand system
				var q = new Quaternion(eyePose.Orientation.x, eyePose.Orientation.y, -eyePose.Orientation.z, eyePose.Orientation.w);
				var v = new Vector3(-eyePose.Position.x, eyePose.Position.y, eyePose.Position.z);
				var M = Matrix.RotationQuaternion(q) * Matrix.Translation(v);

				result[eyeIndex] = M;
			}

			return result;
		}

		public void EndScene(RenderTarget leftEyeRenderTarget, RenderTarget rightEyeRenderTarget)
		{
			var renderTargets = new RenderTarget[] { leftEyeRenderTarget, rightEyeRenderTarget };
			var eyeTextures = new LibOVR.ovrTexture[2];
			for (int index = 0; index < 2; ++index)
			{
				eyeTextures[index].Header.API = LibOVR.ovrRenderAPIType.D3D11;
				eyeTextures[index].Header.TextureSize = ToOvrSizei(renderTargets[index].Resolution);
				eyeTextures[index].Header.RenderViewport = ToOvrRecti(renderTargets[index].Resolution);// assume that use full area
				eyeTextures[index].Texture = renderTargets[index].TargetTexture.NativePointer;
				eyeTextures[index].View = renderTargets[index].ShaderResourceView.NativePointer;
			}

			LibOVR.ovrHmd_EndFrame(m_handle.Ptr, m_tmpEyePoses, eyeTextures);
		}

		public void ResetPose()
		{
			LibOVR.ovrHmd_RecenterPose(m_handle.Ptr);
		}

		
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

		private CRef<LibOVR.ovrHmdDesc> m_handle;
		private LibOVR.ovrEyeRenderDesc[] m_eyeDescArray = null;
		private LibOVR.ovrPosef[] m_tmpEyePoses = new LibOVR.ovrPosef[2];
	}
}
