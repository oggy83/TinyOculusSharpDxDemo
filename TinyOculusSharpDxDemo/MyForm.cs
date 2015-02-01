using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TinyOculusSharpDxDemo
{
	public partial class MyForm : Form
	{
		public MyForm()
		{
			InitializeComponent();
		}

		public Panel GetRenderTarget()
		{
			return m_renderPanel;
		}

		private void _OnLoad(object sender, EventArgs e)
		{
			if (!LibOVR.ovr_Initialize())
			{
				MessageBox.Show("Failed to initialize LibOVR.");
				Close();
				return;
			}

			string version = CRef<string>.FromCharPtr(LibOVR.ovr_GetVersionString()).Value;
			int detect = LibOVR.ovrHmd_Detect();

			m_hmd = CRef<LibOVR.ovrHmdDesc>.FromPtr(LibOVR.ovrHmd_Create(0));
			if (m_hmd == null)
			{
				MessageBox.Show("Oculus Rift not detected.");
				Close();
				return;
			}
		}

		private void _OnFormClosed(object sender, FormClosedEventArgs e)
		{
			LibOVR.ovrHmd_Destroy(m_hmd.Ptr);
			LibOVR.ovr_Shutdown();
		}

		private CRef<LibOVR.ovrHmdDesc> m_hmd = null;
	}
}
