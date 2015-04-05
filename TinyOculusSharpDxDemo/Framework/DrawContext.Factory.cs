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
	public partial class DrawContext
	{
		public class Factory : IDisposable
		{
			public Factory(DrawSystem.D3DData d3d, DrawResourceRepository repository)
			{
				m_initParam.D3D = d3d;
				m_initParam.Repository = repository;

				m_initParam.WorldVtxConst = DrawUtil.CreateConstantBuffer<_WorldVertexShaderConst>(d3d, 1);
				m_initParam.WorldPixConst = DrawUtil.CreateConstantBuffer<_WorldPixelShaderConst>(d3d, 1);

				m_initParam.RasterizerState = new RasterizerState(d3d.Device, new RasterizerStateDescription()
				{
					CullMode = CullMode.Back,
					FillMode = FillMode.Solid,
					IsAntialiasedLineEnabled = false,	// we do not use wireframe 
					IsDepthClipEnabled = true,
					IsMultisampleEnabled = false,
				});

				var renderModes = new[] { DrawSystem.RenderMode.Opaque, DrawSystem.RenderMode.Transparency };
				m_initParam.BlendStates = new BlendState[renderModes.Length];
				foreach (DrawSystem.RenderMode mode in renderModes)
				{
					int index = (int)mode;
					m_initParam.BlendStates[index] = _CreateBlendState(d3d, mode);
				}
				m_initParam.DepthStencilStates = new DepthStencilState[renderModes.Length];
				foreach (DrawSystem.RenderMode mode in renderModes)
				{
					int index = (int)mode;
					m_initParam.DepthStencilStates[index] = _CreateDepthStencilState(d3d, mode);
				}

				{
					var shader = new Effect(
					"Std",
					d3d,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
						new InputElement("NORMAL", 0, Format.R32G32B32_Float, 24, 0),
					},
					"Shader/VS_Std.fx",
					"Shader/PS_Std.fx");

					repository.AddResource(shader);
				}
			}

			virtual public void Dispose()
			{
				m_initParam.WorldPixConst.Dispose();
				m_initParam.WorldVtxConst.Dispose();
				foreach (var state in m_initParam.BlendStates)
				{
					state.Dispose();
				}
				foreach (var state in m_initParam.DepthStencilStates)
				{
					state.Dispose();
				}
				m_initParam.RasterizerState.Dispose();
			}

			public CommonInitParam GetInitParam()
			{
				return m_initParam;
			}

			#region private members

			private CommonInitParam m_initParam;

			#endregion // private members

			#region // private methods

			private static BlendState _CreateBlendState(DrawSystem.D3DData d3d, DrawSystem.RenderMode mode)
			{
				switch (mode)
				{
					case DrawSystem.RenderMode.Opaque:
						{
							var blendDesc = BlendStateDescription.Default();
							return new BlendState(d3d.Device, blendDesc);
						}

					case DrawSystem.RenderMode.Transparency:
						{
							var blendDesc = new BlendStateDescription()
							{
								AlphaToCoverageEnable = false,
								IndependentBlendEnable = false,
							};
							blendDesc.RenderTarget[0] = new RenderTargetBlendDescription(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha, BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, ColorWriteMaskFlags.All);
							return new BlendState(d3d.Device, blendDesc);
						}
					default:
						return null;
				}

			}

			private static DepthStencilState _CreateDepthStencilState(DrawSystem.D3DData d3d, DrawSystem.RenderMode mode)
			{
				switch (mode)
				{
					case DrawSystem.RenderMode.Opaque:
						{
							return new DepthStencilState(d3d.Device, new DepthStencilStateDescription()
							{
								BackFace = new DepthStencilOperationDescription()
								{
									// Stencil operations if pixel is back-facing
									DepthFailOperation = StencilOperation.Decrement,
									FailOperation = StencilOperation.Keep,
									PassOperation = StencilOperation.Keep,
									Comparison = SharpDX.Direct3D11.Comparison.Always,
								},
								FrontFace = new DepthStencilOperationDescription()
								{
									// Stencil operations if pixel is front-facing
									DepthFailOperation = StencilOperation.Increment,
									FailOperation = StencilOperation.Keep,
									PassOperation = StencilOperation.Keep,
									Comparison = SharpDX.Direct3D11.Comparison.Always,
								},
								IsDepthEnabled = true,
								IsStencilEnabled = false,
								StencilReadMask = 0xff,
								StencilWriteMask = 0xff,
								DepthComparison = Comparison.Less,
								DepthWriteMask = DepthWriteMask.All,
							});
						}

					case DrawSystem.RenderMode.Transparency:
						{
							return new DepthStencilState(d3d.Device, new DepthStencilStateDescription()
							{
								BackFace = new DepthStencilOperationDescription()
								{
									// Stencil operations if pixel is back-facing
									DepthFailOperation = StencilOperation.Decrement,
									FailOperation = StencilOperation.Keep,
									PassOperation = StencilOperation.Keep,
									Comparison = SharpDX.Direct3D11.Comparison.Always,
								},
								FrontFace = new DepthStencilOperationDescription()
								{
									// Stencil operations if pixel is front-facing
									DepthFailOperation = StencilOperation.Increment,
									FailOperation = StencilOperation.Keep,
									PassOperation = StencilOperation.Keep,
									Comparison = SharpDX.Direct3D11.Comparison.Always,
								},
								IsDepthEnabled = false,	// disable depth
								IsStencilEnabled = false,
								StencilReadMask = 0xff,
								StencilWriteMask = 0xff,
								DepthComparison = Comparison.Less,
								DepthWriteMask = DepthWriteMask.All,
							});
						}
					default:
						return null;
				}
			}

			#endregion // private methods
		}
	}
}
