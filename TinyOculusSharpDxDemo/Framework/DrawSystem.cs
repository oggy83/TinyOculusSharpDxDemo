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
			s_singleton.m_hmd.Detach();
			s_singleton.m_context.Dispose();
			s_singleton.m_repository.Dispose();
			timeEndPeriod(1);
			s_singleton = null;
		}

		static public DrawSystem GetInstance()
		{
			return s_singleton;
		}

		[DllImport("winmm.dll")]
		public static extern int timeBeginPeriod(int uPeriod);

		[DllImport("winmm.dll")]
		public static extern int timeEndPeriod(int uPeriod);

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

			int capacity = 1024;
			m_commandBuffer = new DrawCommandBuffer(capacity);

			m_repository = new DrawResourceRepository(m_d3d);
			m_context = new DrawContext(m_d3d, m_repository, hmd, bStereoRendering);

			m_bStereoRendering = bStereoRendering;
			m_hmd = hmd;
			m_hmd.Attach(m_d3d, m_repository.GetDefaultRenderTarget());

			
		}

		public void SetDirectionalLight(DirectionalLightData light)
		{
			m_world.dirLight = light;
		}

		public void SetPointLight(PointLightData[] lights)
		{
			m_world.pointLights = lights;
		}

		/// <summary>
		/// Add draw command which is processed in current frame
		/// </summary>
		/// <param name="command">command</param>
		public void AddDrawCommand(DrawCommand command)
		{
			m_commandBuffer.AddCommand(command);
		}

		/// <summary>
		/// Process all draw command
		/// </summary>
		/// <param name="dtFpsCounter">fps counter</param>
		/// <remarks>This method clears draw command list</remarks>
		public void ProcessDrawCommand(FpsCounter dtFpsCounter)
		{
			DrawSystem.WorldData data = m_world;
			m_context.BeginScene(data);

			m_commandBuffer.Sort();
			m_context.Draw(m_commandBuffer);
			m_commandBuffer.Clear();

			m_context.EndScene();
			dtFpsCounter.EndFrame();
			dtFpsCounter.BeginFrame();
		}

		#region private members

		/// <summary>
		/// current world data
		/// </summary>
		private WorldData m_world;

		private DrawCommandBuffer m_commandBuffer = null;

		private DrawContext m_context = null;

		private HmdDevice m_hmd = null;

		private bool m_bStereoRendering;

		#endregion // private members

	
	}
}
