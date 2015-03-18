using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using System.Runtime.InteropServices;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// Basic functions about graphics sytem
	/// </summary>
    public partial class DrawSystem 
	{
		#region static

		private static DrawSystem s_singleton = null;

		static public void Initialize(IntPtr hWnd, Device device, SwapChain swapChain, HmdDevice hmd, bool bStereoRendering)
		{
			s_singleton = new DrawSystem(hWnd, device, swapChain, hmd, bStereoRendering);
		}

		static public void Dispose()
		{
			s_singleton.m_modelPassCtrl.Dispose();
			s_singleton.m_hmd.Detach();
			s_singleton.m_context.Dispose();
			s_singleton.m_repository.Dispose();
			s_singleton = null;
		}

		static public DrawSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		#region properties

		/// <summary>
		/// get/set a camera which makes Transform for World => View
		/// </summary>
		public Matrix Camera
		{
			get
			{
				return m_world.camera;
			}
			set
			{
				m_world.camera = value;
			}
		}

		public Color3 AmbientColor
		{
			get
			{
				return m_world.ambientCol;
			}
			set
			{
				m_world.ambientCol = value;
			}
		}

		private D3DData m_d3d;
		public D3DData D3D
		{
			get
			{
				return m_d3d;
			}
		}

		private DrawResourceRepository m_repository = null;
		public DrawResourceRepository ResourceRepository
		{
			get
			{
				return m_repository;
			}
		}

		#endregion // properties

		private DrawSystem(IntPtr hWnd, Device device, SwapChain swapChain, HmdDevice hmd, bool bStereoRendering)
        {
			m_d3d = new D3DData
			{
				device = device,
				context = device.ImmediateContext,
				swapChain = swapChain,
				hWnd = hWnd,
			};
			
			AmbientColor = new Color3(0, 0, 0);
			m_world.dirLight.Direction = new Vector3(0, 1, 0);
			m_world.dirLight.Color = new Color3(1, 1, 1);

			m_repository = new DrawResourceRepository(m_d3d);
			m_context = new DrawContext(m_d3d, m_repository, hmd, bStereoRendering);

			m_bStereoRendering = bStereoRendering;
			m_hmd = hmd;
			m_hmd.Attach(m_d3d, m_repository.GetDefaultRenderTarget());

			m_modelPassCtrl = new DrawModelPassCtrl(m_d3d, m_repository);
		}

		public void SetDirectionalLight(DirectionalLightData light)
		{
			m_world.dirLight = light;
		}

		public DrawContext BeginScene()
		{
			DrawSystem.WorldData data = m_world;
			m_context.BeginScene(data, m_modelPassCtrl);
			return m_context;
		}

		public void EndScene()
		{
			m_context.EndScene(m_modelPassCtrl);
		}

		#region private members

		/// <summary>
		/// current world data
		/// </summary>
		private WorldData m_world;

		private DrawContext m_context = null;

		private HmdDevice m_hmd = null;

		private bool m_bStereoRendering;

		private DrawModelPassCtrl m_modelPassCtrl = null;

		#endregion // private members

	
	}
}
