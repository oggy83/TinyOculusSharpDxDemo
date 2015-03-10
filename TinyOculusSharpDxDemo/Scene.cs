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
		
        public Scene(Device device, SwapChain swapChain, Panel renderTarget, HmdDevice hmd, bool bStereoRendering)
		{
			var drawSys = DrawSystem.GetInstance();

			// load textures
			m_blockTexture = TextureView.FromFile("block", drawSys.D3D, "Image/block.png");
			m_floorTexture = TextureView.FromFile("floor", drawSys.D3D, "Image/floor.jpg");
			drawSys.ResourceRepository.AddResource(m_blockTexture);
			drawSys.ResourceRepository.AddResource(m_floorTexture);

			// light setting
			drawSys.SetDirectionalLight(new DrawSystem.DirectionalLightData()
			{
				Direction = new Vector3(0.3f, -0.5f, 0),
				Color = new Color3(0.9f, 0.9f, 0.8f),
			});
			drawSys.AmbientColor = new Color3(0.3f, 0.4f, 0.6f);

			// camera setting
			drawSys.Camera = new DrawSystem.CameraData(new Vector3(0.0f, 1.2f, 0.0f), new Vector3(0.0f, 1.2f, 1.0f), Vector3.Up).GetViewMatrix();
			
			// create random box storm
			var rnd = new Random();
			var directions = new Vector2[] { new Vector2(1,0), new Vector2(-1,0), new Vector2(0,1), new Vector2(0,-1)};
			for (int i = 0; i < 10; ++i)
			{
				for (int j = 0; j < 10; ++j)
				{
					foreach (var dir in directions)
					{
						float scale = RandomUtil.NextFloat(rnd, 0.1f, 0.5f);
						float r = RandomUtil.NextFloat(rnd, 0.7f, 1.0f);
						float g = RandomUtil.NextFloat(rnd, 0.7f, 1.0f);
						float b = RandomUtil.NextFloat(rnd, 0.7f, 1.0f);
						float speed = RandomUtil.NextFloat(rnd, 0.6f, 0.9f);
						var box = DrawModel.CreateBox("box", scale, 1.0f, new Color4(r, g, b, 1.0f), Vector4.Zero);
						var layout = Matrix.Translation(-10.0f + 2.0f * j, 2.0f + 2.0f * i, -15.0f);
						m_entityList.Add(new _EntityData()
						{
							Model = box,
							Layout = layout,
							Delay = RandomUtil.NextFloat(rnd, 0.0f, 100.0f),
							Velocity = speed * dir,
						});
					}
				}
			}

			// init others
			m_fps = new FpsCounter();
			m_floor = DrawModel.CreateFloor("floor", 10.0f, 4.0f, Color4.White, Vector4.Zero);
		}

        public void RenderFrame()
		{
			double dt = m_fps.GetDeltaTime();
			m_accTime += dt;

			// draw
			var drawSys = DrawSystem.GetInstance();

			// disp fps
			{
				double avgDT = m_fps.GetAverageDeltaTime();
				string text = String.Format("FPS:{0:f2}, DeltaTime:{1:f2}ms", 1.0 / avgDT, avgDT * 1000.0f);
				drawSys.AddDrawCommand(DrawCommand.CreateDrawTextCommand(text));
			}

			// draw floor
			drawSys.AddDrawCommand(DrawCommand.CreateDrawModelCommand(Matrix.Identity, m_floor.NodeList[0].Mesh, m_floorTexture));

			// draw block entities
			
			foreach (var entity in m_entityList)
			{
				float frame = (float)m_accTime + entity.Delay;
				float angle = frame % (2.0f * (float)Math.PI);
				Matrix worldTrans =
					Matrix.RotationYawPitchRoll(angle, angle, angle) 
					* entity.Layout
					* Matrix.Translation((entity.Velocity.X * frame) % 30.0f, 0.0f, (entity.Velocity.Y * frame) % 30.0f);
				drawSys.AddDrawCommand(DrawCommand.CreateDrawModelCommand(worldTrans, entity.Model.NodeList[0].Mesh, m_blockTexture));
			}

			// execute draw commands
			drawSys.ProcessDrawCommand(m_fps);
		}

        /// <summary>
        /// Release all com resources.
        /// </summary>
        public void Dispose()
        {
			// @todo release DrawModel
		}

		#region private types

		private struct _EntityData
		{
			public DrawModel Model;
			public Matrix Layout;
			public float Delay;
			public Vector2 Velocity;
		}

		#endregion // private types

		#region private members

		private FpsCounter m_fps;
		private List<_EntityData> m_entityList = new List<_EntityData>();
		private DrawModel m_floor;
		private double m_accTime = 0;
		private TextureView m_blockTexture = null;
		private TextureView m_floorTexture = null;


		#endregion // private members
	}
}
