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
		public static DrawModel CreateGizmo(String uid, float length)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;

			var vertices = new _VertexDebug[6];

			// x axis
			vertices[0].Position = new Vector4(0, 0, 0, 1);
			vertices[0].Color = new Color4(1, 0, 0, 1);
			vertices[1].Position = new Vector4(length, 0, 0, 1);
			vertices[1].Color = vertices[0].Color;

			// y axis
			vertices[2].Position = new Vector4(0, 0, 0, 1);
			vertices[2].Color = new Color4(0, 1, 0, 1);
			vertices[3].Position = new Vector4(0, length, 0, 1);
			vertices[3].Color = vertices[2].Color;

			// z axis
			vertices[4].Position = new Vector4(0, 0, 0, 1);
			vertices[4].Color = new Color4(0, 0, 1, 1);
			vertices[5].Position = new Vector4(0, 0, length, 1);
			vertices[5].Color = vertices[4].Color;

			var model = new DrawModel(uid);
			model.m_nodeList.Add(new Node()
			{
				Mesh = DrawUtil.CreateMeshData<_VertexDebug>(d3d, PrimitiveTopology.LineList, vertices),
				Material = new DrawSystem.MaterialData()
				{
					Type = DrawSystem.MaterialTypes.Default,
					DiffuseTex0 = null,
					BumpTex0 = null,
				},
				IsDebug = true,
				HasBone = false,
			});

			return model;
		}

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
				Material = new DrawSystem.MaterialData()
				{
					Type = DrawSystem.MaterialTypes.Default,
					DiffuseTex0 = null,
					BumpTex0 = null,
				},
				IsDebug = true,
				HasBone = false,
			});

			return model;
		}

		public static DrawModel CreateBone(String uid, float length, Color4 color, Vector4 offset)
		{
			var drawSys = DrawSystem.GetInstance();
			var d3d = drawSys.D3D;
			float w = 0.1f * length;
			float L = length;

			var vertices = new _VertexDebug[]
			{
				// bottom pyramid
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( 0,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w,  w, 1) },

				// middle plane
				new _VertexDebug() { Position = new Vector4( w,  w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w,  w, 1) },

				// top pyramid
				new _VertexDebug() { Position = new Vector4( L,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( L,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w, -w,  w, 1) },
				new _VertexDebug() { Position = new Vector4( L,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w, -w, 1) },
				new _VertexDebug() { Position = new Vector4( L,  0,  0, 1) },
				new _VertexDebug() { Position = new Vector4( w,  w,  w, 1) },
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
				Material = new DrawSystem.MaterialData()
				{
					Type = DrawSystem.MaterialTypes.Default,
					DiffuseTex0 = null,
					BumpTex0 = null,
				},
				IsDebug = true,
				HasBone = false,
			});

			return model;
		}
	}
}
