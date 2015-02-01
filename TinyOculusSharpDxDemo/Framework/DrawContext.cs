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

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class process DrawCommand and render.
	/// Change of render state is minimized by comparing new and last command,
	/// This means that draw command sorting is important to get high performance.
	/// </summary>
	public class DrawContext : IDisposable
	{
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_lastCommand.m_type = DrawCommandTypes.Invalid;
			
			m_modelPassCtrl = new DrawModelPassCtrl(m_d3d, m_repository);
			m_fePassCtrl = new DrawFrontendPassCtrl(m_d3d, m_repository);
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

			if (m_lastCommand.m_type != command.m_type)
			{
				//m_modelPassCtrl.StartPath();
				passCtrl.StartPass();	
			}
			
			// Draw
			passCtrl.ExecuteCommand(command, m_worldData);
		
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
			m_modelPassCtrl.StartPass();// @todo temporary code
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene()
		{
			int syncInterval = 0;// immediately
			m_d3d.swapChain.Present(syncInterval, PresentFlags.None);
		}

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;
		DrawCommand m_lastCommand;
		DrawSystem.WorldData m_worldData;

		DrawModelPassCtrl m_modelPassCtrl = null;
		DrawFrontendPassCtrl m_fePassCtrl = null;

		#endregion // private members
		
	}
}
