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
	public partial class DrawModel : ResourceBase
	{
		#region properties

		private List<Node> m_nodeList = null;
		public ReadOnlyCollection<Node> NodeList
		{
			get
			{
				return m_nodeList.AsReadOnly();
			}
		}

		/// <summary>
		/// get a bone array 
		/// </summary>
		/// <remarks>in case of bone count is 0, BoneArray is null</remarks>
		private DrawSystem.BoneData[] m_boneArray = null;
		public DrawSystem.BoneData[] BoneArray
		{
			get
			{
				return m_boneArray;
			}
		}

		#endregion // properties

		#region inner class

		public class Node
		{
            public DrawSystem.MeshData Mesh;
			public DrawSystem.MaterialData Material;
			public bool IsDebug;
			public bool HasBone;
		}

		private struct _VertexCommon
		{
			public Vector4 Position;
			public Vector3 Normal;
			public Vector2 Texcoord;
		}

		private struct _VertexBoneWeight
		{
			public const int MAX_COUNT = 4;

			public uint Index0;
			public uint Index1;
			public uint Index2;
			public uint Index3;

			public float Weight0;
			public float Weight1;
			public float Weight2;
			public float Weight3;
		}

		private struct _VertexDebug
		{
			public Vector4 Position;
			public Color4 Color;
		}

		#endregion // inner class

		public DrawModel(String uid)
			: base(uid)
		{
			m_nodeList = new List<Node>();
			m_boneArray = null;
		}

		public static DrawModel FromScene(String uid, BlenderScene scene)
		{
			var drawSys = DrawSystem.GetInstance();
			var drawRepository = drawSys.ResourceRepository;
			var d3d = drawSys.D3D;

			var model = new DrawModel(uid);
			
			foreach (var n in scene.NodeList)
			{
				if (n.MaterialData.Type != DrawSystem.MaterialTypes.Default)
				{
					continue;
				}

				// Build a vertex buffer(s)
				var vertices1 = n.Vertics
					.Select(v => new _VertexCommon()
					{
						Position = v.Position,
						Normal = v.Normal,
						Texcoord = v.Texcoord
					}).ToArray();

				var vertices2 = n.Vertics
					.Select(v => v.Tangent).ToArray();

				var vertices3 = n.Vertics
					.Select(v =>
					{
						Debug.Assert(BlenderUtil.GetLengthOf(v.BoneIndices) == BlenderUtil.GetLengthOf(v.BoneWeights), "both of bone index and bone weight must be matched");
						Debug.Assert(BlenderUtil.GetLengthOf(v.BoneWeights) <= _VertexBoneWeight.MAX_COUNT, "length of bone weight is over :" + BlenderUtil.GetLengthOf(v.BoneWeights));
						return new _VertexBoneWeight()
						{
							Index0 = BlenderUtil.GetLengthOf(v.BoneIndices) > 0 ? v.BoneIndices[0] : 0,
							Weight0 = BlenderUtil.GetLengthOf(v.BoneWeights) > 0 ? v.BoneWeights[0] : 0.0f,
							Index1 = BlenderUtil.GetLengthOf(v.BoneIndices) > 1 ? v.BoneIndices[1] : 0,
							Weight1 = BlenderUtil.GetLengthOf(v.BoneWeights) > 1 ? v.BoneWeights[1] : 0.0f,
							Index2 = BlenderUtil.GetLengthOf(v.BoneIndices) > 2 ? v.BoneIndices[2] : 0,
							Weight2 = BlenderUtil.GetLengthOf(v.BoneWeights) > 2 ? v.BoneWeights[2] : 0.0f,
							Index3 = BlenderUtil.GetLengthOf(v.BoneIndices) > 3 ? v.BoneIndices[3] : 0,
							Weight3 = BlenderUtil.GetLengthOf(v.BoneWeights) > 3 ? v.BoneWeights[3] : 0.0f,
						};
					}).ToArray();


				var node = new Node();
				node.Mesh = DrawUtil.CreateMeshData(d3d, PrimitiveTopology.TriangleList, vertices1, vertices2, vertices3);
				node.Material = n.MaterialData;
				node.IsDebug = false;
				node.HasBone = BlenderUtil.GetLengthOf(n.BoneArray) > 0;

				// add dispoable
				foreach (var buf in node.Mesh.Buffers)
				{
					model._AddDisposable(buf.Buffer);
				}

				// create skeleton
				if (model.m_boneArray == null && n.BoneArray != null)
				{
					model.m_boneArray = (DrawSystem.BoneData[])n.BoneArray.Clone();
				}

				// load new texture
				foreach (var texName in n.TextureNames.Values)
				{
					if (!drawRepository.Contains(texName))
					{
						var tex = TextureView.FromFile(texName, drawSys.D3D, "Image/" + texName);
						drawRepository.AddResource(tex);
					}
				}

				// copy textures from cache
				if (n.TextureNames.ContainsKey(DrawSystem.TextureTypes.Diffuse0))
				{
					node.Material.DiffuseTex0 = drawRepository.FindResource<TextureView>(n.TextureNames[DrawSystem.TextureTypes.Diffuse0]);
				}
				if (n.TextureNames.ContainsKey(DrawSystem.TextureTypes.Bump0))
				{
					node.Material.BumpTex0 = drawRepository.FindResource<TextureView>(n.TextureNames[DrawSystem.TextureTypes.Bump0]);
				}

				model.m_nodeList.Add(node);
			}

			return model;
		}


		public static DrawModel CreateTangentFrame(String uid, BlenderScene scene)
		{
			var drawSys = DrawSystem.GetInstance();
			var drawRepository = drawSys.ResourceRepository;
			var d3d = drawSys.D3D;

			var model = new DrawModel(uid);

			foreach (var n in scene.NodeList)
			{
				if (n.MaterialData.Type != DrawSystem.MaterialTypes.Default)
				{
					continue;
				}

				int polyCount = n.Vertics.Length / 3;
				var vertices = new _VertexDebug[polyCount * 6 * 3];
				int nextVIndex = 0;
				for (int polyIndex = 0; polyIndex < polyCount; ++polyIndex)
				{
					var normalColor = new Color4(0, 0, 1, 1);
					var tangentColor = new Color4(1, 0, 0, 1);
					var binormalColor = new Color4(0, 1, 0, 1);
					float lineLength = 0.3f;

					for (int vIndex = 0; vIndex < 3; ++vIndex)
					{
						var pos = n.Vertics[polyIndex * 3 + vIndex].Position;
						var normal = n.Vertics[polyIndex * 3 + vIndex].Normal;
						var tangent = n.Vertics[polyIndex * 3 + vIndex].Tangent;
						var binormal = n.Vertics[polyIndex * 3 + vIndex].Binormal;

						// normal start vertex
						vertices[nextVIndex].Position = pos;
						vertices[nextVIndex].Color = normalColor;
						nextVIndex++;

						// normal end vertex
						vertices[nextVIndex].Position = pos + new Vector4(normal * lineLength, 0);
						vertices[nextVIndex].Color = normalColor;
						nextVIndex++;

						// tangent start vertex
						vertices[nextVIndex].Position = pos;
						vertices[nextVIndex].Color = tangentColor;
						nextVIndex++;

						// tangent end vertex
						vertices[nextVIndex].Position = pos + new Vector4(tangent * lineLength, 0);
						vertices[nextVIndex].Color = tangentColor;
						nextVIndex++;

						// binormal start vertex
						vertices[nextVIndex].Position = pos;
						vertices[nextVIndex].Color = binormalColor;
						nextVIndex++;

						// binormal end vertex
						vertices[nextVIndex].Position = pos + new Vector4(binormal * lineLength, 0);
						vertices[nextVIndex].Color = binormalColor;
						nextVIndex++;
					}

				}

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
			}


			return model;
		}

	}
}
