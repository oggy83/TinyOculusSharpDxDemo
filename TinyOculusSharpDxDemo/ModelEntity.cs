using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace TinyOculusSharpDxDemo
{
	public class ModelEntity 
	{
		#region public types

		public struct InitParam
		{
			public DrawModel Model;
			public TextureView Texture;
			public Matrix Layout;
			public float Delay;
			public Vector3 Forward;
			public Color4 Color;
			public float Speed;
		}

		#endregion // public types

		#region properties

		public float Delay
		{
			get
			{
				return m_initParam.Delay;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return m_initParam.Forward;
			}
		}

		public float Speed
		{
			get
			{
				return m_initParam.Speed;
			}
		}

		#endregion // properties

		public ModelEntity(InitParam initParam)
		{
			m_initParam = initParam;
			m_worldTrans = Matrix.Identity;
		}

		public void Dispose()
		{
			// nothing
		}

		public void SetPose(Vector3 rot, Vector3 offset)
		{
			m_worldTrans = Matrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) 
				* m_initParam.Layout 
				* Matrix.Translation(offset);
		}

		public void Draw(IDrawContext context)
		{
			context.DrawModel(m_worldTrans, m_initParam.Color, m_initParam.Model.Mesh, m_initParam.Texture, DrawSystem.RenderMode.Opaque);
		}

		public void BeginDrawInstance(IDrawContext context)
		{
			context.BeginDrawInstance(m_initParam.Model.Mesh, m_initParam.Texture, DrawSystem.RenderMode.Opaque);
		}

		public void AddInstance(IDrawContext context)
		{
			context.AddInstance(m_worldTrans, m_initParam.Color);
		}

		#region private members

		private InitParam m_initParam;
		private Matrix m_worldTrans;

		#endregion // private members
	}
}
