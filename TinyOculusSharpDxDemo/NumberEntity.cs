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
	public class NumberEntity : IDisposable
	{
		public struct InitParam
		{
			public TextureView Dot;
			public TextureView[] Numbers;
			public Matrix Layout;
		}

		public NumberEntity(InitParam initParam)
		{
			Debug.Assert(initParam.Numbers.Length == 10, "set texture array whitch represents 0-9 number");
			m_initParam = initParam;
			m_plane = DrawModel.CreateFloor(0.3f, 1.0f, Color4.White, Vector4.Zero);
		}

		public void Dispose()
		{
			m_plane.Dispose();
		}

		public void SetNumber(float num)
		{
			// @todo yasut
			//String.Format("{0:f2}", 1.0 / avgDT)
		}

		public void Draw()
		{
			var drawSys = DrawSystem.GetInstance();
			drawSys.AddDrawCommand(DrawCommand.CreateDrawModelCommand(m_initParam.Layout, m_plane.Mesh, m_initParam.Numbers[0]));
		}

		#region private members

		private InitParam m_initParam;
		private DrawModel m_plane;

		#endregion // private members
	}
}
