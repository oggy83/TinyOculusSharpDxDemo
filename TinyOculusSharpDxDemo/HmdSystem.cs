using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace TinyOculusSharpDxDemo
{
	public class HmdSystem
	{
		#region static

		private static HmdSystem s_singleton = null;

		static public void Initialize()
		{
			s_singleton = new HmdSystem();
		}

		static public void Dispose()
		{
			if (s_singleton.m_hmdHandle != null)
			{
				LibOVR.ovrHmd_Destroy(s_singleton.m_hmdHandle.Ptr);
			}
			LibOVR.ovr_Shutdown();
		}

		static public HmdSystem GetInstance()
		{
			return s_singleton;
		}

		#endregion // static

		private HmdSystem()
		{
			if (!LibOVR.ovr_Initialize())
			{
				MessageBox.Show("Failed to initialize LibOVR.");
				return;
			}

			string version = CRef<string>.FromCharPtr(LibOVR.ovr_GetVersionString()).Value;
			int detect = LibOVR.ovrHmd_Detect();

			Debug.Print("[HMD] sdk version = {0}", version);
			Debug.Print("[HMD] detected device count = {0}", detect);
		}

		/// <summary>
		/// get the first detected hmd device
		/// </summary>
		/// <returns>
		/// detected hmd
		/// </returns>
		public HmdDevice DetectHmd()
		{
			var hmd = CRef<LibOVR.ovrHmdDesc>.FromPtr(LibOVR.ovrHmd_Create(0));
			if (hmd == null)
			{
				MessageBox.Show("Oculus Rift not detected.");
				return null;
			}
			else
			{
				m_hmdHandle = hmd;
				return new HmdDevice(hmd);
			}
		}

		#region private members

		private CRef<LibOVR.ovrHmdDesc> m_hmdHandle = null;

		#endregion // private members
	}
}
