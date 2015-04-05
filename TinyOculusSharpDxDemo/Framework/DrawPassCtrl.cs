using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public class DrawPassCtrl : IDisposable
	{
		#region properties

		private DrawContext m_context = null;
		public IDrawContext Context
		{
			get
			{
				return m_context;
			}
		}

		#endregion // properties

		public DrawPassCtrl(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, bool bStereoRendering)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_bStereoRendering = bStereoRendering;
			m_hmd = hmd;
			m_factory = new DrawContext.Factory(d3d, repository);

			if (bStereoRendering)
			{
				m_context = StereoDrawContext.Create(m_factory.GetInitParam(), m_hmd);
			}
			else
			{
				m_context = MonoralDrawContext.Create(m_factory.GetInitParam());
			}

			// Init settings
			m_d3d.Device.QueryInterface<Device1>().MaximumFrameLatency = 1;

			m_subThreadCtxList = new List<_SubThreadContextData>();
			for (int index = 0; index < 10; ++index)
			{
				var rawContext = new DeviceContext(m_d3d.Device);
				var drawContext = new DrawContext(rawContext, m_factory.GetInitParam());
				m_subThreadCtxList.Add(new _SubThreadContextData() { RawContext = rawContext, DrawContext = drawContext });
			}
		}

		public void Dispose()
		{
			foreach (var data in m_subThreadCtxList)
			{
				data.DrawContext.Dispose();
				data.RawContext.Dispose();
			}

			m_factory.Dispose();
			m_context.Dispose();
		}

		public void StartPass(DrawSystem.WorldData worldData)
		{
			var renderTarget = m_context.BeginScene(worldData);
			foreach (var data in m_subThreadCtxList)
			{
				data.DrawContext.BeginScene(worldData);
				data.DrawContext.SetWorldParams(renderTarget, worldData);
			}
		}

		public void EndPass()
		{
			foreach (var data in m_subThreadCtxList)
			{
				data.DrawContext.EndScene();
			}
			m_context.EndScene();
		}

		public IDrawContext GetSubThreadContext(int index)
		{
			return m_subThreadCtxList[index].DrawContext;
		}


		#region private types

		private struct _SubThreadContextData
		{
			public DrawContext DrawContext;
			public DeviceContext RawContext;
		}

		#endregion // private types

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private bool m_bStereoRendering;
		private HmdDevice m_hmd = null;
		private DrawContext.Factory m_factory = null;
		private List<_SubThreadContextData> m_subThreadCtxList = null;

		#endregion // private members
	}
}
