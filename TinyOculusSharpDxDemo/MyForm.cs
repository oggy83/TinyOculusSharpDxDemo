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

		private void _OnLoad(object sender, EventArgs e)
		{
			if (!LibOVR.ovr_Initialize())
			{
				MessageBox.Show("Failed to initialize LibOVR.");
				Close();
				return;
			}

			m_hmd = LibOVR.ovrHmd_Create(0);
			if (m_hmd == IntPtr.Zero)
			{
				MessageBox.Show("Oculus Rift not detected.");
				Close();
				return;
			}
		}

		private void _OnFormClosed(object sender, FormClosedEventArgs e)
		{
			LibOVR.ovrHmd_Destroy(m_hmd);
			LibOVR.ovr_Shutdown();
		}

		private IntPtr m_hmd = IntPtr.Zero;
	}
}
