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
			if (!LibOVR.ovr_Initialize())
			{
				MessageBox.Show("Failed to initialize LibOVR.");
				return;
			}

			string version = CRef<string>.FromCharPtr(LibOVR.ovr_GetVersionString()).Value;
			int detect = LibOVR.ovrHmd_Detect();

			var hmd = CRef<LibOVR.ovrHmdDesc>.FromPtr(LibOVR.ovrHmd_Create(0));
			if (hmd == null)
			{
				MessageBox.Show("Oculus Rift not detected.");
				return;
			}

			bool isWindow = (hmd.Value.HmdCaps & (int)LibOVR.ovrHmdCaps.ExtendDesktop) == 1 ? false : true;
			var rect = new LibOVR.ovrRecti
			{
				Pos = hmd.Value.WindowPos,
				Size = hmd.Value.Resolution,
			};

			var form = new MyForm();
			form.ClientSize = new Size(rect.Size.w, rect.Size.h);

			// Create Device & SwapChain
			var desc = new SwapChainDescription()
			{
				BufferCount = 2,
				ModeDescription =
					new ModeDescription(rect.Size.w, rect.Size.h, new Rational(0, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = isWindow,
				OutputHandle = form.GetRenderTarget().Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Sequential,
				Usage = Usage.RenderTargetOutput,
				Flags = SwapChainFlags.AllowModeSwitch,
			};

			Device device;
			SwapChain swapChain;
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, desc, out device, out swapChain);

			var scene = new Scene(device, swapChain, form.GetRenderTarget(), hmd);
			RenderLoop.Run(form, () => { scene.RenderFrame(); });

			// Release
			scene.Dispose();
			device.Dispose();
			swapChain.Dispose();

			LibOVR.ovrHmd_Destroy(hmd.Ptr);
			LibOVR.ovr_Shutdown();
		}

	}
}
