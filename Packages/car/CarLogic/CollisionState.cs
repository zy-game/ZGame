using System.Collections;
using UnityEngine;

namespace CarLogic
{
	public class CollisionState
	{
		public static Float FaceBounceAngle = 60;

		public static Float AngleSpeed = 45;

		public static Float OutBounceAngleOffset = 0;

		public static Float OutBounceSideAngleOffset = 1;

		public static Float BounceTriggerVelocity = 3;

		public static Float FaceBounceMaxVelocity = 1.5f;

		public static Float SideBounceMinVelocity = 5;

		public static Float FaceBounceVelocityFactor = 0.1f;

		public static Float SideBounceVelocityFactor = 0.15f;

		public static Float TailBounceVelocityFactor = 0.05f;

		public static Float ForwardVelocityFactor = 0.2f;

		public static Float OutSlideTime = 0.15f;

		public static AnimationCurve OutSlideForce = new AnimationCurve(new Keyframe(0f, 20f), new Keyframe(1f, 10f));

		public static AnimationCurve OutSideWayFriction = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 50f));

		public static Float PileupVelocityFactor = 0.1f;

		public static Float AiTriggerIntervalTime = 0.1f;

		public static AnimationCurve OtherPileupVelocityCurve = new AnimationCurve(new Keyframe(0f, 30f), new Keyframe(1f, 50f));

		public static Float OtherPileupVelocityFactor = 0.3f;

		public static Float AiSideRotAngleSpeed = 10;

		public static Float AiTailRotAngleSpeed = 20;

		public static Float AiFaceBounceVelocityFactor = 0.15f;

		public static Float MaxRotTime = 1f;

		public static Float CapsuleColliderHeight = 0.6f;

		public static Float CapsuleColliderRadius = 0.25f;

		private CarView carView;

		private CarState carState;

		private float rotAngle;

		public float StartRotTime;

		private IEnumerator coFrictionWait;

		private bool stopFriction;

		private float lastLostVelocityTime;

		public float RotAngle
		{
			get
			{
				return rotAngle;
			}
			set
			{
				rotAngle = Mathf.Min((float)MaxRotTime * (float)AngleSpeed, value);
				StartRotTime = Time.time;
			}
		}

		public void Init(CarView carView, CarState carState)
		{
			this.carView = carView;
			this.carState = carState;
		}

		public HitWallDirection DoCollisionDirection(Vector3 hitForces)
		{
			Vector3 right = carView.transform.right;
			Vector3 forward = carView.transform.forward;
			float num = Vector3.Dot(hitForces, right);
			if (num >= 0.25f)
			{
				carState.CollisionDirection = HitWallDirection.RIGHT;
			}
			else if (num <= -0.25f)
			{
				carState.CollisionDirection = HitWallDirection.LEFT;
			}
			else if (Vector3.Dot(hitForces, forward) >= 0f)
			{
				carState.CollisionDirection = HitWallDirection.FRONT;
			}
			else
			{
				carState.CollisionDirection = HitWallDirection.BACK;
			}
			return carState.CollisionDirection;
		}

		public bool DoFaceBounce(Vector3 v0, float factor, bool slide)
		{
			Vector3 forward = carState.transform.forward;
			Vector3 normalized = Vector3.Cross(carState.HitNormal, carState.transform.up).normalized;
			if (Application.isEditor)
			{
				Debug.DrawRay(carState.HitPoint, forward, Color.green, 5f);
			}
			if (slide)
			{
				DoSlide(OutSlideTime, normalized, carState.HitNormal);
			}
			carState.velocity = carState.rigidbody.velocity;
			return true;
		}

		public bool DoSideBounce(Vector3 v0, float factor)
		{
			Vector3 vector = Vector3.Project(v0, carState.HitNormal);
			Vector3 normalized = (v0 + vector).normalized;
			bool result = vector.magnitude >= (float)BounceTriggerVelocity;
			float num = Vector3.Angle(carView.transform.forward, carState.rigidbody.velocity);
			float magnitude = v0.magnitude;
			carState.rigidbody.velocity = normalized * magnitude;
			if (Time.time - lastLostVelocityTime > 1f)
			{
				lastLostVelocityTime = Time.time;
				carState.rigidbody.velocity *= 1f - (float)ForwardVelocityFactor;
				vector *= factor;
				float magnitude2 = vector.magnitude;
				if (Mathf.Abs(num - 90f) < 10f)
				{
					if (magnitude2 <= (float)SideBounceMinVelocity)
					{
						vector = vector.normalized * SideBounceMinVelocity;
					}
				}
				else if (magnitude2 < 1f)
				{
					vector = vector.normalized;
				}
				else if (magnitude2 > (float)FaceBounceMaxVelocity)
				{
					vector = Vector3.ClampMagnitude(vector, FaceBounceMaxVelocity);
				}
				carState.rigidbody.velocity += vector;
			}
			carState.velocity = carState.rigidbody.velocity;
			return result;
		}

		public void DoSlide(float duration, Vector3 forwardNormal, Vector3 forceNormal)
		{
			DoStopSideWayFriction();
			carView.StartCoroutine(coFrictionWait = CoFrictionAddForce(duration, forwardNormal, forceNormal));
		}

		private IEnumerator CoFrictionAddForce(float duration, Vector3 forwardNormal, Vector3 forceNormal)
		{
			stopFriction = false;
			float dotSide = Vector3.Dot(carState.transform.right, -carState.HitNormal);
			int sign = ((!(dotSide > 0f)) ? 1 : (-1));
			forwardNormal *= (float)sign;
			if (Application.isEditor)
			{
				Debug.DrawRay(carState.HitPoint, forwardNormal, Color.red, 5f);
			}
			float passTime = 0f;
			float vSize = carState.rigidbody.velocity.magnitude;
			while (passTime < duration)
			{
				passTime += Time.deltaTime;
				float forceSize = OutSlideForce.Evaluate(passTime);
				Vector3 forceVector = forceSize * forceNormal;
				if (stopFriction)
				{
					break;
				}
				carView.SetSideWayFriction(OutSideWayFriction.Evaluate(passTime));
				carView.GetComponent<Rigidbody>().AddForce(forceVector);
				DLogError("v0: {0}, {1}", carState.rigidbody.velocity, carState.rigidbody.velocity.sqrMagnitude);
				carState.rigidbody.velocity = forwardNormal.normalized * vSize;
				carState.velocity = carState.rigidbody.velocity;
				DLogError("v1: {0}, {1}", carState.rigidbody.velocity, carState.rigidbody.velocity.sqrMagnitude);
				yield return new WaitForEndOfFrame();
			}
			if (Application.isEditor)
			{
				Debug.DrawRay(carState.transform.position, carState.rigidbody.velocity, Color.green, 5f);
			}
			carView.SetSideWayFriction(carView.carModel.SideWayFriction);
		}

		public void OnReset()
		{
			DoStopSideWayFriction();
			RotAngle = 0f;
			carState.CollisionDirection = HitWallDirection.NONE;
		}

		private void DoStopSideWayFriction()
		{
			if (coFrictionWait != null)
			{
				stopFriction = true;
				carView.StopCoroutine(coFrictionWait);
				coFrictionWait = null;
			}
			carView.SetSideWayFriction(carView.carModel.SideWayFriction);
		}

		private void DLogError(string message, params object[] args)
		{
		}
	}
}
