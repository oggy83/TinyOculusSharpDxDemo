using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace TinyOculusSharpDxDemo
{
	public class MonoralDrawContext : DrawContext
	{
		public MonoralDrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		:　base(d3d, repository)
		{
			// nothing
		}

		override public void Dispose()
		{
			base.Dispose();
		}

		override public void BeginScene(DrawSystem.WorldData data)
		{
			base.BeginScene(data);
			var renderTarget = m_repository.GetDefaultRenderTarget();
			_UpdateWorldParams(m_d3d.Device.ImmediateContext, renderTarget, Matrix.Identity);

			{
				m_d3d.Device.ImmediateContext.Rasterizer.State = m_rasterizerState;
				var context = m_d3d.Device.ImmediateContext;

				int width = renderTarget.Resolution.Width;
				int height = renderTarget.Resolution.Height;
				context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
				context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);
				context.ClearDepthStencilView(renderTarget.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
				context.ClearRenderTargetView(renderTarget.TargetView, new Color4(0.3f, 0.5f, 0.8f, 1.0f));

				// init fixed settings
				Effect effect = null;
				effect = m_repository.FindResource<Effect>("Std");

				// set context
				context.InputAssembler.InputLayout = effect.Layout;
				context.VertexShader.Set(effect.VertexShader);
				context.PixelShader.Set(effect.PixelShader);
			}

			m_d3d.Device.ImmediateContext.VertexShader.SetConstantBuffer(1, m_worldVtxConst);

		}

		override public void EndScene()
		{
			base.EndScene();
			int syncInterval = 0;// 0 => immediately return, 1 => vsync
			m_d3d.SwapChain.Present(syncInterval, PresentFlags.None);
		}

		override protected DeviceContext _GetContext()
		{
			return m_d3d.Device.ImmediateContext;
		}
	}
}
