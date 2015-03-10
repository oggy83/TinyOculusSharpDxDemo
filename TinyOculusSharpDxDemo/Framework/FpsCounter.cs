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
			m_deltaTick = 0;
			m_deltaTickQueue = new Queue<long>();
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
				m_deltaTick = m_sw.ElapsedTicks;
				if (m_deltaTickQueue.Count == MaxDeltaTimeQueueCount)
				{
					m_deltaTickQueue.Dequeue();
				}
				m_deltaTickQueue.Enqueue(m_deltaTick);
			}
		}

		/// <summary>
		/// get delta time in sec
		/// </summary>
		/// <returns></returns>
		public double GetDeltaTime()
		{
			return (double)m_deltaTick / Stopwatch.Frequency;
		}

		/// <summary>
		/// get average delta time in sec
		/// </summary>
		/// <returns></returns>
		public double GetAverageDeltaTime()
		{
			double avgTickCount = (m_deltaTickQueue.Count == 0) ? 0 : m_deltaTickQueue.Average();
			return avgTickCount / Stopwatch.Frequency;
		}

		#region private members

		private Stopwatch m_sw = null;

		/// <summary>
		/// next delta tick count
		/// </summary>
		private long m_deltaTick;

		/// <summary>
		/// delta tick count queue
		/// </summary>
		private Queue<long> m_deltaTickQueue;

		#endregion // private members

	
	}
}
