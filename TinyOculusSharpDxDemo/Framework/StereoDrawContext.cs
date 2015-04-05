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
		private StereoDrawContext(DeviceContext context, CommonInitParam initParam, HmdDevice hmd)
		: base(context, initParam)
		{
			m_initParam = initParam;
			m_hmd = hmd;
			m_deferredContext = context;

			// Create render targets for each HMD eye
			var sizeArray = hmd.EyeResolutions;
			var resNames = new string[] { "OVRLeftEye", "OVRRightEye" };
			for (int index = 0; index < 2; ++index)
			{
				var renderTarget = RenderTarget.CreateRenderTarget(m_initParam.D3D, resNames[index], sizeArray[index].Width, sizeArray[index].Height);
				m_initParam.Repository.AddResource(renderTarget);
			}
		}

		public static StereoDrawContext Create(CommonInitParam initParam, HmdDevice hmd)
		{
			return new StereoDrawContext(new DeviceContext(initParam.D3D.Device), initParam, hmd);
		}

		override public void Dispose() 
		{
			m_deferredContext.Dispose();
			base.Dispose();
		}

		override public RenderTarget BeginScene(DrawSystem.WorldData data)
		{
			var repository = m_initParam.Repository;
			var renderTarget = repository.FindResource<RenderTarget>("OVRLeftEye");
			var eyeOffset = m_hmd.GetEyePoses();

			SetWorldParams(renderTarget, data);
			m_hmd.BeginScene();

			_UpdateWorldParams(m_initParam.D3D.Device.ImmediateContext, data);
			_UpdateEyeParams(m_initParam.D3D.Device.ImmediateContext, renderTarget, eyeOffset[1]);// set right eye settings
			_ClearRenderTarget(renderTarget);

			return renderTarget;
		}

		override public void EndScene()
		{
			var repository = m_initParam.Repository;
			var d3d = m_initParam.D3D;
			var renderTargets = new[] { repository.FindResource<RenderTarget>("OVRLeftEye"), repository.FindResource<RenderTarget>("OVRRightEye") };
			var eyeOffset = m_hmd.GetEyePoses();

			var commandList = m_deferredContext.FinishCommandList(true);

			// render right eye image to left eye buffer
			d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);

			// copy left eye buffer to right eye buffer
			d3d.Device.ImmediateContext.CopyResource(renderTargets[0].TargetTexture, renderTargets[1].TargetTexture);

			// set left eye settings
			_UpdateEyeParams(d3d.Device.ImmediateContext, renderTargets[0], eyeOffset[0]);

			// render left eye image to left eye buffer
			d3d.Device.ImmediateContext.ExecuteCommandList(commandList, true);

			commandList.Dispose();

			var leftEyeRT = repository.FindResource<RenderTarget>("OVRLeftEye");
			var rightEyeRT = repository.FindResource<RenderTarget>("OVRRightEye");
			m_hmd.EndScene(leftEyeRT, rightEyeRT);
		}

		#region private members

		private CommonInitParam m_initParam;
		private DeviceContext m_deferredContext = null;
		private HmdDevice m_hmd = null;

		#endregion // private members
	}
}
