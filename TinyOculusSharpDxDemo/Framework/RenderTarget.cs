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
            var depthBuffer = _CreateDepthBuffer(d3d, backBuffer.Description.Width, backBuffer.Description.Height);
            
			res.TargetView = new RenderTargetView(d3d.Device, backBuffer);
            res.DepthStencilView = _CreateDepthStencilView(d3d, depthBuffer);
			res.ShaderResourceView = null;
			res.TargetTexture = backBuffer;
			res._AddDisposable(res.TargetView);
            res._AddDisposable(depthBuffer);
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

            var depthBuffer = _CreateDepthBuffer(d3d, width, height);

            var res = new RenderTarget(name);
            res.ShaderResourceView = new ShaderResourceView(d3d.Device, backBuffer);
            res.TargetTexture = backBuffer;
            res.TargetView = new RenderTargetView(d3d.Device, backBuffer);
            res.DepthStencilView = _CreateDepthStencilView(d3d, depthBuffer);

            res._AddDisposable(backBuffer);
            res._AddDisposable(depthBuffer);
            res._AddDisposable(res.ShaderResourceView);
            res._AddDisposable(res.TargetView);
            res._AddDisposable(res.DepthStencilView);
            
			return res;
		}

        public static RenderTarget[] FromSwapTextureSet(DrawSystem.D3DData d3d, String name, HmdSwapTextureSet swapTextureSet)
        {
            int width = swapTextureSet.Resolution.Width;
            int height = swapTextureSet.Resolution.Height;

            var depthBuffer = _CreateDepthBuffer(d3d, width, height);
            var depthStencilView = _CreateDepthStencilView(d3d, depthBuffer);

            var resultList = new List<RenderTarget>();

            int texCount = swapTextureSet.Textures.Count();
            for (int texIndex = 0; texIndex < texCount; ++texIndex)
            {
                var texture = swapTextureSet.Textures[texIndex];

                var res = new RenderTarget(name + texIndex);
                res.ShaderResourceView = new ShaderResourceView(d3d.Device, texture);
                res.TargetTexture = texture;
                res.TargetView = swapTextureSet.RenderTargetView[texIndex];
                res.DepthStencilView = depthStencilView;

                res._AddDisposable(res.ShaderResourceView);
                if (texIndex == 0)
                {
                    // depth buffer and view are shared by each render targets
                    res._AddDisposable(depthBuffer);
                    res._AddDisposable(res.DepthStencilView);
                }

                resultList.Add(res);
            }

            return resultList.ToArray();
        }

        #region private methods

        public static Texture2D _CreateDepthBuffer(DrawSystem.D3DData d3d, int width, int height)
        {
            return new Texture2D(d3d.Device, new Texture2DDescription()
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
        }

        public static DepthStencilView _CreateDepthStencilView(DrawSystem.D3DData d3d, Texture2D depthBuffer)
        {
            var dsvDesc = new DepthStencilViewDescription
            {
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
                Format = depthBuffer.Description.Format,
                Texture2D = new DepthStencilViewDescription.Texture2DResource() { MipSlice = 0 }
            };

            return new DepthStencilView(d3d.Device, depthBuffer, dsvDesc);
        }

        #endregion // private methods
    }
}
