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
			public Matrix Camera;
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

	}
}
