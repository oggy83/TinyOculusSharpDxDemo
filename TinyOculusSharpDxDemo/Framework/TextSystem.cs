using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Windows;

namespace TinyOculusSharpDxDemo
{
	public class TextSystem
	{
		#region static

		private static TextSystem s_singleton = null;

		static public void Initialize(Panel renderTargetPanel)
		{
			s_singleton = new TextSystem(renderTargetPanel);
		}

		static public void Dispose()
		{
			s_singleton._Dispose();
			s_singleton = null;
		}

		static public TextSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		public void DrawText(string text)
		{
			var rect = m_renderTargetPanel.ClientRectangle;

			var layout = new SharpDX.RectangleF
			(
				0,
				0,
				(float)rect.Width / m_dpiScaleX,
				(float)rect.Height / m_dpiScaleY
			);

			m_renderTarget.BeginDraw();
			m_renderTarget.DrawText(text, m_textFormat, layout, m_brush);
			m_renderTarget.EndDraw();
		}


		#region private members

		private Panel m_renderTargetPanel;
		private SharpDX.Direct2D1.Factory m_d2dFactory;
		private SharpDX.DirectWrite.Factory m_dWriteFactory;
		private SharpDX.Direct2D1.RenderTarget m_renderTarget;
		private float m_dpiScaleX;
		private float m_dpiScaleY;

		private TextFormat m_textFormat;
		private SolidColorBrush m_brush;

		#endregion // private members

		#region private methods

		private TextSystem(Panel renderTargetPanel)
		{
			m_renderTargetPanel = renderTargetPanel;

			using (var g = Graphics.FromHwnd(IntPtr.Zero))
			{
				m_dpiScaleX = g.DpiX / 96.0f;
				m_dpiScaleY = g.DpiY / 96.0f;
			}

			m_d2dFactory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.MultiThreaded);
			m_dWriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

			m_textFormat = new TextFormat(m_dWriteFactory, "Meiryo", 24);

			var prop = new RenderTargetProperties()
			{
				DpiX = 0,
				DpiY = 0,
				MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
				PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),	// use a swap chain format
				Type = RenderTargetType.Hardware,
				Usage = RenderTargetUsage.None
			};

			var giSurface = DrawSystem.GetInstance().D3D.swapChain.GetBackBuffer<Surface>(0);
			var hWndProp = new HwndRenderTargetProperties()
			{
				Hwnd = m_renderTargetPanel.Handle,
				PixelSize = new Size2(m_renderTargetPanel.Width, m_renderTargetPanel.Height),
				PresentOptions = PresentOptions.None
			};

			m_renderTarget = new SharpDX.Direct2D1.RenderTarget(m_d2dFactory, giSurface, prop);
			m_brush = new SolidColorBrush(m_renderTarget, Color4.White);
		}

		private void _Dispose()
		{
			m_brush.Dispose();
			m_renderTarget.Dispose();
			m_textFormat.Dispose();
			m_dWriteFactory.Dispose();
			m_d2dFactory.Dispose();
		}

		#endregion // private methods
	}
}
