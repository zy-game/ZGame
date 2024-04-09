using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class SyncMoveSenderForecast
	{
		public float AsyncInterval = 0.033f;

		public static float SpeedFactor = 1.05f;

		public static float AngleSpeedFactor = 10f;

		public static float SyncKeyAngle = 0.85f;

		public static float SyncKeySpeed = 30f;

		public static bool UseSyncKeySpeed = false;

		public static float SyncKeySpeedInterval = 0.033f;

		private float _lastLinearVelocity;

		public float _lastKeySpeedSend;

		public Action<byte[]> OnSelfCarSendAction;

		private CarState carState;

		private SpecialType SpecialType;

		private int lastGHit;

		private float oldSpeed;

		private float oldThrottle;

		private float lastSendTime;

		internal long NetDelay;

		private float timeLeft;

		private ushort myIndex;

		private SpecialType lastStype;

		public Dictionary<uint, long> netTimer = new Dictionary<uint, long>();

		private MovementDataForecast curTarget;

		private Vector3 lastPosition;

		private float lastSteer;

		private Vector3 _lastSelfRelativeVelocity = Vector3.zero;

		public SyncMoveSenderForecast(CarState carState)
		{
			this.carState = carState;
		}

		public void OnUpdateSelf()
		{
			if (lastSendTime <= 0f)
			{
				lastSendTime = Time.time;
			}
			bool flag = false;
			if (curTarget != null)
			{
				float z = carState.relativeVelocity.z;
				if (Mathf.Sign(z) != Mathf.Sign(oldSpeed) && Mathf.Abs(z + oldSpeed) > 3f)
				{
					flag = true;
				}
			}
			if (UseSyncKeySpeed && Time.time - _lastKeySpeedSend >= SyncKeySpeedInterval && carState.LinearVelocity >= SyncKeySpeed && Vector3.Angle(_lastSelfRelativeVelocity, carState.relativeVelocity) >= SyncKeyAngle)
			{
				flag = true;
				_lastKeySpeedSend = Time.time;
			}
			_lastSelfRelativeVelocity = carState.relativeVelocity;
			if ((lastGHit == 0 && carState.GroundHit != 0) || (lastGHit != 0 && carState.GroundHit == 0))
			{
				flag = true;
			}
			float y = carState.rigidbody.angularVelocity.y;
			if ((AsyncInterval > 0.1f && Mathf.Abs(y) > 1f) || lastStype == SpecialType.Translate)
			{
				int num = 1;
				if (AsyncInterval > 0.4f)
				{
					num = 3;
				}
				else if (AsyncInterval > 0.3f)
				{
					num = 2;
				}
				timeLeft -= Time.fixedDeltaTime * (float)num;
			}
			if (timeLeft <= 0f || oldThrottle != carState.Throttle)
			{
				oldThrottle = carState.Throttle;
				flag = true;
			}
			if (lastStype != carState.ApplyingSpecialType)
			{
				lastStype = carState.ApplyingSpecialType;
				flag = true;
			}
			if (lastSteer != carState.Steer)
			{
				lastSteer = carState.Steer;
				flag = true;
			}
			if (flag)
			{
				SendTransform();
				if (timeLeft > 0f)
				{
					timeLeft = 0f;
				}
				timeLeft += AsyncInterval;
				lastGHit = carState.GroundHit;
				oldSpeed = carState.relativeVelocity.z;
			}
			else
			{
				timeLeft -= Time.fixedDeltaTime;
			}
		}

		public void SendTransform()
		{
			float num = Time.time - lastSendTime;
			MovementDataForecast movementDataForecast = new MovementDataForecast(carState, myIndex++, num, NetDelay);
			if (carState.ApplyingSpecialType == SpecialType.Translate)
			{
				Vector3 vector = (movementDataForecast.Position - lastPosition) / num;
				movementDataForecast.Velocity = carState.transform.forward * vector.magnitude;
			}
			lastPosition = movementDataForecast.Position;
			lastSendTime = Time.time;
			curTarget = movementDataForecast;
			netTimer[movementDataForecast.Index] = DateTime.Now.Ticks;
			if (OnSelfCarSendAction != null)
			{
				OnSelfCarSendAction(movementDataForecast.ToArray());
			}
			else
			{
				RaceCallback.SendRemote(movementDataForecast.ToArray());
			}
		}

		public void UpdateNetDelay(MovementDataForecast data)
		{
			if (netTimer.ContainsKey(data.Index))
			{
				long num = netTimer[data.Index];
				netTimer.Remove(data.Index);
				NetDelay = (DateTime.Now.Ticks - num) / 10000;
				FPSShower.Messages["NetDelay"] = NetDelay + "ms";
			}
		}

		public void ResetTimeLeft()
		{
			timeLeft = 0f;
		}
	}
}
