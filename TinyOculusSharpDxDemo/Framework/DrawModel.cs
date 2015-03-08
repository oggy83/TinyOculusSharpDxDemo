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

		#endregion // properties

		#region inner class

		public class Node
		{
            public DrawSystem.MeshData Mesh;
		}

		private struct _VertexDebug
		{
			public Vector4 Position;
			public Color4 Color;
			public Vector2 UV;
		}

		#endregion // inner class

		public DrawModel(String uid)
			: base(uid)
		{
			m_nodeList = new List<Node>();
		}


	}
}
