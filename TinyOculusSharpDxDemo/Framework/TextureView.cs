using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Diagnostics;


namespace TinyOculusSharpDxDemo
{
	public class TextureView : ResourceBase
	{
		public ShaderResourceView View { get; set; }
		public SamplerState SamplerState { get; set; }

		public TextureView(string uid)
			: base(uid)
		{
			View = null;
			SamplerState = null;
		}

		public static TextureView FromFile(string uid, DrawSystem.D3DData d3d, string filePath)
		{
			var result = new TextureView(uid);

			var texRes = Texture2D.FromFile<Texture2D>(d3d.Device, filePath);
			result.View = new ShaderResourceView(d3d.Device, texRes);

			var desc = new SamplerStateDescription()
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				BorderColor = Color.Black,
				ComparisonFunction = Comparison.Never,
				MaximumAnisotropy = 16,
				MipLodBias = 0,
				MinimumLod = 0,
				MaximumLod = 16,
			};
			result.SamplerState = new SamplerState(d3d.Device, desc);

			result._AddDisposable(texRes);
			result._AddDisposable(result.View);
			result._AddDisposable(result.SamplerState);

			return result;
		}

		/// <summary>
		/// set texture parameters to device context
		/// </summary>
		/// <param name="slot">slot index</param>
		/// <param name="context">device context</param>
		public void SetContext(int slot, DeviceContext context)
		{
			context.PixelShader.SetShaderResource(slot, View);
			context.PixelShader.SetSampler(slot, SamplerState);
		}
	}
}
