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

		
		public IDrawContext Context
		{
			get
			{
				if (m_bStereoRendering)
				{
					return m_stereoContext;
				}
				else
				{
					return m_monoralContext;
				}
			}
		}

		#endregion // properties

		public DrawPassCtrl(DrawSystem.D3DData d3d, DrawResourceRepository repository, HmdDevice hmd, bool bStereoRendering, int multiThreadCount)
		{
			m_d3d = d3d;
			m_repository = repository;
			m_bStereoRendering = bStereoRendering;
			m_hmd = hmd;
			m_factory = new DrawContext.Factory(d3d, repository);

			if (bStereoRendering)
			{
				m_stereoContext = new StereoDrawContext(d3d, repository, hmd, m_factory.CreateDeferredDrawContext());
			}
			else
			{
				m_monoralContext = new MonoralDrawContext(d3d, repository, m_factory.CreateImmediateDrawContext());
			}

			// Init settings
			m_d3d.Device.QueryInterface<Device1>().MaximumFrameLatency = 1;

			m_subThreadCtxList = new List<_SubThreadContextData>();
			for (int index = 0; index < multiThreadCount; ++index)
			{
				var drawContext = m_factory.CreateDeferredDrawContext();
				m_subThreadCtxList.Add(new _SubThreadContextData() { DrawContext = drawContext });
			}
		}

		public void Dispose()
		{
			foreach (var data in m_subThreadCtxList)
			{
				data.DrawContext.Dispose();
			}

			m_factory.Dispose();
			if (m_monoralContext != null)
			{
				m_monoralContext.Dispose();
			}

			if (m_stereoContext != null)
			{
				m_stereoContext.Dispose();
			}
		}

		public void StartPass(DrawSystem.WorldData worldData)
		{
			RenderTarget renderTarget = null;
			if (m_bStereoRendering)
			{
				renderTarget = m_stereoContext.StartPass(worldData);
			}
			else
			{
				renderTarget = m_monoralContext.StartPass(worldData);
			}

			foreach (var data in m_subThreadCtxList)
			{
				data.DrawContext.SetWorldParams(renderTarget, worldData);
			}
		}

		public void EndPass()
		{
			if (m_bStereoRendering)
			{
				m_stereoContext.EndPass();
			}
			else
			{
				m_monoralContext.EndPass();
			}
		}

		public IDrawContext GetSubThreadContext(int index)
		{
			return m_subThreadCtxList[index].DrawContext;
		}


		#region private types

		private struct _SubThreadContextData
		{
			public DrawContext DrawContext;
		}

		#endregion // private types

		#region private members

		private MonoralDrawContext m_monoralContext = null;
		private StereoDrawContext m_stereoContext = null;
		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private bool m_bStereoRendering;
		private HmdDevice m_hmd = null;
		private DrawContext.Factory m_factory = null;
		private List<_SubThreadContextData> m_subThreadCtxList = null;

		#endregion // private members
	}
}
