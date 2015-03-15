using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOculusSharpDxDemo
{
	public interface IDrawPassCtrl
	{
		/// <summary>
		/// setup render path
		/// </summary>
		/// <param name="renderTarget">render target</param>
		void StartPass(RenderTarget renderTarget);

	}
}
