using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// draw model class
	/// </summary>
	public partial class DrawModel : IDisposable
	{
		#region properties

		private DrawSystem.MeshData m_mesh = new DrawSystem.MeshData();
		public DrawSystem.MeshData Mesh
		{
			get
			{
				return m_mesh;
			}
		}

		#endregion // properties

		#region inner class

		private struct _VertexDebug
		{
			public Vector4 Position;
			public Color4 Color;
			public Vector2 UV;
			public Vector3 Normal;
		}

		#endregion // inner class

		public DrawModel()
		{
			// nothing
		}

		public void Dispose()
		{
			m_mesh.Buffer.Buffer.Dispose();
		}

		public static DrawModel CreateBox(float geometryScale, float uvScale, Color4 color, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float gs = geometryScale;
			float us = uvScale;

			var vertices = new _VertexDebug[]
			{
				// top plane
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  gs, 1), UV = new Vector2(us, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  -gs, 1), UV = new Vector2(0, us), Normal = Vector3.UnitY },

				// bottom plane
				new _VertexDebug() { Position = new Vector4( -gs, -gs,  gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  gs, 1), UV = new Vector2(us, 0), Normal = -Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs, -gs,  gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs, -gs,  -gs, 1), UV = new Vector2(0, us), Normal = -Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitY },

				// forward plane
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  -gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  -gs, 1), UV = new Vector2(us, 0), Normal = -Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  -gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( -gs,  -gs,  -gs, 1), UV = new Vector2(0, us), Normal = -Vector3.UnitZ },

				// back plane
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  gs, 1), UV = new Vector2(us, 0), Normal = Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( -gs,  -gs,  gs, 1), UV = new Vector2(0, us), Normal = Vector3.UnitZ },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitZ },

				// left plane
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  -gs, 1), UV = new Vector2(us, 0), Normal = -Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( -gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( -gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = -Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( -gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = -Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( -gs,  -gs,  gs, 1), UV = new Vector2(0, us), Normal = -Vector3.UnitX },
				
				// right plane
				new _VertexDebug() { Position = new Vector4( gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  -gs, 1), UV = new Vector2(us, 0), Normal = Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( gs,  gs,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  gs, 1), UV = new Vector2(0, us), Normal = Vector3.UnitX },
				new _VertexDebug() { Position = new Vector4( gs,  -gs,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitX },
				
			};

			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
			}

			var model = new DrawModel();
			model.m_mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.TriangleList, vertices);

			return model;
		}

		public static DrawModel CreateFloor(float geometryScale, float uvScale, Color4 color, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float gs = geometryScale;
			float us = uvScale;

			var vertices = new _VertexDebug[]
			{
				new _VertexDebug() { Position = new Vector4( -gs,  0,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  0,  gs, 1), UV = new Vector2(us, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  0,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs,  0,  gs, 1), UV = new Vector2(0, 0), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( gs,  0,  -gs, 1), UV = new Vector2(us, us), Normal = Vector3.UnitY },
				new _VertexDebug() { Position = new Vector4( -gs,  0,  -gs, 1), UV = new Vector2(0, us), Normal = Vector3.UnitY },
			};

			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
			}

			var model = new DrawModel();
			model.m_mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.TriangleList, vertices);

			return model;
		}
	}
}
