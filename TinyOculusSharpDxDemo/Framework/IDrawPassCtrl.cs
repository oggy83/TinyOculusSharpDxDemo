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

		/// <summary>
		/// set render state for the given command
		/// </summary>
		/// <param name="command">draw command</param>
		/// <param name="world">world data</param>
		void ExecuteCommand(DrawCommand command, DrawSystem.WorldData world);
	}
}
