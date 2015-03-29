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
		private const double AverageDeltaTimeUpdateInterval = 1.0;

		public FpsCounter()
		{
			m_sw = new Stopwatch();
			m_lastDeltaTick = 0;
			m_sumDeltaTick = 0;
			m_deltaTickCount = 0;
			m_averageDeltaTimeUpdateTimer = 0;
			m_lastAverageDeltaTime = 0;
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
				m_lastDeltaTick = m_sw.ElapsedTicks;

				m_averageDeltaTimeUpdateTimer -= GetDeltaTime();
				m_sumDeltaTick += m_lastDeltaTick;
				m_deltaTickCount++;

				if (m_averageDeltaTimeUpdateTimer < 0.0f)
				{
					// update average delta time
					double avgTickCount = (double)m_sumDeltaTick / m_deltaTickCount;
					m_lastAverageDeltaTime = avgTickCount / Stopwatch.Frequency;

					m_sumDeltaTick = 0;
					m_deltaTickCount = 0;
					m_averageDeltaTimeUpdateTimer = AverageDeltaTimeUpdateInterval;
				}
			}
		}

		/// <summary>
		/// get delta time in sec
		/// </summary>
		/// <returns></returns>
		public double GetDeltaTime()
		{
			return (double)m_lastDeltaTick / Stopwatch.Frequency;
		}

		/// <summary>
		/// get average delta time in sec
		/// </summary>
		/// <returns></returns>
		public double GetAverageDeltaTime()
		{
			return m_lastAverageDeltaTime;
		}

		#region private members

		private Stopwatch m_sw = null;

		/// <summary>
		/// next delta tick count
		/// </summary>
		private long m_lastDeltaTick;

		/// <summary>
		/// the sum of delta time since last update of average delta time
		/// </summary>
		private long m_sumDeltaTick;

		/// <summary>
		/// counter for m_sumDeltaTime
		/// </summary>
		private int m_deltaTickCount;

		/// <summary>
		/// timer for update average delta time
		/// </summary>
		private double m_averageDeltaTimeUpdateTimer = 0;

		/// <summary>
		/// value for GetAverageDeltaTime()
		/// </summary>
		private double m_lastAverageDeltaTime = 0;

		#endregion // private members

	
	}
}
