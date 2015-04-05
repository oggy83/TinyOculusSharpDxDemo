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

		override public RenderTarget BeginScene(DrawSystem.WorldData data)
		{
			var repository = m_initParam.Repository;
			var d3d = m_initParam.D3D;
			var renderTarget = repository.GetDefaultRenderTarget();

			SetWorldParams(renderTarget, data);

			_UpdateWorldParams(d3d.Device.ImmediateContext, data);
			_UpdateEyeParams(d3d.Device.ImmediateContext, renderTarget, Matrix.Identity);
			_ClearRenderTarget(renderTarget);

			return renderTarget;
		}

		override public void EndScene()
		{
			int syncInterval = 0;// 0 => immediately return, 1 => vsync
			m_initParam.D3D.SwapChain.Present(syncInterval, PresentFlags.None);
		}

		#region private members

		private CommonInitParam m_initParam;

		#endregion // private members
	}
}
