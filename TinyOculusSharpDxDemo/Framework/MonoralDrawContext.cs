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
		private MonoralDrawContext(DeviceContext context, CommonInitParam initParam)
		:　base(context, initParam)
		{
			m_initParam = initParam;
		}

		public static MonoralDrawContext Create(CommonInitParam initParam)
		{
			return new MonoralDrawContext(initParam.D3D.Device.ImmediateContext, initParam);
		}

		override public void Dispose()
		{
			base.Dispose();
		}

		override public void BeginScene(DrawSystem.WorldData data)
		{
			base.BeginScene(data);

			var repository = m_initParam.Repository;
			var d3d = m_initParam.D3D;
			var renderTarget = repository.GetDefaultRenderTarget();
			_UpdateWorldParams(d3d.Device.ImmediateContext, renderTarget, Matrix.Identity);

			{
				d3d.Device.ImmediateContext.Rasterizer.State = m_initParam.RasterizerState;
				var context = d3d.Device.ImmediateContext;

				int width = renderTarget.Resolution.Width;
				int height = renderTarget.Resolution.Height;
				context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
				context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);
				context.ClearDepthStencilView(renderTarget.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
				context.ClearRenderTargetView(renderTarget.TargetView, new Color4(data.fogCol));

				// init fixed settings
				Effect effect = null;
				effect = m_initParam.Repository.FindResource<Effect>("Std");

				// set context
				context.InputAssembler.InputLayout = effect.Layout;
				context.VertexShader.Set(effect.VertexShader);
				context.PixelShader.Set(effect.PixelShader);
			}

			m_initParam.D3D.Device.ImmediateContext.VertexShader.SetConstantBuffer(1, m_initParam.WorldVtxConst);

		}

		override public void EndScene()
		{
			base.EndScene();
			int syncInterval = 0;// 0 => immediately return, 1 => vsync
			m_initParam.D3D.SwapChain.Present(syncInterval, PresentFlags.None);
		}

		#region private members

		private CommonInitParam m_initParam;

		#endregion // private members
	}
}
