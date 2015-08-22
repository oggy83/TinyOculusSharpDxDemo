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
			if (s_singleton.m_hmdDevice != null)
			{
                s_singleton.m_hmdDevice.Dispose();
                s_singleton.m_hmdDevice = null;
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
            //var initParams = new LibOVR.ovrInitParams;
            //initParams.Flags = 0;

			if (LibOVR.ovr_Initialize(IntPtr.Zero) != 0)
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
            IntPtr hmdPtr;
            if (LibOVR.ovrHmd_Create(0, out hmdPtr) != 0)
            {
                MessageBox.Show("Oculus Rift not detected.");
                return null;
            }

			var hmd = CRef<LibOVR.ovrHmdDesc>.FromPtr(hmdPtr);
			if (hmd == null)
			{
				return null;
			}
			else
			{
				m_hmdDevice = new HmdDevice(hmd);
                return m_hmdDevice;
			}
		}

        public HmdDevice GetDevice()
        {
            Debug.Assert(m_hmdDevice != null, "no hmd device is detected");
            return m_hmdDevice;
        }

		#region private members

        private HmdDevice m_hmdDevice = null;

		#endregion // private members
	}
}
