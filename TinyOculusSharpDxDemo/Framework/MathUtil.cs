using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;

namespace TinyOculusSharpDxDemo
{
	public static class MathUtil
	{
		public const float E = 2.71828f;
		public const float PI = 3.14159f;

		public static Vector3 Transform3(Vector3 v, Matrix M)
		{
			Vector4 tmp = Vector3.Transform(v, M);
			return ToVector3(tmp);
		}

		public static Vector3 ToVector3(Vector4 v)
		{
			return new Vector3(v.X , v.Y, v.Z);
		}

		/// <summary>
		/// get matrix to transform project space to uv space
		/// </summary>
		/// <returns>matrix</returns>
		public static Matrix GetProj2UvMatrix()
		{
			var m = new Matrix();
			m.Row1 = new Vector4(0.5f, 0.0f, 0.0f, 0.0f);
			m.Row2 = new Vector4(0.0f,-0.5f, 0.0f, 0.0f);
			m.Row3 = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
			m.Row4 = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
			return m;
		}

		public static Vector3 ComputeFaceTangent(Vector4 pos1, Vector4 pos2, Vector4 pos3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
		{
			Vector4 e1 = pos2 - pos1;
			Vector4 e2 = pos3 - pos1;
			Vector2 s = uv2 - uv1;
			Vector2 t = uv3 - uv1;

			float invDet = s.X * t.Y - s.Y * t.X;
			if (invDet == 0)
			{
				//Debug.Assert(invDet != 0, "Degenerate uv Found!");
				return Vector3.Zero;
			}
			float det = 1.0f / invDet;

			Vector3 tangent;
			tangent.X = det * (t.Y * e1.X - s.Y * e2.X);
			tangent.Y = det * (t.Y * e1.Y - s.Y * e2.Y);
			tangent.Z = det * (t.Y * e1.Z - s.Y * e2.Z);
			tangent.Normalize();

			return tangent;
		}

		public static float Clamp(float x, float min, float max)
		{
			return Math.Min(Math.Max(min, x), max);
		}

		private static Random m_rnd = new Random();
		public static Random GetRandom()
		{
			return m_rnd;
		}

		public static Matrix CreateMatrix(Matrix3x3 m, Vector4 v)
		{
			return new Matrix()
			{
				M11 = m.M11,
				M12 = m.M12,
				M13 = m.M13,
				M14 = 0,
				M21 = m.M21,
				M22 = m.M22,
				M23 = m.M23,
				M24 = 0,
				M31 = m.M31,
				M32 = m.M32,
				M33 = m.M33,
				M34 = 0,
				M41 = v.X,
				M42 = v.Y,
				M43 = v.Z,
				M44 = v.W,
			};
		}

		public static float SinF(float f)
		{
			return (float)Math.Sin((double)f);
		}

		public static float CosF(float f)
		{
			return (float)Math.Cos((double)f);
		}

		public static float TanF(float f)
		{
			return (float)Math.Tan((double)f);
		}

		public static void Orthonormalize(ref Vector3 v1, ref Vector3 v2)
		{
			var srcArray = new Vector3[] { v1, v2 };
			var destArray = new Vector3[] { v1, v2 };
			Vector3.Orthonormalize(destArray, srcArray);
			v1 = destArray[0];
			v2 = destArray[1];
		}
	}
}
