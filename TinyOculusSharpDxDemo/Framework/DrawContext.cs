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
	public partial class DrawContext : IDisposable
	{
		public DrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, bool bStereoRendering)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_hmd = hmd;
			m_bStereoRendering = bStereoRendering;

			if (bStereoRendering)
			{
				// Create render targets for each HMD eye
				var sizeArray = hmd.EyeResolutions;
				var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
				for (int index = 0; index < 2; ++index)
				{
					var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
					m_repository.AddResource(renderTarget);
				}
			}

			m_vcBuf = DrawUtil.CreateConstantBuffer<_VertexShaderConst>(m_d3d);
			m_pcBuf = DrawUtil.CreateConstantBuffer<_PixelShaderConst>(m_d3d);

		}

		public void Dispose()
		{
			// nothing	
		}

		/// <summary>
		/// Execute DrawCommand
		/// </summary>
		/// <param name="commandBuffer">sorted draw command list</param>
		public void Draw(IDrawPassCtrl passCtrl, DrawCommandBuffer commandBuffer)
		{
			RenderTarget[] renderTargets;
			Matrix[] eyeOffset;
			if (m_bStereoRendering)
			{
				renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
				eyeOffset = m_hmd.GetEyePoses();
			}
			else
			{
				renderTargets = new[] { m_repository.GetDefaultRenderTarget() };
				eyeOffset = new[] { Matrix.Identity };
			}

			for (int index = 0; index < renderTargets.Count(); ++index)
			{
				var renderTarget = renderTargets[index];
				passCtrl.StartPass(renderTarget);

				// update view-projection matrix
				m_vpMatrix = m_worldData.camera;
				m_vpMatrix *= eyeOffset[index];

				// update projection matrix
				int width = renderTarget.Resolution.Width;
				int height = renderTarget.Resolution.Height;
				Single aspect = (float)width / (float)height;
				Single fov = (Single)Math.PI / 4;
				m_vpMatrix *= Matrix.PerspectiveFovLH(fov, aspect, 0.1f, 100.0f);

				foreach (var command in commandBuffer.Commands)
				{
					SetDrawParams(command.m_worldTransform, command.m_mesh, command.m_texture);
				}
			}
		}

		/// <summary>
		/// Begin scene rendering
		/// </summary>
		/// <param name="data">world data</param>
		public void BeginScene(DrawSystem.WorldData data)
		{
			m_worldData = data;

			if (m_bStereoRendering)
			{
				m_hmd.BeginScene();
			}
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene()
		{
			if (m_bStereoRendering)
			{
				var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
				var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
				m_hmd.EndScene(leftEyeRT, rightEyeRT);
			}
			else
			{
				int syncInterval = 0;// 0 => immediately return, 1 => vsync
				m_d3d.swapChain.Present(syncInterval, PresentFlags.None);
			}
		}

		public void SetDrawParams(Matrix worldTrans, DrawSystem.MeshData mesh, TextureView tex)
		{
			var context = m_d3d.context;

			tex.SetContext(0, m_d3d);


			// update vertex shader resouce
			var wvpMat = worldTrans * m_vpMatrix;
			var vdata = new _VertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				wvpMat = Matrix.Transpose(wvpMat),
				worldMat = Matrix.Transpose(worldTrans),
			};
			context.UpdateSubresource(ref vdata, m_vcBuf);
			context.VertexShader.SetConstantBuffer(0, m_vcBuf);

			// update pixel shader resource
			var pdata = new _PixelShaderConst()
			{
				ambientCol = new Color4(m_worldData.ambientCol),
				light1Col = new Color4(m_worldData.dirLight.Color),
				cameraPos = new Vector4(m_worldData.camera.TranslationVector, 1.0f),
				light1Dir = new Vector4(m_worldData.dirLight.Direction, 0.0f),
			};
			context.UpdateSubresource(ref pdata, m_pcBuf);
			context.PixelShader.SetConstantBuffer(0, m_pcBuf);

			// draw
			m_d3d.context.InputAssembler.PrimitiveTopology = mesh.Topology;
			m_d3d.context.InputAssembler.SetVertexBuffers(0, mesh.Buffer);
			m_d3d.context.Draw(mesh.VertexCount, 0);
		}

		#region private types

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _VertexShaderConst
		{
			public Matrix wvpMat;			// word view projection matrix
			public Matrix worldMat;			// word matrix
		}

		private struct _PixelShaderConst
		{
			public Color4 ambientCol;
			public Color4 light1Col;	// light1 color
			public Vector4 cameraPos;	// camera position in model coords
			public Vector4 light1Dir;	// light1 direction in model coords
		}

		#endregion // private types

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private DrawSystem.WorldData m_worldData;
		private HmdDevice m_hmd = null;
		private bool m_bStereoRendering;
		Matrix m_vpMatrix;

		// draw param 
		Buffer m_vcBuf = null;
		Buffer m_pcBuf = null;

		#endregion // private members
		
	}
}
