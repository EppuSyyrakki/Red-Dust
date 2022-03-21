using System;
using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Utility timer. Update() needs to be called to advance. 
	/// </summary>
	public class Timer
		{
		private readonly bool _resetOnAlarm;
		private float _alarmTime;
		private bool _alarmRaised;
		
		/// <summary>
		/// Event that will be called when the timer reaches the alarm time.
		/// </summary>
		public event Action Alarm;

		public float TimeElapsed { get; set; }

		public float AlarmTime => _alarmTime;

		/// <summary>
		/// Creates a timer. By default resets when Alarm raised.
		/// </summary>
		/// <param name="alarmTime">The time when the Alarm is raised.</param>
		/// <param name="resetOnAlarm">True resets when Alarm.</param>
		/// <param name="initialTime">Starting time in the timer, default 0.</param>
		public Timer(float alarmTime, bool resetOnAlarm = true, float initialTime = 0)
		{
			if (alarmTime < 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			_alarmTime = alarmTime;
			_resetOnAlarm = resetOnAlarm;
			TimeElapsed = initialTime;
		}

		/// <summary>
		/// Does not run automatically each frame. Must be called from a MonoBehavior Update().
		/// </summary>
		public void Tick()
		{
			TimeElapsed += Time.deltaTime;

			if (TimeElapsed >= _alarmTime && !_alarmRaised)
			{
				RaiseAlarm();
			}
		}

		private void RaiseAlarm()
		{
			if (Alarm != null)
			{
				Alarm();
				_alarmRaised = true;

				if (_resetOnAlarm)
				{
					Reset();
				}
			}
		}
		
		/// <summary>
		/// Resets the timer. 
		/// </summary>
		public void Reset()
		{
			TimeElapsed = 0;
			_alarmRaised = false;
		}

		/// <summary>
		/// Sets the time when the alarm is raised.
		/// </summary>
		/// <param name="time"></param>
		public void SetAlarm(float time)
		{
			_alarmTime = time;
		}
	}
}