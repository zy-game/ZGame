using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemTranslator : ItemTweener
	{
		private Transform item;

		private float minDistance = 2f;

		private float speed = 10f;

		private float accel = 90f;

		private float floatTime = 0.3f;

		private Vector3 velocity;

		private Vector3 vt1;

		private Vector3 oldDir;

		private Vector3 floatOffset = new Vector3(0.4f, 0.5f, 0.2f);

		private Transform user;

		private bool autoDestroy;

		private AbstractView view;

		private float upGoTime = 0.5f;

		private int stage;

		public Action OnStageChange;

		public Vector3 FloatOffset
		{
			get
			{
				return floatOffset;
			}
			set
			{
				floatOffset = value;
			}
		}

		public Transform User
		{
			get
			{
				return user;
			}
			set
			{
				user = value;
			}
		}

		public float Acceleration
		{
			get
			{
				return accel;
			}
			set
			{
				accel = value;
			}
		}

		public float StartSpeed
		{
			get
			{
				return speed;
			}
			set
			{
				speed = value;
			}
		}

		public float UpGoTime
		{
			get
			{
				return upGoTime;
			}
			set
			{
				upGoTime = value;
			}
		}

		public float MinDistance
		{
			get
			{
				return minDistance;
			}
			set
			{
				minDistance = value;
			}
		}

		public float FloatTime
		{
			get
			{
				return floatTime;
			}
			set
			{
				floatTime = value;
			}
		}

		public ItemTranslator(CarState target, Transform item, bool destroyItem = false, Action callback = null, float duration = 3f)
			: base(target, duration, callback)
		{
			this.item = item;
			autoDestroy = destroyItem;
		}

		public override void Start()
		{
			base.Start();
			if (item == null)
			{
				Debug.LogWarning("Null item");
				finish();
				return;
			}
			if (target == null)
			{
				CarState carState = new CarState();
				GameObject gameObject = new GameObject("EmuTarget");
				view = RaceCallback.View;
				carState.transform = gameObject.transform;
				target = carState;
				carState.transform.position = item.TransformPoint(new Vector3(0f, 0f, 1000f));
				item.LookAt(target.transform);
			}
			else
			{
				view = target.view;
				if (floatTime <= 0f)
				{
					stage = 1;
				}
			}
			if (view == null)
			{
				finish();
				return;
			}
			if (user == null)
			{
				floatTime = 0f;
			}
			AbstractView abstractView = view;
			abstractView.OnUpdate = (Action)Delegate.Combine(abstractView.OnUpdate, new Action(update));
			AbstractView abstractView2 = view;
			abstractView2.OnCollisionBegin = (Action<Collision>)Delegate.Combine(abstractView2.OnCollisionBegin, new Action<Collision>(onCollisionEnter));
		}

		private void update()
		{
			if (Time.time - startTime > maxDuration)
			{
				finish();
				return;
			}
			if (target == null || target.transform == null || item == null)
			{
				finish();
				return;
			}
			try
			{
				vt1 = target.transform.position - item.position;
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex.ToString());
			}
			if (vt1.sqrMagnitude < minDistance * minDistance)
			{
				finish();
				return;
			}
			if (stage != 0 && Vector3.Dot(vt1, oldDir) < 0f)
			{
				finish();
				return;
			}
			oldDir = vt1;
			switch (stage)
			{
				case 0:
					if (Time.time - startTime > floatTime || user == null)
					{
						stage = 1;
					}
					if (user != null)
					{
						item.position = user.TransformPoint(floatOffset);
					}
					break;
				case 1:
					velocity.Set(0f, speed * Time.deltaTime, 0f);
					item.Translate(velocity, Space.World);
					velocity.Set(0f, 0f, Time.deltaTime * speed);
					item.Translate(velocity, Space.Self);
					if (Time.time - startTime > upGoTime + floatTime)
					{
						stage = 2;
						if (OnStageChange != null)
						{
							OnStageChange();
						}
					}
					break;
				default:
					item.LookAt(target.transform);
					speed += accel * Time.deltaTime;
					velocity.Set(0f, 0f, speed * Time.deltaTime);
					item.Translate(velocity, Space.Self);
					break;
			}
		}

		private void onCollisionEnter(Collision col)
		{
			if (col.collider.transform.Equals(item))
			{
				finish();
			}
		}

		public override void finish()
		{
			if (view != null)
			{
				AbstractView abstractView = view;
				abstractView.OnUpdate = (Action)Delegate.Remove(abstractView.OnUpdate, new Action(update));
				AbstractView abstractView2 = view;
				abstractView2.OnCollisionBegin = (Action<Collision>)Delegate.Remove(abstractView2.OnCollisionBegin, new Action<Collision>(onCollisionEnter));
			}
			if (!finished)
			{
				base.finish();
				if (autoDestroy && (bool)item)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
		}
	}
}
