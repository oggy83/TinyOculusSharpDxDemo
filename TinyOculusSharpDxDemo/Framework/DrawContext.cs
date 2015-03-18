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

			m_mainVtxConst = DrawUtil.CreateConstantBuffer<_MainVertexShaderConst>(m_d3d);
			m_worldVtxConst = DrawUtil.CreateConstantBuffer<_WorldVertexShaderConst>(m_d3d);
			m_pixConst = DrawUtil.CreateConstantBuffer<_PixelShaderConst>(m_d3d);
			m_deferredContext = new DeviceContext(m_d3d.device);

		}

		public void Dispose()
		{
			m_deferredContext.Dispose();
			m_pixConst.Dispose();
			m_worldVtxConst.Dispose();
			m_mainVtxConst.Dispose();
		}

		/// <summary>
		/// Begin scene rendering
		/// </summary>
		/// <param name="data">world data</param>
		public void BeginScene(DrawSystem.WorldData data, IDrawPassCtrl passCtrl)
		{
			m_passCtrl = passCtrl;
			m_worldData = data;
			if (m_bStereoRendering)
			{
				m_hmd.BeginScene();
			}

			if (m_bStereoRendering)
			{
				var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
				var eyeOffset = m_hmd.GetEyePoses();

				// set right eye settings
				UpdateWorldParams(m_d3d.context, renderTargets[0], eyeOffset[1]);

				// make command list by deferred context
				passCtrl.StartPass(m_deferredContext, renderTargets[0]);
				m_deferredContext.VertexShader.SetConstantBuffer(1, m_worldVtxConst);

			}
			else
			{
				var renderTarget = m_repository.GetDefaultRenderTarget();
				UpdateWorldParams(m_d3d.context, renderTarget, Matrix.Identity);

				passCtrl.StartPass(m_d3d.context, renderTarget);
				m_d3d.context.VertexShader.SetConstantBuffer(1, m_worldVtxConst);

			}
		}

		/// <summary>
		/// End scene rendering
		/// </summary>
		public void EndScene(IDrawPassCtrl passCtrl)
		{
			if (m_bStereoRendering)
			{
				var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
				var eyeOffset = m_hmd.GetEyePoses();

				var commandList = m_deferredContext.FinishCommandList(false);

				// render right eye image to left eye buffer
				m_d3d.context.ExecuteCommandList(commandList, false);

				// copy left eye buffer to right eye buffer
				m_d3d.context.CopyResource(renderTargets[0].TargetTexture, renderTargets[1].TargetTexture);

				// set left eye settings
				UpdateWorldParams(m_d3d.context, renderTargets[0], eyeOffset[0]);

				// render left eye image to left eye buffer
				m_d3d.context.ExecuteCommandList(commandList, false);

				commandList.Dispose();

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

		public void UpdateWorldParams(DeviceContext context, RenderTarget renderTarget, Matrix eyeOffset)
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
			
		}

		public void SetDrawParams(Matrix worldTrans, DrawSystem.MeshData mesh, TextureView tex)
		{
			var context = m_passCtrl.Context;
			tex.SetContext(0, context);

			// update vertex shader resouce
			var vdata = new _MainVertexShaderConst()
			{
				// hlsl is column-major memory layout, so we must transpose matrix
				worldMat = Matrix.Transpose(worldTrans),
			};
			context.UpdateSubresource(ref vdata, m_mainVtxConst);
			context.VertexShader.SetConstantBuffer(0, m_mainVtxConst);
			

			// update pixel shader resource
			var pdata = new _PixelShaderConst()
			{
				ambientCol = new Color4(m_worldData.ambientCol),
				light1Col = new Color4(m_worldData.dirLight.Color),
				cameraPos = new Vector4(m_worldData.camera.TranslationVector, 1.0f),
				light1Dir = new Vector4(m_worldData.dirLight.Direction, 0.0f),
			};
			context.UpdateSubresource(ref pdata, m_pixConst);
			context.PixelShader.SetConstantBuffer(0, m_pixConst);

			// draw
			context.InputAssembler.PrimitiveTopology = mesh.Topology;
			context.InputAssembler.SetVertexBuffers(0, mesh.Buffer);
			context.Draw(mesh.VertexCount, 0);
		}

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
		private DeviceContext m_deferredContext = null;
		private IDrawPassCtrl m_passCtrl;

		// draw param 
		private Buffer m_mainVtxConst = null;
		private Buffer m_worldVtxConst = null;
		private Buffer m_pixConst = null;

		#endregion // private members
		
	}
}
