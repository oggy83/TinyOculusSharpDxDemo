using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
	public partial class DrawContext
	{
		#region public types

		#endregion // public types

		private static BlendState _CreateBlendState(DrawSystem.D3DData d3d, DrawSystem.RenderMode mode)
		{
			switch (mode)
			{
				case DrawSystem.RenderMode.Opaque:
					{
						var blendDesc = BlendStateDescription.Default();
						return new BlendState(d3d.Device, blendDesc);
					}

				case DrawSystem.RenderMode.Transparency:
					{
						var blendDesc = new BlendStateDescription()
						{
							AlphaToCoverageEnable = false,
							IndependentBlendEnable = false,
						};
						blendDesc.RenderTarget[0] = new RenderTargetBlendDescription(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha, BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, ColorWriteMaskFlags.All);
						return new BlendState(d3d.Device, blendDesc);
					}
				default :
					return null;
			}
		
		}

	}
}
