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
			//return new Buffer(d3d.device, size, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
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
			data.Buffers[0] = new VertexBufferBinding(CreateVertexBuffer<Type1>(d3d, vertices1), Utilities.SizeOf<Type1>(), 0);
			data.Topology = topology;
			return data;
		}

		public static DrawSystem.MeshData CreateMeshData<Type1, Type2>(DrawSystem.D3DData d3d, PrimitiveTopology topology, Type1[] vertices1, Type2[] vertices2)
			where Type1 : struct
			where Type2 : struct
		{
			Debug.Assert(vertices1.Length == vertices2.Length, "the lengths of vertex buffer are unmatched");

			var data = DrawSystem.MeshData.Create(2);
			data.VertexCount = vertices1.Length;
			data.Buffers[0] = new VertexBufferBinding(CreateVertexBuffer<Type1>(d3d, vertices1), Utilities.SizeOf<Type1>(), 0);
			data.Buffers[1] = new VertexBufferBinding(CreateVertexBuffer<Type2>(d3d, vertices2), Utilities.SizeOf<Type2>(), 0);
			data.Topology = topology;
			return data;
		}

		public static DrawSystem.MeshData CreateMeshData<Type1, Type2, Type3>(DrawSystem.D3DData d3d, PrimitiveTopology topology, Type1[] vertices1, Type2[] vertices2, Type3[] vertices3)
			where Type1 : struct
			where Type2 : struct
			where Type3 : struct
		{
			Debug.Assert(vertices1.Length == vertices2.Length && vertices1.Length == vertices3.Length, "the lengths of vertex buffer are unmatched");

			var data = DrawSystem.MeshData.Create(3);
			data.VertexCount = vertices1.Length;
			data.Buffers[0] = new VertexBufferBinding(CreateVertexBuffer<Type1>(d3d, vertices1), Utilities.SizeOf<Type1>(), 0);
			data.Buffers[1] = new VertexBufferBinding(CreateVertexBuffer<Type2>(d3d, vertices2), Utilities.SizeOf<Type2>(), 0);
			data.Buffers[2] = new VertexBufferBinding(CreateVertexBuffer<Type3>(d3d, vertices3), Utilities.SizeOf<Type3>(), 0);
			data.Topology = topology;
			return data;
		}

		

	}
}
