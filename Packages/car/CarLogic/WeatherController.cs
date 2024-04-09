using System;
using UnityEngine;

namespace CarLogic
{
	internal class WeatherController : ControllerBase
	{
		private int noWeatherLayer;

		private int flag;

		public void Init(CarView car)
		{
			car.OnTriggerBegin = (Action<Collider>)Delegate.Combine(car.OnTriggerBegin, new Action<Collider>(onCollisionEnter));
			car.OnTriggerEnd = (Action<Collider>)Delegate.Combine(car.OnTriggerEnd, new Action<Collider>(onCollisionExit));
			noWeatherLayer = LayerMask.NameToLayer("NoWeather");
		}

		private void onCollisionEnter(Collider c)
		{
			if (c.gameObject.layer == noWeatherLayer)
			{
				flag++;
				if (RacePathManager.ActiveInstance != null)
				{
					RacePathManager.ActiveInstance.PauseWeather();
				}
			}
		}

		private void onCollisionExit(Collider c)
		{
			if (c.gameObject.layer == noWeatherLayer)
			{
				flag--;
				if (flag <= 0 && RacePathManager.ActiveInstance != null)
				{
					RacePathManager.ActiveInstance.ShowWeather();
				}
			}
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
		}
	}
}
