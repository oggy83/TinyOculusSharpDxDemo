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
		const int CLIENT_WIDTH = 800;
		const int CLIENT_HEIGHT = 600;
		const int FRAME_PER_SEC = 60;

		[STAThread]
		static void Main()
		{
			var form = new MyForm();
			form.ClientSize = new Size(CLIENT_WIDTH, CLIENT_HEIGHT + 24/*@todo  menu bar height*/);

			// Create Device & SwapChain
			var desc = new SwapChainDescription()
			{
				BufferCount = 1,
				ModeDescription =
					new ModeDescription(CLIENT_WIDTH, CLIENT_HEIGHT, new Rational(FRAME_PER_SEC, 1), Format.B8G8R8A8_UNorm),
				IsWindowed = true,
				OutputHandle = form.GetRenderTarget().Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			Device device;
			SwapChain swapChain;
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, desc, out device, out swapChain);

			var scene = new Scene(device, swapChain, form.GetRenderTarget());
			RenderLoop.Run(form, () => { scene.RenderFrame(); });

			// Release
			scene.Dispose();
			device.Dispose();
			swapChain.Dispose();
		}

	}
}
