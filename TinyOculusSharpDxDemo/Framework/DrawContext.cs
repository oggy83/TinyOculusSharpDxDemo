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
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_lastCommand.m_type = DrawCommandTypes.Invalid;
			m_hmd = hmd;
			
			m_modelPassCtrl = new DrawModelPassCtrl(m_d3d, m_repository);
			m_fePassCtrl = new DrawFrontendPassCtrl(m_d3d, m_repository);

			// Create render targets for each HMD eye
			var sizeArray = hmd.EyeResolutions;
			var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
			for (int index = 0; index < 2; ++index)
			{
				var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
				m_repository.AddResource(renderTarget);
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

			m_hmd.BeginScene();
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene()
		{
			var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
			m_hmd.EndScene(leftEyeRT, rightEyeRT);
	
			//int syncInterval = 0;// immediately
			//m_d3d.swapChain.Present(syncInterval, PresentFlags.None);
		}

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;
		DrawCommand m_lastCommand;
		DrawSystem.WorldData m_worldData;
		HmdDevice m_hmd = null;
		
		DrawModelPassCtrl m_modelPassCtrl = null;
		DrawFrontendPassCtrl m_fePassCtrl = null;

		#endregion // private members
		
	}
}
