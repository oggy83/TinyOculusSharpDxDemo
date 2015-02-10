using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class process DrawCommand and render.
	/// Change of render state is minimized by comparing new and last command,
	/// This means that draw command sorting is important to get high performance.
	/// </summary>
	public class DrawContext : IDisposable
	{
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, CRef<LibOVR.ovrHmdDesc> hmd)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_lastCommand.m_type = DrawCommandTypes.Invalid;
			m_hmd = hmd;
			
			m_modelPassCtrl = new DrawModelPassCtrl(m_d3d, m_repository);
			m_fePassCtrl = new DrawFrontendPassCtrl(m_d3d, m_repository);


			// Create render targets for each HMD eye
			m_eyeRenderTargetSizeArray = new LibOVR.ovrSizei[2];
			m_eyeRenderViewportArray = new LibOVR.ovrRecti[2];
			var eyeTypes = new LibOVR.ovrEyeType[2] { LibOVR.ovrEyeType.Left, LibOVR.ovrEyeType.Right };
			var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
			for (int index = 0; index < 2; ++index)
			{
				m_eyeRenderTargetSizeArray[index] = LibOVR.ovrHmd_GetFovTextureSize(hmd.Ptr, eyeTypes[index], hmd.Value.DefaultEyeFov[index], 1.0f);
				var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], m_eyeRenderTargetSizeArray[index].w, m_eyeRenderTargetSizeArray[index].h);
				m_repository.AddResource(renderTarget);

				m_eyeRenderViewportArray[index].Pos.x = 0;
				m_eyeRenderViewportArray[index].Pos.y = 0;
				m_eyeRenderViewportArray[index].Size = m_eyeRenderTargetSizeArray[index];
			}
			
			var apiConfig = new LibOVR.ovrRenderAPIConfig();
			apiConfig.Header.API = LibOVR.ovrRenderAPIType.D3D11;
			apiConfig.Header.BackBufferSize = hmd.Value.Resolution;
			apiConfig.Header.Multisample = 1;
			apiConfig.Device = m_d3d.device.NativePointer;
			apiConfig.DeviceContext = m_d3d.context.NativePointer;
			apiConfig.BackBufferRT = m_repository.GetDefaultRenderTarget().TargetView.NativePointer;
			apiConfig.SwapChain = m_d3d.swapChain.NativePointer;

			uint distCaps =
					(uint)LibOVR.ovrDistortionCaps.Chromatic
					| (uint)LibOVR.ovrDistortionCaps.Vigette
					| (uint)LibOVR.ovrDistortionCaps.TimeWarp
					| (uint)LibOVR.ovrDistortionCaps.Overdrive;
			//| (uint)LibOVR.ovrDistortionCaps.ComputeShader;

			m_eyeDescArray = new LibOVR.ovrEyeRenderDesc[2];

			unsafe
			{
				if (!LibOVR.ovrHmd_ConfigureRendering(hmd.Ptr, (IntPtr)(&apiConfig), distCaps, hmd.Value.DefaultEyeFov, m_eyeDescArray))
				{
					Debug.Fail("failed to ovrHmd_ConfigureRendering");
				}
			}

		

		}

		
		public void Dispose()
		{
			m_modelPassCtrl.Dispose();
			m_fePassCtrl.Dispose();
		}

		/// <summary>
		/// Execute DrawCommand
		/// </summary>
		/// <param name="command"></param>
		public void Draw(DrawCommand command)
		{
			if (command.m_type == DrawCommandTypes.Invalid)
			{
				// nothing
				return;
			}

			
			// Select Render Target
			IDrawPassCtrl passCtrl = null;
			switch (command.m_type)
			{
				case DrawCommandTypes.DrawModel:
					Debug.Assert(false, "fix temporary code!!(85line)");
					passCtrl = m_modelPassCtrl;
					break;

				case DrawCommandTypes.DrawText:
					passCtrl = m_fePassCtrl;
					break;
			}

			var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
			foreach (var renderTarget in renderTargets)
			{
				if (m_lastCommand.m_type != command.m_type)
				{
					//m_modelPassCtrl.StartPath();
					passCtrl.StartPass(renderTarget);
				}

				// Draw
				passCtrl.ExecuteCommand(command, m_worldData);
			}
		
			m_lastCommand = command;
		}

		/// <summary>
		/// Begin scene rendering
		/// </summary>
		/// <param name="data">world data</param>
		public void BeginScene(DrawSystem.WorldData data)
		{
			m_lastCommand = new DrawCommand();
			m_worldData = data;

			var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
			m_modelPassCtrl.StartPass(leftEyeRT);// @todo temporary code
			m_modelPassCtrl.StartPass(rightEyeRT);// @todo temporary code

			LibOVR.ovrFrameTiming timing = LibOVR.ovrHmd_BeginFrame(m_hmd.Ptr, 0);

			var state = new LibOVR.ovrHSWDisplayState();
			unsafe
			{
				LibOVR.ovrHmd_GetHSWDisplayState(m_hmd.Ptr, (IntPtr)(&state));
			}
			if (state.Displayed && LibOVR.ovrHmd_DismissHSWDisplay(m_hmd.Ptr))
			{
				// start draw model
			}
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene()
		{
			var hmdToEyeOffsets = new LibOVR.ovrVector3f[] { m_eyeDescArray[0].HmdtoEyeViewOffset, m_eyeDescArray[1].HmdtoEyeViewOffset }; 
			var outEyePoses = new LibOVR.ovrPosef[2];
			LibOVR.ovrHmd_GetEyePoses(m_hmd.Ptr, 0, hmdToEyeOffsets, outEyePoses, IntPtr.Zero);

			var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");

			var eyeTextures = new LibOVR.ovrTexture[2];
			eyeTextures[0].Header.API = LibOVR.ovrRenderAPIType.D3D11;
			eyeTextures[0].Header.TextureSize = m_eyeRenderTargetSizeArray[0];
			eyeTextures[0].Header.RenderViewport = m_eyeRenderViewportArray[0];
			eyeTextures[0].Texture = leftEyeRT.TargetTexture.NativePointer;
			eyeTextures[0].View = leftEyeRT.ShaderResourceView.NativePointer;
			eyeTextures[1].Header.API = LibOVR.ovrRenderAPIType.D3D11;
			eyeTextures[1].Header.TextureSize = m_eyeRenderTargetSizeArray[1];
			eyeTextures[1].Header.RenderViewport = m_eyeRenderViewportArray[1];
			eyeTextures[1].Texture = rightEyeRT.TargetTexture.NativePointer;
			eyeTextures[1].View = rightEyeRT.ShaderResourceView.NativePointer;

			LibOVR.ovrHmd_EndFrame(m_hmd.Ptr, outEyePoses, eyeTextures);

			//int syncInterval = 0;// immediately
			//m_d3d.swapChain.Present(syncInterval, PresentFlags.None);
		}

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;
		DrawCommand m_lastCommand;
		DrawSystem.WorldData m_worldData;
		CRef<LibOVR.ovrHmdDesc> m_hmd = null;
		LibOVR.ovrEyeRenderDesc[] m_eyeDescArray= null;
		LibOVR.ovrSizei[] m_eyeRenderTargetSizeArray = null;
		LibOVR.ovrRecti[] m_eyeRenderViewportArray = null;

		DrawModelPassCtrl m_modelPassCtrl = null;
		DrawFrontendPassCtrl m_fePassCtrl = null;

		#endregion // private members
		
	}
}
