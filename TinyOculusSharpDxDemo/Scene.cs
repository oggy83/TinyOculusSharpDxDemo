using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace TinyOculusSharpDxDemo
{
    public class Scene : IDisposable
    {
		private FpsCounter m_dtFpsCounter;
		private FpsCounter m_perfFpsCounter;

        public Scene(Device device, SwapChain swapChain, Panel renderTarget, HmdDevice hmd, bool bStereoRendering)
		{
			DrawSystem.Initialize(renderTarget.Handle, device, swapChain, hmd, bStereoRendering);
			TextSystem.Initialize(renderTarget);
			//InputSystem.Initialize(renderTarget);
			//CameraSystem.Initialize();
			//EntitySystem.Initialize();

			
			
			// camera setting
			//var cameraSys = CameraSystem.GetInstance();
			//cameraSys.ActivateCamera(CameraSystem.FreeCameraName);

			var drawSys = DrawSystem.GetInstance();

			// light setting
			/*
			var dirLight = new DrawSystem.DirectionalLightData()
			{
				Direction = new Vector3(0.3f, -0.5f, 0),
				Color = new Color3(0.22f, 0.22f, 0.2f),
			};
			drawSys.SetDirectionalLight(dirLight);
			*/
			drawSys.AmbientColor = new Color3(0.1f, 0.1f, 0.15f);

			// init other members
			m_dtFpsCounter = new FpsCounter();
			m_perfFpsCounter = new FpsCounter();
		}

        public void RenderFrame()
		{
			float dT = (float)m_dtFpsCounter.GetDeltaTime() / 1000.0f;

			// Process Input
			//var inputSys = InputSystem.GetInstance();
			//inputSys.Update(dT);

			// update camera
			//var cameraSys = CameraSystem.GetInstance();
			//cameraSys.Update(dT);

			// draw
			var drawSys = DrawSystem.GetInstance();
			//drawSys.Camera = cameraSys.GetCameraData();
			//drawSys.ProjectionMatrix = cameraSys.GetProjection();

			// disp fps
			{
				int avgDT = m_perfFpsCounter.GetAverageDeltaTime();
				//int avgDT = m_dtFpsCounter.GetAverageDeltaTime();
				string text = String.Format("FPS:{0:f2}, DeltaTime:{1:f2}ms", 1000.0f / avgDT, avgDT);
				drawSys.AddDrawCommand(DrawCommand.CreateDrawTextCommand(text));
			}

			drawSys.ProcessDrawCommand(m_dtFpsCounter, m_perfFpsCounter);
		}

        /// <summary>
        /// Release all com resources.
        /// </summary>
        public void Dispose()
        {
			//CameraSystem.Dispose();
			//InputSystem.Dispose();
			TextSystem.Dispose();
			DrawSystem.Dispose();
        }
    }
}
