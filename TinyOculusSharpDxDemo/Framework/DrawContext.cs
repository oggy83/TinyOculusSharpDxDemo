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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class process DrawCommand and render.
	/// Change of render state is minimized by comparing new and last command,
	/// This means that draw command sorting is important to get high performance.
	/// </summary>
	public abstract partial class DrawContext : IDisposable, IDrawContext
	{
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		{
			m_d3d = d3d;
			m_repository = repository;

			m_mainVtxConst = DrawUtil.CreateConstantBuffer<_MainVertexShaderConst>(m_d3d, m_maxInstanceCount);
			m_mainPixConst = DrawUtil.CreateConstantBuffer<_MainPixelShaderConst>(m_d3d, m_maxInstanceCount);
			m_worldVtxConst = DrawUtil.CreateConstantBuffer<_WorldVertexShaderConst>(m_d3d, 1);
			m_worldPixConst = DrawUtil.CreateConstantBuffer<_WorldPixelShaderConst>(m_d3d, 1);

			// Create object
			m_rasterizerState = new RasterizerState(m_d3d.Device, new RasterizerStateDescription()
			{
				CullMode = CullMode.Back,
				FillMode = FillMode.Solid,
				IsAntialiasedLineEnabled = false,	// we do not use wireframe 
				IsDepthClipEnabled = true,
				IsMultisampleEnabled = false,
			});

			var renderModes = new[] { DrawSystem.RenderMode.Opaque, DrawSystem.RenderMode.Transparency };
			m_blendStateArray = new BlendState[renderModes.Length];
			foreach (DrawSystem.RenderMode mode in renderModes)
			{
				int index = (int)mode;
				m_blendStateArray[index] = _CreateBlendState(d3d, mode);
			}
			m_depthStencilStateArray = new DepthStencilState[renderModes.Length];
			foreach (DrawSystem.RenderMode mode in renderModes)
			{
				int index = (int)mode;
				m_depthStencilStateArray[index] = _CreateDepthStencilState(d3d, mode);
			}

			_RegisterStandardSetting();

			m_instanceMainVtxConst = new _MainVertexShaderConst[m_maxInstanceCount];
			m_instanceMainPixConst = new _MainPixelShaderConst[m_maxInstanceCount];
		}

		virtual public void Dispose()
		{
			m_worldPixConst.Dispose();
			m_worldVtxConst.Dispose();
			m_mainPixConst.Dispose();
			m_mainVtxConst.Dispose();
			foreach (var state in m_blendStateArray)
			{
				state.Dispose();
			}
			foreach (var state in m_depthStencilStateArray)
			{
				state.Dispose();
			}
			m_rasterizerState.Dispose();
		}

		virtual public void BeginScene(DrawSystem.WorldData data)
		{
			m_worldData = data;
			var context = _GetContext();

			// init pixel shader resource
			var pdata = new _WorldPixelShaderConst()
			{
				ambientCol = new Color4(m_worldData.ambientCol),
				light1Col = new Color4(m_worldData.dirLight.Color),
				cameraPos = new Vector4(m_worldData.camera.TranslationVector, 1.0f),
				light1Dir = new Vector4(m_worldData.dirLight.Direction, 0.0f),
			};
			context.UpdateSubresource(ref pdata, m_worldPixConst);
			
			// bind shader 
			context.VertexShader.SetConstantBuffer(0, m_mainVtxConst);
			context.VertexShader.SetConstantBuffer(1, m_worldVtxConst);
			context.PixelShader.SetConstantBuffer(0, m_mainPixConst);
			context.PixelShader.SetConstantBuffer(1, m_worldPixConst);

			m_drawCallCount = 0;
			m_nextInstanceIndex = 0;
		}

		virtual public void EndScene()
		{
			// nothing
		}


		protected void _UpdateWorldParams(DeviceContext context, RenderTarget renderTarget, Matrix eyeOffset)
		{
			// update view-projection matrix
			var vpMatrix = m_worldData.camera;
			vpMatrix *= eyeOffset;

			int width = renderTarget.Resolution.Width;
			int height = renderTarget.Resolution.Height;
			Single aspect = (float)width / (float)height;
			Single fov = (Single)Math.PI / 4;
			vpMatrix *= Matrix.PerspectiveFovLH(fov, aspect, 0.1f, 100.0f);

			var vdata = new _WorldVertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				vpMat = Matrix.Transpose(vpMatrix),
			};
			context.UpdateSubresource(ref vdata, m_worldVtxConst);

			m_renderTarget = renderTarget;
		}

		public void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, TextureView tex, DrawSystem.RenderMode renderMode)
		{
			var context = _GetContext();

			_SetDrawSetting(color, mesh, tex, renderMode);

			// update vertex shader resouce
			var vdata = new _MainVertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				worldMat = Matrix.Transpose(worldTrans),
			};
			context.UpdateSubresource(ref vdata, m_mainVtxConst);

			// draw
			context.Draw(mesh.VertexCount, 0);
			m_drawCallCount++;
		}

		public void DrawInstancedModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, TextureView tex, DrawSystem.RenderMode renderMode)
		{
			var context = _GetContext();

			if (m_nextInstanceIndex == 0)
			{
				_SetDrawSetting(color, mesh, tex, renderMode);
			}

			// hlsl is column-major memory layout, so we must transpose matrix
			m_instanceMainVtxConst[m_nextInstanceIndex] = new _MainVertexShaderConst() { worldMat = Matrix.Transpose(worldTrans) };
			m_instanceMainPixConst[m_nextInstanceIndex] = new _MainPixelShaderConst() { instanceColor = color };

			m_nextInstanceIndex++;
			if (m_nextInstanceIndex == m_maxInstanceCount)
			{
				// update vertex shader resouce
				context.UpdateSubresource<_MainVertexShaderConst>(m_instanceMainVtxConst, m_mainVtxConst);

				// update pixel shader resouce
				context.UpdateSubresource<_MainPixelShaderConst>(m_instanceMainPixConst, m_mainPixConst);

				// draw
				context.DrawInstanced(m_lastVertexCount, m_nextInstanceIndex, 0, 0);
				m_drawCallCount++;

				m_nextInstanceIndex = 0;
			}

		}

		public void EndDrawInstanceModel()
		{
			var context = _GetContext();
			if (m_nextInstanceIndex != 0)
			{
				// update vertex shader resouce
				context.UpdateSubresource<_MainVertexShaderConst>(m_instanceMainVtxConst, m_mainVtxConst);

				// draw
				context.DrawInstanced(m_lastVertexCount, m_nextInstanceIndex, 0, 0);
				m_drawCallCount++;

				m_nextInstanceIndex = 0;
			}
		}

		private void _SetDrawSetting(Color4 color, DrawSystem.MeshData mesh, TextureView tex, DrawSystem.RenderMode renderMode)
		{
			var context = _GetContext();

			// update texture
			if (m_lastTexture == null || m_lastTexture.IsDisposed() || m_lastTexture != tex)
			{
				tex.SetContext(0, context);
				m_lastTexture = tex;
			}

			// update render mode
			if (m_lastRenderMode == null || m_lastRenderMode != renderMode)
			{
				context.OutputMerger.BlendState = m_blendStateArray[(int)renderMode];
				context.OutputMerger.DepthStencilState = m_depthStencilStateArray[(int)renderMode];

				m_lastRenderMode = renderMode;
			}

			// update pixel shader resouce
			var pdata = new _MainPixelShaderConst()
			{
				instanceColor = color
			};
			context.UpdateSubresource(ref pdata, m_mainPixConst);

			// update input assembler
			if (m_lastTopology == null || m_lastTopology != mesh.Topology)
			{
				context.InputAssembler.PrimitiveTopology = mesh.Topology;
				m_lastTopology = mesh.Topology;
			}
			if (m_lastVertexBuffer == null || m_lastVertexBuffer != mesh.Buffer.Buffer)
			{
				context.InputAssembler.SetVertexBuffers(0, mesh.Buffer);
				m_lastVertexBuffer = mesh.Buffer.Buffer;
				m_lastVertexCount = mesh.VertexCount;
			}
		}

		abstract protected DeviceContext _GetContext();

		#region private types

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _MainVertexShaderConst
		{
			public Matrix worldMat;			// word matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _WorldVertexShaderConst
		{
			public Matrix vpMat;			// view projection matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _MainPixelShaderConst
		{
			public Color4 instanceColor;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _WorldPixelShaderConst
		{
			public Color4 ambientCol;
			public Color4 light1Col;	// light1 color
			public Vector4 cameraPos;	// camera position in model coords
			public Vector4 light1Dir;	// light1 direction in model coords
		}

		#endregion // private types

		#region private members

		protected DrawSystem.D3DData m_d3d;
		protected DrawResourceRepository m_repository = null;
		private DrawSystem.WorldData m_worldData;
		private RenderTarget m_renderTarget = null;
		private int m_drawCallCount = 0;

		// draw param 
		private Buffer m_mainVtxConst = null;
		protected Buffer m_worldVtxConst = null;
		private Buffer m_worldPixConst = null;
		private Buffer m_mainPixConst = null;
		protected RasterizerState m_rasterizerState = null;
		private BlendState[] m_blendStateArray = null;
		private DepthStencilState[] m_depthStencilStateArray = null;

		// previous draw setting
		private DrawSystem.RenderMode? m_lastRenderMode = null;
		private TextureView m_lastTexture = null;
		private PrimitiveTopology? m_lastTopology = null;
		private Buffer m_lastVertexBuffer = null;
		private int m_lastVertexCount = 0;

		// DrawInstancedModel context
		private int m_nextInstanceIndex = 0;
		private int m_maxInstanceCount = 100;
		private _MainVertexShaderConst[] m_instanceMainVtxConst;
		private _MainPixelShaderConst[] m_instanceMainPixConst;
		

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
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
					new InputElement("NORMAL", 0, Format.R32G32B32_Float, 24, 0),
				},
				"Shader/VS_Std.fx",
				"Shader/PS_Std.fx");

			m_repository.AddResource(shader);
		}

		#endregion // private methods
	}
}
