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
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, bool bStereoRendering)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_hmd = hmd;
			m_bStereoRendering = bStereoRendering;
			
			m_modelPassCtrl = new DrawModelPassCtrl(m_d3d, m_repository);

			if (bStereoRendering)
			{
				// Create render targets for each HMD eye
				var sizeArray = hmd.EyeResolutions;
				var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
				for (int index = 0; index < 2; ++index)
				{
					var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
					m_repository.AddResource(renderTarget);
				}
			}
		}

		
		public void Dispose()
		{
			m_modelPassCtrl.Dispose();
		}

		/// <summary>
		/// Execute DrawCommand
		/// </summary>
		/// <param name="commandBuffer">sorted draw command list</param>
		public void Draw(DrawCommandBuffer commandBuffer)
		{
			RenderTarget[] renderTargets;
			Matrix[] eyeOffset;
			if (m_bStereoRendering)
			{
				renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
				eyeOffset = m_hmd.GetEyePoses();
			}
			else
			{
				renderTargets = new[] { m_repository.GetDefaultRenderTarget() };
				eyeOffset = new[] { Matrix.Identity };
			}

			for (int index = 0; index < renderTargets.Count(); ++index)
			{
				var renderTarget = renderTargets[index];
				m_modelPassCtrl.StartPass(renderTarget);

				DrawSystem.WorldData tmpWorldData = m_worldData;
				tmpWorldData.camera = tmpWorldData.camera * eyeOffset[index];

				foreach (var command in commandBuffer.Commands)
				{
					m_modelPassCtrl.ExecuteCommand(command, tmpWorldData);
				}
			}
		}

		/// <summary>
		/// Begin scene rendering
		/// </summary>
		/// <param name="data">world data</param>
		public void BeginScene(DrawSystem.WorldData data)
		{
			m_worldData = data;

			if (m_bStereoRendering)
			{
				m_hmd.BeginScene();
			}
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene()
		{
			if (m_bStereoRendering)
			{
				var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
				var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
				m_hmd.EndScene(leftEyeRT, rightEyeRT);
			}
			else
			{
				int syncInterval = 0;// 0 => immediately return, 1 => vsync
				m_d3d.swapChain.Present(syncInterval, PresentFlags.None);
			}
		}

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private DrawSystem.WorldData m_worldData;
		private HmdDevice m_hmd = null;
		private bool m_bStereoRendering;
		
		private DrawModelPassCtrl m_modelPassCtrl = null;

		#endregion // private members
		
	}
}
