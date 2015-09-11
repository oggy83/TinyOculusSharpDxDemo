using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
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
		private const float EntityRange = 10.0f;
		
        public Scene(Device device, SwapChain swapChain, Panel renderTarget, HmdDevice hmd, bool bStereoRendering, int multiThreadCount)
		{
			var drawSys = DrawSystem.GetInstance();

			// load textures
			var textures = new List<TextureView>(new []
			{
				TextureView.FromFile("block", drawSys.D3D, "Image/block.png"),
				TextureView.FromFile("dot", drawSys.D3D, "Image/dot.png"),
				TextureView.FromFile("floor", drawSys.D3D, "Image/floor.jpg"),
			});
			var numTextures = new TextureView[10];
			for (int i = 0; i < 10; ++i)
			{
				var name = String.Format("number_{0}", i);
				numTextures[i] = TextureView.FromFile(name, drawSys.D3D, String.Format("Image/{0}.png", name));
			}
			textures.AddRange(numTextures);
			foreach (var tex in textures)
			{
				drawSys.ResourceRepository.AddResource(tex);
			}
			

			// light setting
			drawSys.SetDirectionalLight(new DrawSystem.DirectionalLightData()
			{
				Direction = new Vector3(0.3f, -0.5f, 0),
				Color = new Color3(0.9f, 0.9f, 0.8f),
			});
			drawSys.AmbientColor = new Color3(0.3f, 0.4f, 0.6f);
			drawSys.FogColor = new Color3(0.3f, 0.5f, 0.8f);
            drawSys.NearClip = 0.01f;
            drawSys.FarClip = 10000.0f;

			// camera setting
			drawSys.Camera = new DrawSystem.CameraData(new Vector3(0.0f, 1.5f, 0.0f), new Vector3(0.0f, 1.5f, 1.0f), Vector3.Up);

			// create random box storm
			var boxModel = DrawModel.CreateBox(1.0f, 1.0f, Vector4.Zero);
			m_drawModelList.Add(boxModel);
			var rnd = new Random();
			for (int i = 0; i < 40; ++i)
			{
				for (int j = 0; j < 10; ++j)
				{
					for (int k = 0; k < 40; ++k)
					{
						double angle = RandomUtil.NextDouble(rnd, 0.0, 2 * Math.PI);
						float scale = RandomUtil.NextFloat(rnd, 0.03f, 0.1f);
						float r = RandomUtil.NextFloat(rnd, 0.5f, 1.0f);
						float g = RandomUtil.NextFloat(rnd, 0.5f, 1.0f);
						float b = RandomUtil.NextFloat(rnd, 0.5f, 1.0f);
						float speed = RandomUtil.NextFloat(rnd, 0.2f, 0.5f);
						var layout = Matrix.Scaling(scale) * Matrix.Translation(i - 20.0f, 1.5f + j, k - 20.0f);
						m_boxList.Add(new ModelEntity(new ModelEntity.InitParam()
						{
							Model = boxModel,
							Texture = drawSys.ResourceRepository.FindResource<TextureView>("block"),
							Layout = layout,
							Delay = RandomUtil.NextFloat(rnd, 0.0f, 100.0f),
							Forward = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle)),
							Color = new Color4(r, g, b, 1),
							Speed = speed,
						}));
					}
				}
			}

			// create number entity
			m_fps = new FpsCounter();
			m_numberEntity = new NumberEntity(new NumberEntity.InitParam()
			{
				Dot = drawSys.ResourceRepository.FindResource<TextureView>("dot"),
				Numbers = numTextures,
				Layout = Matrix.RotationYawPitchRoll(1.0f, -1.5f, 0.0f) * Matrix.Translation(1.5f, 2.5f, 4.5f)
			});

			// create floor entity
			var floorModel = DrawModel.CreateFloor(20.0f, 10.0f, Vector4.Zero);
			m_floor = new ModelEntity(new ModelEntity.InitParam()
			{
				Model = floorModel,
				Texture = drawSys.ResourceRepository.FindResource<TextureView>("floor"),
				Layout = Matrix.Identity,
				Delay = 0.0f,
				Forward = Vector3.Zero,
				Color = Color4.White,
				Speed = 1,
			});
			m_drawModelList.Add(floorModel);
			m_multiThreadCount = multiThreadCount;
			m_taskList = new List<Task>(m_multiThreadCount);
			m_taskResultList = new List<CommandList>(m_multiThreadCount);
		}

        public void RenderFrame()
		{
			double dt = m_fps.GetDeltaTime();

			var drawSys = DrawSystem.GetInstance();

			// update fps
			{
				double avgDT = m_fps.GetAverageDeltaTime();
				string text = String.Format("FPS:{0:f2}, DeltaTime:{1:f2}ms", 1.0 / avgDT, avgDT * 1000.0f);
				m_numberEntity.SetNumber(1.0f / (float)avgDT);
			}


			if (m_multiThreadCount > 1)
			{
				Task.WaitAll(m_taskList.ToArray());
				var tmpTaskResult = new List<CommandList>(m_taskResultList);
				var context = drawSys.BeginScene();

				// start command list generation for the next frame
				int spanIndex = m_boxList.Count() / m_multiThreadCount;
				m_taskList.Clear();
				m_taskResultList.Clear();
				m_taskResultList.AddRange(Enumerable.Repeat<CommandList>(null, m_multiThreadCount));
				m_accTime += dt;
				for (int threadIndex = 0; threadIndex < m_multiThreadCount; ++threadIndex)
				{
					int startIndex = spanIndex * threadIndex;
					int resultIndex = threadIndex;

					var subThreadContext = drawSys.GetSubThreadContext(threadIndex);
					m_taskList.Add(Task.Run(() =>
					{
						m_boxList[startIndex].BeginDrawInstance(subThreadContext);
						for (int index = startIndex; index < startIndex + spanIndex; ++index)
						{
							var entity = m_boxList[index];
							float frame = (float)m_accTime + entity.Delay;
							float angle = frame % (2.0f * (float)Math.PI);
							entity.SetPose(new Vector3(angle, angle, angle),
								((frame * entity.Speed) % 20.0f) * entity.Forward);

							entity.AddInstance(subThreadContext);
						}
						subThreadContext.EndDrawInstance();
						m_taskResultList[resultIndex] = subThreadContext.FinishCommandList();
					}));
				}

				// draw floor
				m_floor.Draw(context);

				foreach (var result in tmpTaskResult)
				{
					context.ExecuteCommandList(result);
				}

				m_numberEntity.Draw(context);
				drawSys.EndScene();
			}
			else
			{
				var context = drawSys.BeginScene();

				// draw floor
				m_floor.Draw(context);

				// draw block entities
				m_accTime += dt;
				m_boxList[0].BeginDrawInstance(context);
				foreach (var entity in m_boxList)
				{
					float frame = (float)m_accTime + entity.Delay;
					float angle = frame % (2.0f * (float)Math.PI);
					entity.SetPose(new Vector3(angle, angle, angle),
						((frame * entity.Speed) % 5.0f) * entity.Forward);

					entity.AddInstance(context);
				}
				context.EndDrawInstance();

				m_numberEntity.Draw(context);
				drawSys.EndScene();
			}

			m_fps.EndFrame();
			m_fps.BeginFrame();
		}

        /// <summary>
        /// Release all com resources.
        /// </summary>
        public void Dispose()
        {
			Task.WaitAll(m_taskList.ToArray());
			m_numberEntity.Dispose();
			m_floor.Dispose();
			foreach (var entity in m_boxList)
			{
				entity.Dispose();
			}
			foreach (var model in m_drawModelList)
			{
				model.Dispose();
			}
		}

		#region private members

		private FpsCounter m_fps;
		private List<DrawModel> m_drawModelList = new List<DrawModel>();
		private List<ModelEntity> m_boxList = new List<ModelEntity>();
		private ModelEntity m_floor;
		private NumberEntity m_numberEntity = null;
		private double m_accTime = 0;
		private int m_multiThreadCount;
		private List<Task> m_taskList = null;
		private List<CommandList> m_taskResultList = null;

		#endregion // private members
	}
}
