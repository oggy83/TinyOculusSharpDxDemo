using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public class DrawFrontendPassCtrl : IDrawPassCtrl
	{
		public DrawFrontendPassCtrl(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		{
			m_d3d = d3d;
			m_repository = repository;
		}

		public void Dispose()
		{
			// nothing
		}

		public void StartPass()
		{
			var renderTarget = m_repository.GetDefaultRenderTarget();
			var context = m_d3d.context;

			// Init a render target
			context.Rasterizer.SetViewport(new Viewport(0, 0, 800, 600, 0.0f, 1.0f));// temp
			context.OutputMerger.SetTargets(renderTarget.TargetView);
		}

		public void ExecuteCommand(DrawCommand command, DrawSystem.WorldData world)
		{
			var commandData = command.GetDrawTextData();

			var textSys = TextSystem.GetInstance();
			textSys.DrawText(commandData.m_text);
		}

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;

		#endregion // private members
	}
}
