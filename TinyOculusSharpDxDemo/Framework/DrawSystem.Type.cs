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
		}

		/// <summary>
		/// world data (camera + light)
		/// </summary>
		public struct WorldData
		{
			public CameraData Camera;
			public Color3 AmbientColor;
			public Color3 FogColor;
			public DrawSystem.DirectionalLightData DirectionalLight;
            public float NearClip;
            public float FarClip;
		}

		public struct D3DData
		{
			public Device Device;
			public SwapChain SwapChain;
			public IntPtr WindowHandle;
		}

		public enum RenderMode
		{
			Opaque = 0,		// disable alpha blend
			Transparency,	// enable alpha blend
		}

		public struct MeshData
		{
			public int VertexCount { get; set; }
			public VertexBufferBinding Buffer { get; set; }
			public PrimitiveTopology Topology { get; set; }

			public static MeshData Create(int bufferCount)
			{
				var data = new MeshData()
				{
					VertexCount = 0,
					Topology = PrimitiveTopology.TriangleList
				};

				return data;
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

			public static CameraData GetIdentity()
			{
				return new CameraData(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			}

			public static CameraData Conbine(CameraData a, CameraData b)
			{
				var z = (a.lookAt - a.eye);
				z.Normalize();
				var x = Vector3.Cross(a.up, z);
				x.Normalize();
				var y = Vector3.Cross(z, x);

				var trans = new Matrix3x3(x.X, x.Y, x.Z, y.X, y.Y, y.Z, z.X, z.Y, z.Z);

				var result = new CameraData(
					Vector3.Transform(b.eye, trans),
					Vector3.Transform(b.lookAt, trans),
					Vector3.Transform(b.up, trans));
				result.eye += a.eye;
				result.lookAt += a.eye;
				result.up.Normalize();
				return result;
			}
		}

	}
}
