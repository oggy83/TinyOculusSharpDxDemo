using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// Draw command
	/// </summary>
	public struct DrawCommand
	{
		public Matrix m_worldTransform;
		public DrawSystem.MeshData m_mesh;
		public TextureView m_texture;
	}
}
