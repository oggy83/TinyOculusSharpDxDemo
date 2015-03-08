using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;

namespace TinyOculusSharpDxDemo
{
	public partial class DrawModel
	{
		public static DrawModel CreateBox(String uid, float scale, Color4 color, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float s = scale;

			var vertices = new _VertexDebug[]
			{
				// x-axis lines
				new _VertexDebug() { Position = new Vector4(-s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4( s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s, -s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s, -s, 1) },

				// y-axis lines
				new _VertexDebug() { Position = new Vector4( s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s, -s, 1) },
				new _VertexDebug() { Position = new Vector4(-s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s, -s, 1) },

				// z-axis lines
				new _VertexDebug() { Position = new Vector4( s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4(-s,  s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s,  s, -s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4( s, -s, -s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s,  s, 1) },
				new _VertexDebug() { Position = new Vector4(-s, -s, -s, 1) },
			};

			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
			}

			var model = new DrawModel(uid);
			model.m_nodeList.Add(new Node()
			{
				Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.LineList, vertices),
			});

			return model;
		}

		public static DrawModel CreateFloor(String uid, float geometryScale, float uvScale, Color4 color, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float gs = geometryScale;
			float us = uvScale;

			var vertices = new _VertexDebug[]
			{
				new _VertexDebug() { Position = new Vector4( -gs,  0,  gs, 1), UV = new Vector2(0f, 0) },
				new _VertexDebug() { Position = new Vector4( gs,  0,  gs, 1), UV = new Vector2(us, 0) },
				new _VertexDebug() { Position = new Vector4( gs,  0,  -gs, 1), UV = new Vector2(us, us) },
				new _VertexDebug() { Position = new Vector4( -gs,  0,  gs, 1), UV = new Vector2(0, 0) },
				new _VertexDebug() { Position = new Vector4( gs,  0,  -gs, 1), UV = new Vector2(us, us) },
				new _VertexDebug() { Position = new Vector4( -gs,  0,  -gs, 1), UV = new Vector2(0, us) },
			};

			for (int i = 0; i < vertices.Length; ++i)
			{
				vertices[i].Position += offset;
				vertices[i].Position.W = 1;
				vertices[i].Color = color;
			}

			var model = new DrawModel(uid);
			model.m_nodeList.Add(new Node()
			{
				Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.TriangleList, vertices),
			});

			return model;
		}
	}
}
