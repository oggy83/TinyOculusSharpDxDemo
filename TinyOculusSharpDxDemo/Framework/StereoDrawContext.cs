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
	public class StereoDrawContext : IDrawContext
	{
		public StereoDrawContext(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, DrawContext context)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_context = context;
			m_hmd = hmd;

			// Create render targets for each HMD eye
			var sizeArray = hmd.EyeResolutions;
			var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
			for (int index = 0; index < 2; ++index)
			{
				var renderTarget = RenderTarget.CreateRenderTarget(m_d3d, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
				m_repository.AddResource(renderTarget);
			}

			m_commandListTable = new List<CommandList>();
		}

		public void Dispose() 
		{
			foreach (var commandList in m_commandListTable)
			{
				commandList.Dispose();
			}

			m_context.Dispose();
		}

		public RenderTarget StartPass(DrawSystem.WorldData data)
		{
			var renderTarget = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var eyeOffset = m_hmd.GetEyePoses();

			m_context.SetWorldParams(renderTarget, data);
			m_hmd.BeginScene();

			m_context.UpdateWorldParams(m_d3d.Device.ImmediateContext, data);
			m_context.UpdateEyeParams(m_d3d.Device.ImmediateContext, renderTarget, eyeOffset[1]);// set right eye settings
			m_context.ClearRenderTarget(renderTarget);
			m_isContextDirty = true;

			return renderTarget;
		}

		public void EndPass()
		{
			var renderTargets = new[] { m_repository.FindResource<RenderTarget>("OVRLeftEye"), m_repository.FindResource<RenderTarget>("OVRRightEye") };
			var eyeOffset = m_hmd.GetEyePoses();

			if (m_isContextDirty)
			{
				var prevCommandList = m_context.FinishCommandList();
				m_commandListTable.Add(prevCommandList);
				m_isContextDirty = false;
			}

			// render right eye image to left eye buffer
			foreach (var commandList in m_commandListTable)
			{
				m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);
			}

			// copy left eye buffer to right eye buffer
			m_d3d.Device.ImmediateContext.CopyResource(renderTargets[0].TargetTexture, renderTargets[1].TargetTexture);

			// set left eye settings
			m_context.UpdateEyeParams(m_d3d.Device.ImmediateContext, renderTargets[0], eyeOffset[0]);

			// render left eye image to left eye buffer
			foreach (var commandList in m_commandListTable)
			{
				m_d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);
			}

			var leftEyeRT = m_repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = m_repository.FindResource<RenderTarget>("OVRRightEye");
			m_hmd.EndScene(leftEyeRT, rightEyeRT);

			// delete command list
			foreach (var commandList in m_commandListTable)
			{
				commandList.Dispose();
			}
			m_commandListTable.Clear();
		}

		public void DrawModel(Matrix worldTrans, Color4 color, DrawSystem.MeshData mesh, TextureView tex, DrawSystem.RenderMode renderMode)
		{
			m_isContextDirty = true;
			m_context.DrawModel(worldTrans, color, mesh, tex, renderMode);
		}

		public void BeginDrawInstance(DrawSystem.MeshData mesh, TextureView tex, DrawSystem.RenderMode renderMode)
		{
			m_isContextDirty = true;
			m_context.BeginDrawInstance(mesh, tex, renderMode);
		}

		public void AddInstance(Matrix worldTrans, Color4 color)
		{
			m_isContextDirty = true;
			m_context.AddInstance(worldTrans, color);
		}

		public void EndDrawInstance()
		{
			m_isContextDirty = true;
			m_context.EndDrawInstance();
		}

		public CommandList FinishCommandList()
		{
			Debug.Assert(false, "MonoralDrawContext do not support FinishCommandList()");
			return null;
		}

		public void ExecuteCommandList(CommandList commandList)
		{
			if (m_isContextDirty)
			{
				var prevCommandList = m_context.FinishCommandList();
				m_commandListTable.Add(prevCommandList);
				m_isContextDirty = false;
			}

			m_commandListTable.Add(commandList);
		}

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private DrawContext m_context = null;
		private HmdDevice m_hmd = null;
		private List<CommandList> m_commandListTable = null;
		private bool m_isContextDirty = false;

		#endregion // private members
	}
}
