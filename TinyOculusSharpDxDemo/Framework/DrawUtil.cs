using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
    public static class DrawUtil
    {
		public static Buffer CreateConstantBuffer<Type>(DrawSystem.D3DData d3d) where Type : struct
		{
			int size = Utilities.SizeOf<Type>();
			Debug.Assert(size % 16 == 0, "size of constant buffer must be aligned to 16 byte");
			return CreateConstantBuffer(d3d, size);
		}

		public static Buffer CreateConstantBuffer(DrawSystem.D3DData d3d, int size)
		{
			return new Buffer(d3d.device, size, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
		}

		public static Buffer CreateVertexBuffer<Type>(DrawSystem.D3DData d3d, Type[] vertices) where Type : struct
		{
			var desc = new BufferDescription
			{
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = Utilities.SizeOf<Type>() * vertices.Length,
				StructureByteStride = Utilities.SizeOf<Type>(),
				Usage = ResourceUsage.Default,
			};
			return Buffer.Create(d3d.device, vertices, desc);
		}

		/// <summary>
		/// create a mesh data from vertex array
		/// </summary>
		/// <typeparam name="Type">type of vertex</typeparam>
		/// <param name="d3d">d3d data</param>
		/// <param name="topology">primitive topology</param>
		/// <param name="vertices">vertex array1</param>
		/// <returns>mesh data</returns>
		public static DrawSystem.MeshData CreateMeshData<Type1>(DrawSystem.D3DData d3d, PrimitiveTopology topology, Type1[] vertices1)
			where Type1 : struct
		{
			var data = DrawSystem.MeshData.Create(1);
			data.VertexCount = vertices1.Length;
			data.Buffer = new VertexBufferBinding(CreateVertexBuffer<Type1>(d3d, vertices1), Utilities.SizeOf<Type1>(), 0);
			data.Topology = topology;
			return data;
		}
	}
}
