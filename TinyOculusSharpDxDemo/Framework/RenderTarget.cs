using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using System.Drawing;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// This class represents render target
	/// </summary>
	public class RenderTarget : ResourceBase
	{
		public ShaderResourceView ShaderResourceView { get; set; }
		public Texture2D TargetTexture { get; set; }
		public DepthStencilView DepthStencilView { get; set; }
		public RenderTargetView TargetView { get; set; }
		public Size Resolution 
		{
			get
			{
				return new Size(TargetTexture.Description.Width, TargetTexture.Description.Height);
			}
		}

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
			res.ShaderResourceView = null;
			res.TargetTexture = backBuffer;
			res._AddDisposable(res.TargetView);
			res._AddDisposable(res.DepthStencilView);

			return res;
		}

		public static RenderTarget CreateRenderTarget(DrawSystem.D3DData d3d, string name, int width, int height)
		{
			var res = new RenderTarget(name);

			var backBuffer = new Texture2D(d3d.device, new Texture2DDescription()
			{
				Format = Format.R8G8B8A8_UNorm,
				ArraySize = 1,
				MipLevels = 1,
				Width = width,
				Height = height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			var depthBuffer = new Texture2D(d3d.device, new Texture2DDescription()
			{
				Format = Format.D32_Float_S8X24_UInt,
				ArraySize = 1,
				MipLevels = 1,
				Width = width,
				Height = height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			var dsvDesc = new DepthStencilViewDescription
			{
				Dimension = DepthStencilViewDimension.Texture2D,
				Flags = DepthStencilViewFlags.None,
				Format = depthBuffer.Description.Format,
			};

			res.ShaderResourceView = new ShaderResourceView(d3d.device, backBuffer);
			res.TargetTexture = backBuffer;
			res.TargetView = new RenderTargetView(d3d.device, backBuffer);
			res.DepthStencilView = new DepthStencilView(d3d.device, depthBuffer, dsvDesc);
			res._AddDisposable(res.ShaderResourceView);
			res._AddDisposable(res.TargetView);
			res._AddDisposable(res.DepthStencilView);

			return res;
		}

	}
}
