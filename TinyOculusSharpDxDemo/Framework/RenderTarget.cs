using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class represents render target
	/// </summary>
	public class RenderTarget : ResourceBase
	{
		public Texture2D TargetTexture { get; set; }
		public DepthStencilView DepthStencilView { get; set; }
		public RenderTargetView TargetView { get; set; }

		public RenderTarget(String uid)
			: base(uid)
		{
			TargetView = null;
			TargetTexture = null;
			DepthStencilView = null;			
		}

		/// <summary>
		/// create a default render target as ResourceBase
		/// </summary>
		/// <param name="d3d">Direct3D data</param>
		/// <returns>render target</returns>
		public static RenderTarget CreateDefaultRenderTarget(DrawSystem.D3DData d3d)
		{
			var res = new RenderTarget("Default");
			
			var backBuffer = Texture2D.FromSwapChain<Texture2D>(d3d.swapChain, 0);
			var depthBuffer = new Texture2D(d3d.device, new Texture2DDescription()
			{
				Format = Format.D32_Float_S8X24_UInt,
				ArraySize = 1,
				MipLevels = 1,
				Width = backBuffer.Description.Width,
				Height = backBuffer.Description.Height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			res.TargetView = new RenderTargetView(d3d.device, backBuffer);
			res.DepthStencilView = new DepthStencilView(d3d.device, depthBuffer);
			res._AddDisposable(res.TargetView);
			res._AddDisposable(res.DepthStencilView);

			return res;
		}

	}
}
