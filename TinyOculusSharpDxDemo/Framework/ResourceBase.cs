using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// Base class of resource
	/// </summary>
	public class ResourceBase : IDisposable
	{
		private String m_uid = "";
		public String Uid
		{
			get
			{
				return m_uid;
			}
		}

		public ResourceBase(String uid)
		{
			m_uid = uid;
			m_disposable = new LinkedList<IDisposable>();
		}

		public void Dispose()
		{
			foreach (var obj in m_disposable)
			{
				obj.Dispose();
			}
			m_isDisposed = true;
		}

		public bool IsDisposed()
		{
			return m_isDisposed;
		}

		/// <summary>
		/// Reserve an object which will be disposed on Dispose().
		/// </summary>
		/// <param name="obj">dispose object</param>
		protected void _AddDisposable(IDisposable obj)
		{
			m_disposable.AddLast(obj);
		}

		private LinkedList<IDisposable> m_disposable;
		private bool m_isDisposed = false;
	}
}
