using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public partial class DrawContext
	{
		public struct CommonInitParam
		{
			public DrawSystem.D3DData D3D;
			public DrawResourceRepository Repository;
			public Buffer WorldVtxConst;
			public Buffer WorldPixConst;
			public RasterizerState RasterizerState;
			public BlendState[] BlendStates;
			public DepthStencilState[] DepthStencilStates;
		}

		#region private types

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _MainVertexShaderConst
		{
			public Matrix worldMat;			// word matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _WorldVertexShaderConst
		{
			public Matrix vpMat;			// view projection matrix
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _MainPixelShaderConst
		{
			public Color4 instanceColor;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 16)]
		private struct _WorldPixelShaderConst
		{
			public Color4 ambientCol;	// ambient color
			public Color4 fogCol;		// fog color
			public Color4 light1Col;	// light1 color
			public Vector4 cameraPos;	// camera position in model coords
			public Vector4 light1Dir;	// light1 direction in model coords
		}

		#endregion // private types


	}
}
