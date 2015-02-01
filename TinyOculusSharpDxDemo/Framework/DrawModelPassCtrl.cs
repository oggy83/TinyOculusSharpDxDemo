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
		public const int MaxBoneMatrices = 32;

		public DrawModelPassCtrl(DrawSystem.D3DData d3d, DrawResourceRepository repository)
		{
			m_d3d = d3d;
			m_repository = repository;

			_RegisterStandardSetting();

			m_vcBuf = DrawUtil.CreateConstantBuffer<_VertexShaderConst>(m_d3d);
			m_dbgVcBuf = DrawUtil.CreateConstantBuffer<_DebugVertexShaderConst>(m_d3d);
			m_pcBuf = DrawUtil.CreateConstantBuffer<_PixelShaderConst>(m_d3d);
			m_boneBuf = DrawUtil.CreateConstantBuffer(m_d3d, Utilities.SizeOf<Matrix>() * MaxBoneMatrices);
		}

		public void Dispose()
		{
			// nothing
		}

		/// <summary>
		/// setup render pass
		/// </summary>
		public void StartPass()
		{
			var renderTarget = m_repository.GetDefaultRenderTarget();
			var context = m_d3d.context;

			// Init a render target
			context.Rasterizer.SetViewport(new Viewport(0, 0, 800, 600, 0.0f, 1.0f));// temp
			context.OutputMerger.SetTargets(renderTarget.DepthStencilView, renderTarget.TargetView);
			
			context.ClearDepthStencilView(renderTarget.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
			context.ClearRenderTargetView(renderTarget.TargetView, new Color4(0.3f, 0.3f, 0.45f, 1.0f));
		}

		/// <summary>
		/// set render state for the indicated command
		/// </summary>
		/// <param name="command">draw command</param>
		/// <param name="world">world data</param>
		public void ExecuteCommand(DrawCommand command, DrawSystem.WorldData world)
		{
			var commandData = command.GetDrawModelData();

			Effect effect = null;
			effect = m_repository.FindResource<Effect>("Std");

			// set context
			m_d3d.context.InputAssembler.InputLayout = effect.Layout;
			m_d3d.context.VertexShader.Set(effect.VertexShader);
			m_d3d.context.PixelShader.Set(effect.PixelShader);

			// update matrix
			var wvpMat = commandData.m_worldTransform * world.camera.GetViewMatrix() * world.proj;
			var context = m_d3d.context;

			var vdata = new _VertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				wvpMat = Matrix.Transpose(wvpMat),
				worldMat = Matrix.Transpose(commandData.m_worldTransform),
				isEnableSkinning = commandData.m_boneMatrices != null,
			};

			context.UpdateSubresource(ref vdata, m_vcBuf);
			context.VertexShader.SetConstantBuffer(0, m_vcBuf);

			var pdata = new _PixelShaderConst()
			{
				ambientCol = new Color4(world.ambientCol),
				light1Col = new Color4(world.dirLight.Color),
				light2Col = new Color4(world.pointLights[0].Color),
				lightRange = new Vector4(world.pointLights[0].Range, 0, 0, 0),
				cameraPos = new Vector4(world.camera.eye, 1.0f),
				light1Dir = new Vector4(world.dirLight.Direction, 0.0f),
				light2Pos = world.pointLights[0].Position,
			};
			
			context.UpdateSubresource(ref pdata, m_pcBuf);
			context.PixelShader.SetConstantBuffer(0, m_pcBuf);

			// draw
			m_d3d.context.InputAssembler.PrimitiveTopology = commandData.m_mesh.Topology;
			m_d3d.context.InputAssembler.SetVertexBuffers(0, commandData.m_mesh.Buffers);
			m_d3d.context.Draw(commandData.m_mesh.VertexCount, 0);
		}

		#region private types

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _VertexShaderConst
		{
			
			public Matrix wvpMat;			// word view projection matrix
			public Matrix worldMat;			// word matrix
			
			public bool isEnableSkinning;
			public float dummy1;
			public float dummy2;
			public float dummy3;
			
			//public Matrix light1InvWvpMat;	// inverse word view projection matrix for light1
		}

		private struct _PixelShaderConst
		{
			public Color4 ambientCol;
			public Color4 light1Col;	// light1 color
			public Color4 light2Col;	// light2 color
			public Vector4 lightRange;	// light2-light5 range

			public Vector4 cameraPos;		// camera position in model coords
			public Vector4 light1Dir;		// light1 direction in model coords
			public Vector4 light2Pos;		// light2 position in model coords
		}

		private struct _DebugVertexShaderConst
		{
			public Matrix wvpMat;		// word view projection matrix
			public Vector4 Param;			// general parameter(s)
		}

		#endregion // private types

		#region private members

		DrawSystem.D3DData m_d3d;
		DrawResourceRepository m_repository = null;

		Buffer m_vcBuf = null;
		Buffer m_dbgVcBuf = null;
		Buffer m_pcBuf = null;
		Buffer m_boneBuf = null;
		Matrix[] m_tmpBoneMatrices = new Matrix[MaxBoneMatrices];

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
					new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, 0),
					new InputElement("TANGENT", 0, Format.R32G32B32_Float, 0, 1),
					new InputElement("BONEINDEX", 0, Format.R32G32B32A32_SInt, 0, 2),
					new InputElement("BONEWEIGHT", 0, Format.R32G32B32A32_Float, 16, 2),
				},
				"Shader/VS_Std.fx",
				"Shader/PS_Std.fx");

			m_repository.AddResource(shader);
		}

		#endregion // private methods

	}
}
