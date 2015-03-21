﻿using System;
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
			if (bStereoRendering)
			{
				m_context = new StereoDrawContext(d3d, repository, hmd);
			}
			else
			{
				m_context = new MonoralDrawContext(d3d, repository);
			}

			// Init settings
			m_d3d.Device.QueryInterface<Device1>().MaximumFrameLatency = 1;
		}

		public void Dispose()
		{
			m_context.Dispose();
		}

		public void StartPass(DrawSystem.WorldData data)
		{
			m_context.BeginScene(data);
		}

		public void EndPass()
		{
			m_context.EndScene();
		}

		#region private members

		private DrawSystem.D3DData m_d3d;
		private DrawResourceRepository m_repository = null;
		private bool m_bStereoRendering;
		private HmdDevice m_hmd = null;

		#endregion // private members
	}
}