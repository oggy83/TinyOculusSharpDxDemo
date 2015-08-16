using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// C-lang reference
	/// </summary>
	public class CRef<ValueType>
	{
		private IntPtr m_ptr;
		public IntPtr Ptr
		{
			get
			{
				return m_ptr;
			}
		}

		public ValueType Value;

        public void Clear()
        {
            m_ptr = IntPtr.Zero;
        }

		public static CRef<ValueType> FromPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return null;
			}
			else
			{
				var handle = new CRef<ValueType>();
				handle.m_ptr = ptr;
				handle.Value = (ValueType)Marshal.PtrToStructure(ptr, typeof(ValueType));
				return handle;
			}
		}

		public static CRef<string> FromCharPtr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
			{
				return null;
			}
			else
			{
				var handle = new CRef<string>();
				handle.m_ptr = ptr;
				handle.Value = Marshal.PtrToStringAnsi(ptr);
				return handle;
			}
		}

	}
}
