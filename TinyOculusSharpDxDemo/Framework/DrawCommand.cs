using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// Draw Command Type
	/// </summary>
	public enum DrawCommandTypes
	{
		// value is rendering order
		Invalid = 0,
		DrawModel,
	}

	/// <summary>
	/// Draw command
	/// </summary>
	public struct DrawCommand
	{
		public DrawCommandTypes m_type;
		public object CommandData;

		#region DrawModel

		public static DrawCommand CreateDrawModelCommand(Matrix worldTrans, DrawSystem.MeshData mesh, TextureView texture)
		{
			return new DrawCommand()
			{
				m_type = DrawCommandTypes.DrawModel,
				CommandData = new DrawModelCommandData()
				{
					m_worldTransform = worldTrans,
					m_mesh = mesh,
					m_texture = texture,
				}
			};
		}

		public DrawModelCommandData GetDrawModelData()
		{
			return (DrawModelCommandData)CommandData;
		}

		#endregion // DrawModel
	}

	/// <summary>
	/// Command data for 'DrawModel'
	/// </summary>
	public class DrawModelCommandData
	{
		public Matrix m_worldTransform;
		public DrawSystem.MeshData m_mesh;
		public TextureView m_texture;
	}
}
