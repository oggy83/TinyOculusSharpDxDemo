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
			
			var backBuffer = Texture2D.FromSwapChain<Texture2D>(d3d.SwapChain, 0);
			var depthBuffer = new Texture2D(d3d.Device, new Texture2DDescription()
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

			res.TargetView = new RenderTargetView(d3d.Device, backBuffer);
			res.DepthStencilView = new DepthStencilView(d3d.Device, depthBuffer);
			res.ShaderResourceView = null;
			res.TargetTexture = backBuffer;
			res._AddDisposable(res.TargetView);
			res._AddDisposable(res.DepthStencilView);

			return res;
		}

		public static RenderTarget CreateRenderTarget(DrawSystem.D3DData d3d, string name, int width, int height)
		{
			var backBuffer = new Texture2D(d3d.Device, new Texture2DDescription()
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

            var res = FromTexture(d3d, name, backBuffer);
			res._AddDisposable(res.TargetView);

			return res;
		}

        public static RenderTarget FromTexture(DrawSystem.D3DData d3d, string name, Texture2D texture)
        {
            int width = texture.Description.Width;
            int height = texture.Description.Height;

            var res = new RenderTarget(name);

            var depthBuffer = new Texture2D(d3d.Device, new Texture2DDescription()
            {
                Format = Format.D32_Float,
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
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };

            res.ShaderResourceView = new ShaderResourceView(d3d.Device, texture);
            res.TargetTexture = texture;
            res.TargetView = new RenderTargetView(d3d.Device, texture);
            res.DepthStencilView = new DepthStencilView(d3d.Device, depthBuffer, dsvDesc);
            res._AddDisposable(res.ShaderResourceView);
            res._AddDisposable(res.DepthStencilView);

            return res;
        }

        public static RenderTarget[] FromSwapTextureSet(DrawSystem.D3DData d3d, String name, HmdDevice.HmdSwapTextureSet swapTextureSet)
        {
            var resultList = new List<RenderTarget>();

            int texCount = swapTextureSet.TexturePtrs.Count();
            for (int texIndex = 0; texIndex < texCount; ++texIndex)
            {
                IntPtr texPtr = swapTextureSet.TexturePtrs[texIndex];
                var texture = new Texture2D(texPtr);

                var res = FromTexture(d3d, name + texIndex, texture);
                resultList.Add(res);
            }

            return resultList.ToArray();
        }
	}
}
