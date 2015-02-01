using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TinyOculusSharpDxDemo
{
	public class FpsCounter
	{
		private const int MaxDeltaTimeQueueCount = 100;

		public FpsCounter()
		{
			m_sw = new Stopwatch();
			m_dT = 0;
			m_dtQueue = new Queue<long>();
			m_sw.Start();
		}

		public void BeginFrame()
		{
			m_sw.Restart();
		}

		public void EndFrame()
		{
			if (m_sw.IsRunning)
			{
				m_sw.Stop();
				// update delta time
				m_dT = m_sw.ElapsedMilliseconds;
				if (m_dtQueue.Count == MaxDeltaTimeQueueCount)
				{
					m_dtQueue.Dequeue();
				}
				m_dtQueue.Enqueue(m_dT);
			}
		}

		/// <summary>
		/// get delta time in milisec
		/// </summary>
		/// <returns></returns>
		public int GetDeltaTime()
		{
			return (int)m_dT;
		}

		/// <summary>
		/// get average delta time in milisec
		/// </summary>
		/// <returns></returns>
		public int GetAverageDeltaTime()
		{
			return m_dtQueue.Count == 0? 0 : (int)m_dtQueue.Average();
		}

		#region private members

		private Stopwatch m_sw = null;

		/// <summary>
		/// next delta time [mili sec]
		/// </summary>
		private long m_dT;

		/// <summary>
		/// delta time queue
		/// </summary>
		private Queue<long> m_dtQueue;

		#endregion // private members

	
	}
}
