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
	public class NumberEntity : IDisposable, IDrawable
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
			m_text = "".ToArray();
		}

		public void Dispose()
		{
			m_plane.Dispose();
		}

		public void SetNumber(float num)
		{
			num = MathUtil.Clamp(num, 0, 999.9f);
			m_text = String.Format("{0:f1}", num).ToArray();
		}

		public void Draw(IDrawContext context)
		{
			var drawSys = DrawSystem.GetInstance();
			Matrix layout = m_initParam.Layout;
			foreach (char c in m_text)
			{
				TextureView tex = null;
				float offset = 0.0f;
				switch (c)
				{
					case '.':
						tex = m_initParam.Dot;
						offset = 0.22f;
						break;
					default:
						if ('0' <= c && c <= '9')
						{
							int num = (int)c - (int)'0';
							tex = m_initParam.Numbers[num];
							offset = 0.3f;
						}
						break;
				}

				Debug.Assert(tex != null, "invalid character");
				context.DrawModel(layout, m_plane.Mesh, tex, DrawSystem.RenderMode.Transparency);
				layout *= Matrix.Translation(offset, 0, 0);
			}

		}

		#region private members

		private InitParam m_initParam;
		private DrawModel m_plane;
		private char[] m_text;

		#endregion // private members
	}
}
