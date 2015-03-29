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
	public class StereoDrawContext : DrawContext
	{
		public StereoDrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd)
		: base(d3d, repository)
		{
			m_hmd = hmd;
			m_deferredContext = new DeviceContext(m_d3d.Device);

			// Create render targets for each HMD eye
			var sizeArray = hmd.EyeResolutions;
			var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
			for (int index = 0; index < 2; ++index)
			{
				var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
				m_repository.AddResource(renderTarget);
			}

			
		}

		override public void Dispose() 
		{
			m_deferredContext.Dispose();
			base.Dispose();
		}

		override public void BeginScene(DrawSystem.WorldData data)
		{
			base.BeginScene(data);
			m_hmd.BeginScene();

			var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
			var eyeOffset = m_hmd.GetEyePoses();

			// set right eye settings
			_UpdateWorldParams(m_d3d.Device.ImmediateContext, renderTargets[0], eyeOffset[1]);

			// make command list by deferred context
			{
				m_deferredContext.Rasterizer.State = m_rasterizerState;
				
				var context = m_deferredContext;
				var renderTarget = renderTargets[0];

				int width = renderTarget.Resolution.Width;
				int height = renderTarget.Resolution.Height;
				context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
				context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);
				context.ClearDepthStencilView(renderTarget.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
				context.ClearRenderTargetView(renderTarget.TargetView, new Color4(data.fogCol));

				// init fixed settings
				Effect effect = null;
				effect = m_repository.FindResource<Effect>("Std");

				// set context
				context.InputAssembler.InputLayout = effect.Layout;
				context.VertexShader.Set(effect.VertexShader);
				context.PixelShader.Set(effect.PixelShader);
			}

			m_deferredContext.VertexShader.SetConstantBuffer(1, m_worldVtxConst);
		}

		override public void EndScene()
		{
			base.EndScene();
			var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
			var eyeOffset = m_hmd.GetEyePoses();

			var commandList = m_deferredContext.FinishCommandList(true);

			// render right eye image to left eye buffer
			m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);

			// copy left eye buffer to right eye buffer
			m_d3d.Device.ImmediateContext.CopyResource(renderTargets[0].TargetTexture, renderTargets[1].TargetTexture);

			// set left eye settings
			_UpdateWorldParams(m_d3d.Device.ImmediateContext, renderTargets[0], eyeOffset[0]);

			// render left eye image to left eye buffer
			m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);

			commandList.Dispose();

			var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
			m_hmd.EndScene(leftEyeRT, rightEyeRT);
		}

		override protected DeviceContext _GetContext() 
		{
			return m_deferredContext;
		}

		#region private members

		private DeviceContext m_deferredContext = null;
		private HmdDevice m_hmd = null;

		#endregion // private members
	}
}
