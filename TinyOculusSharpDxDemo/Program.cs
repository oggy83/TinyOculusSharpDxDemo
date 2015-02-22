using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace TinyOculusSharpDxDemo
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			bool bStereoRendering = false;// change to 'false' due to non-stereo rendering for debug

			// init oculus rift hmd system
			HmdSystem.Initialize();
			var hmdSys = HmdSystem.GetInstance();
			var hmd = hmdSys.DetectHmd();

			Size resolution = hmd.Resolution;
			if (!bStereoRendering)
			{
				//resolution.Width = 1920;// Full HD
				//resolution.Height = 1080;
				resolution.Width = 1280;
				resolution.Height = 720;
			}

			var form = new MyForm();
			form.ClientSize = resolution;

			// Create Device & SwapChain
			var desc = new SwapChainDescription()
			{
				BufferCount = 2,
				ModeDescription =
					new ModeDescription(resolution.Width, resolution.Height, new Rational(0, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = hmd.IsEnableWindowMode,
				OutputHandle = form.GetRenderTarget().Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Sequential,
				Usage = Usage.RenderTargetOutput,
				Flags = SwapChainFlags.AllowModeSwitch,
			};

			Device device;
			SwapChain swapChain;
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, desc, out device, out swapChain);

			var scene = new Scene(device, swapChain, form.GetRenderTarget(), hmd, bStereoRendering);
			RenderLoop.Run(form, () => { scene.RenderFrame(); });

			// Release
			scene.Dispose();
			device.Dispose();
			swapChain.Dispose();
			HmdSystem.Dispose();
		}

	}
}
