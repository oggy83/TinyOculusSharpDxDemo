using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyOculusSharpDxDemo
{
	public static class LibOVR
	{
		[DllImport("libovr.dll")]
		public extern static bool ovr_Initialize();

		[DllImport("libovr.dll")]
		public extern static void ovr_Shutdown();

		[DllImport("libovr.dll")]
		public extern static IntPtr ovrHmd_Create(int index);

		[DllImport("libovr.dll")]
		public extern static void ovrHmd_Destroy(IntPtr hmd);

	}
}
