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
		DrawText,
	}

	/// <summary>
	/// Draw command
	/// </summary>
	public struct DrawCommand
	{
		public DrawCommandTypes m_type;
		public object CommandData;

		#region DrawModel

		public static DrawCommand CreateDrawModelCommand(Matrix worldTrans, DrawSystem.MeshData mesh, Matrix[] boneMatrices)
		{
			return new DrawCommand()
			{
				m_type = DrawCommandTypes.DrawModel,
				CommandData = new DrawModelCommandData()
				{
					m_worldTransform = worldTrans,
					m_mesh = mesh,
					m_boneMatrices = boneMatrices,
				}
			};
		}

		public DrawModelCommandData GetDrawModelData()
		{
			return (DrawModelCommandData)CommandData;
		}

		#endregion // DrawModel

		#region DrawText

		public static DrawCommand CreateDrawTextCommand(string text)
		{
			return new DrawCommand()
			{
				m_type = DrawCommandTypes.DrawText,
				CommandData = new DrawTextCommandData()
				{
					m_text = text,
				}
			};
		}

		public DrawTextCommandData GetDrawTextData()
		{
			return (DrawTextCommandData)CommandData;
		}

		#endregion // DrawText
	}

	/// <summary>
	/// Command data for 'DrawModel'
	/// </summary>
	public class DrawModelCommandData
	{
		public Matrix m_worldTransform;
		public DrawSystem.MeshData m_mesh;

		/// <summary>
		/// bone matrix array (model coords)
		/// </summary>
		public Matrix[] m_boneMatrices;
	}

	/// <summary>
	/// Command data for 'DrawText'
	/// </summary>
	public class DrawTextCommandData
	{
		public string m_text;
	}
}
