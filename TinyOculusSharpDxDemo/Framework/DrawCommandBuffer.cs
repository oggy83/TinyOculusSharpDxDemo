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

		public void AddCommand(DrawCommand[] commands)
		{
			m_list.AddRange(commands);
		}

		public void Clear()
		{
			m_list.Clear();
		}

		#region private members

		private List<DrawCommand> m_list = null;

		#endregion // private members
	}
}
