using UnityEngine;

namespace CarLogic
{
	public class Wheel
	{
		public WheelCollider collider;

		public Transform tireGraphic;

		public float startY;

		public bool driveWheel;

		public bool steerWheel;

		public int lastSkidmark = -1;

		public int lastVerticalmark = -1;

		public Vector3 lastEmitPosition = Vector3.zero;

		public float lastEmitTime = Time.time;

		public Vector3 wheelVelo = Vector3.zero;

		public Vector3 groundSpeed = Vector3.zero;

		public float WheelLenSqr;
	}
}
