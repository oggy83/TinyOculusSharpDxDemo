using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Diagnostics;


namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// vertex/pixel shader + vertex declarations
	/// </summary>
	public class Effect : ResourceBase
	{
		public InputLayout Layout { get; set; }
		public VertexShader VertexShader { get; set; }
		public PixelShader PixelShader { get; set;  }

		public Effect(String uid)
			: base(uid)
		{
			Layout = null;
			VertexShader = null;
			PixelShader = null;
		}

		public Effect(String uid, DrawSystem.D3DData d3d, InputElement[] inputElements, String vertexShader, String pixelShader)
			: this(uid)
		{
			var vertexByteCode = ShaderBytecode.CompileFromFile(vertexShader, "main", "vs_4_0", ShaderFlags.Debug);
			var pixelByteCode = ShaderBytecode.CompileFromFile(pixelShader, "main", "ps_4_0", ShaderFlags.Debug);

            VertexShader = new VertexShader(d3d.device, vertexByteCode);
            PixelShader = new PixelShader(d3d.device, pixelByteCode);
			Layout = new InputLayout(d3d.device, ShaderSignature.GetInputSignature(vertexByteCode), inputElements);

			_AddDisposable(VertexShader);
			_AddDisposable(PixelShader);
			_AddDisposable(Layout);
		}
	}
}
