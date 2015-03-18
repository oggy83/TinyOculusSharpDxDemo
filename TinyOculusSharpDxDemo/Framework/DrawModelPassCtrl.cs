using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public class DrawModelPassCtrl : IDrawPassCtrl
	{
		private DeviceContext m_context;
		public DeviceContext Context
		{
			get
			{
				return m_context;
			}
		}

		public DrawModelPassCtrl(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		{
			m_d3d = d3d;
			m_repository = repository;
			
			// Create object
			m_rasterizerState = new RasterizerState(m_d3d.Device, new RasterizerStateDescription()
			{
				CullMode = CullMode.Back,
				FillMode = FillMode.Solid,
				IsAntialiasedLineEnabled = false,	// we do not use wireframe 
				IsDepthClipEnabled = true,
				IsMultisampleEnabled = false,
			});

			m_depthStencilState = new DepthStencilState(m_d3d.Device, new DepthStencilStateDescription()
			{
				IsDepthEnabled = true,
				DepthComparison = Comparison.Less,
				DepthWriteMask = DepthWriteMask.All,
			});

			var blendDesc = new BlendStateDescription()
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false,
			};
			blendDesc.RenderTarget[0] = new RenderTargetBlendDescription(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha, BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, ColorWriteMaskFlags.All);
			m_blendState = new BlendState(m_d3d.Device, blendDesc);

			_RegisterStandardSetting();

			

			// Init settings
			m_d3d.Device.QueryInterface<Device1>().MaximumFrameLatency = 1;
		}

		public void Dispose()
		{
			m_blendState.Dispose();
			m_depthStencilState.Dispose();
			m_rasterizerState.Dispose();
		}

		/// <summary>
		/// setup render pass
		/// </summary>
		public void StartPass(DeviceContext context, RenderTarget renderTarget)
		{
			m_context = context;
			context.Rasterizer.State = m_rasterizerState;
			context.OutputMerger.DepthStencilState = m_depthStencilState;
			context.OutputMerger.BlendState = m_blendState;

			int width = renderTarget.Resolution.Width;
			int height = renderTarget.Resolution.Height;
			context.Rasterizer.SetViewport(new Viewport(0, 0, width, height, 0.0f, 1.0f));
			context.OutputMerger.SetTargets(renderTarget.TargetView);// disable z-buffer
			//context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);
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

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;
		

		RasterizerState m_rasterizerState = null;
		DepthStencilState m_depthStencilState = null;
		BlendState m_blendState = null;

		#endregion // private members

		#region private methods

		private void _RegisterStandardSetting()
		{
			
			var shader = new Effect(
				"Std",
				m_d3d,
 				new InputElement[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
					new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
					new InputElement("NORMAL", 0, Format.R32G32B32_Float, 40, 0),
				},
				"Shader/VS_Std.fx",
				"Shader/PS_Std.fx");

			m_repository.AddResource(shader);
		}

		#endregion // private methods

	}
}
