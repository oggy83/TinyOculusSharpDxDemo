using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public partial class DrawSystem
	{
		/// <summary>
		/// directional light 
		/// </summary>
		public struct DirectionalLightData
		{
			/// <summary>
			/// direction in world coords
			/// </summary>
			public Vector3 Direction;

			/// <summary>
			/// color 
			/// </summary>
			public Color3 Color;

			public DirectionalLightData(Vector3 dir, Color3 col)
			{
				this.Direction = dir;
				this.Color = col;
			}

			public Matrix GetViewMatrix()
			{
				// make view matrix from directional light
				DrawSystem.CameraData camera;
				camera.lookAt = new Vector3(0, 0, 0);//MathUtil.ToVector3(m_worldTransform.get_Rows(3));
				camera.eye = camera.lookAt + Direction * -20;// eye at 20m off
				camera.up = new Vector3(0, 1, 0);

				return camera.GetViewMatrix();
			}
		}

		public struct PointLightData
		{
			/// <summary>
			/// light position
			/// </summary>
			public Vector4 Position;

			/// <summary>
			/// light range [m]
			/// </summary>
			public float Range;

			/// <summary>
			/// light color 
			/// </summary>
			public Color3 Color;

			public PointLightData(Vector4 position, float range, Color3 col)
			{
				this.Position = position;
				this.Range = range;
				this.Color = col;
			}

		}

		/// <summary>
		/// camera
		/// </summary>
		public struct CameraData
		{
			public Vector3 eye;
			public Vector3 lookAt;
			public Vector3 up;

			public CameraData(Vector3 eye, Vector3 lookAt, Vector3 up)
			{
				this.eye = eye;
				this.lookAt = lookAt;
				this.up = up;
			}

			public Matrix GetViewMatrix()
			{
				return Matrix.LookAtLH(eye, lookAt, up);
			}
		}

		/// <summary>
		/// world data (camera + light)
		/// </summary>
		public struct WorldData
		{
			public DrawSystem.CameraData camera;
			public Color3 ambientCol;
			public DrawSystem.DirectionalLightData dirLight;
			public DrawSystem.PointLightData[] pointLights;
		}

		public struct D3DData
		{
			public Device device;
			public DeviceContext context;
			public SwapChain swapChain;
			public IntPtr hWnd;
		}

		public struct MeshData
		{
			public int VertexCount { get; set; }
			public VertexBufferBinding[] Buffers { get; set; }
			public PrimitiveTopology Topology { get; set; }

			public static MeshData Create(int bufferCount)
			{
				var data = new MeshData()
				{
					VertexCount = 0,
					Buffers = new VertexBufferBinding[bufferCount],
					Topology = PrimitiveTopology.TriangleList
				};

				return data;
			}
		}

	}
}
