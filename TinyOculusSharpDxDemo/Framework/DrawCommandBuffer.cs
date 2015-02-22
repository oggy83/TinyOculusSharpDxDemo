using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOculusSharpDxDemo
{
	public class DrawCommandBuffer
	{
		public IEnumerable<DrawCommand> Commands
		{
			get
			{
				return m_list;
			}
		}

		public DrawCommandBuffer(int capacity)
		{
			m_list = new List<DrawCommand>(capacity);
		}

		public void AddCommand(DrawCommand command)
		{
			m_list.Add(command);
		}

		public void Clear()
		{
			m_list.Clear();
		}

		public void Sort()
		{
			m_list.Sort((a, b) =>
				{
					// 1st. sort by DrawCommandTypes
					return a.m_type - b.m_type;
				});
		}

		#region private members

		private List<DrawCommand> m_list = null;

		#endregion // private members
	}
}
