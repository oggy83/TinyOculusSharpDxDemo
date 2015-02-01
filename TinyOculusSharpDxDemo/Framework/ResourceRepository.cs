using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyOculusSharpDxDemo
{
	public class ResourceRepository : IDisposable
	{
		public ResourceRepository()
		{
			m_resList = new LinkedList<ResourceBase>();
		}

		public void Dispose()
		{
			foreach (var res in m_resList)
			{
				res.Dispose();
			}
			m_resList.Clear();
		}

		/// <summary>
		/// get whether repository contains a resource whose given uid 
		/// </summary>
		/// <param name="uid"></param>
		/// <returns></returns>
		public bool Contains(string uid)
		{
			return m_resList.Where(res => res.Uid == uid).Any();
		}

		protected void _AddResource(ResourceBase res)
		{
			m_resList.AddLast(res);
		}


		#region private members

		LinkedList<ResourceBase> m_resList;

		#endregion // private members

	}
}
