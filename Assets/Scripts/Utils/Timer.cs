using System;
using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Utility timer. Update() needs to be called to advance. 
	/// </summary>
	public class Timer
		{
		private readonly bool resetOnAlarm;
		private float alarmTime;
		private bool alarmRaised;
		
		/// <summary>
		/// Event that will be called when the timer reaches the alarm time.
		/// </summary>
		public event Action Alarm;

		public float TimeElapsed { get; set; }

		public float AlarmTime => alarmTime;

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

			this.alarmTime = alarmTime;
			this.resetOnAlarm = resetOnAlarm;
			TimeElapsed = initialTime;
		}

		/// <summary>
		/// Does not run automatically each frame. Must be called from a MonoBehavior Update().
		/// </summary>
		public void Tick()
		{
			TimeElapsed += Time.deltaTime;

			if (TimeElapsed >= alarmTime && !alarmRaised)
			{
				RaiseAlarm();
			}
		}
		
		/// <summary>
		/// Resets the timer. 
		/// </summary>
		public void Reset()
		{
			TimeElapsed = 0;
			alarmRaised = false;
		}

		/// <summary>
		/// Sets the time when the alarm is raised.
		/// </summary>
		/// <param name="time"></param>
		public void SetAlarm(float time)
		{
			alarmTime = time;
		}

		private void RaiseAlarm()
		{
			if (Alarm != null)
			{
				Alarm();
				alarmRaised = true;

				if (resetOnAlarm)
				{
					Reset();
				}
			}
		}
	}

	/// <summary>
	/// Generic version. The Alarm action is of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Timer<T>
	{
		private readonly bool resetOnAlarm;
		private float alarmTime;
		private bool alarmRaised;
		private T value;

		/// <summary>
		/// Event that will be called when the timer reaches the alarm time.
		/// </summary>
		public event Action<T> Alarm;

		public float TimeElapsed { get; set; }

		public float AlarmTime => alarmTime;

		/// <summary>
		/// Creates a timer. By default resets when Alarm raised.
		/// </summary>
		/// <param name="alarmTime">The time when the Alarm is raised.</param>
		/// <param name="resetOnAlarm">True resets when Alarm.</param>
		/// <param name="initialTime">Starting time in the timer, default 0.</param>
		public Timer(T value, float alarmTime, bool resetOnAlarm = true, float initialTime = 0)
		{
			if (alarmTime < 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			this.alarmTime = alarmTime;
			this.resetOnAlarm = resetOnAlarm;
			this.value = value;
			TimeElapsed = initialTime;
		}

		/// <summary>
		/// Does not run automatically each frame. Must be called from a MonoBehavior Update().
		/// </summary>
		public void Tick()
		{
			TimeElapsed += Time.deltaTime;

			if (TimeElapsed >= alarmTime && !alarmRaised)
			{
				RaiseAlarm();
			}
		}		

		/// <summary>
		/// Resets the timer. 
		/// </summary>
		public void Reset()
		{
			TimeElapsed = 0;
			alarmRaised = false;
		}

		/// <summary>
		/// Sets the time when the alarm is raised.
		/// </summary>
		/// <param name="time"></param>
		public void SetAlarm(float time)
		{
			alarmTime = time;
		}

		private void RaiseAlarm()
		{
			if (Alarm != null)
			{
				Alarm(value);
				alarmRaised = true;

				if (resetOnAlarm)
				{
					Reset();
				}
			}
		}
	}
}