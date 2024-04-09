using UnityEngine;

namespace CarLogic
{
	[AddComponentMenu("CarLogic/Translate Starter")]
	public class TranslateStarter : MonoBehaviour
	{
		public int Id;

		public Collider StartTrigger;

		public Collider EndTrigger;

		public bool PushGrounded;

		public float TimeScaleNormal = 0.8f;

		public float TimeScaleN2Force = 0.65f;

		public AnimationCurve TimeCurve = AnimationCurve.Linear(0f, 0.4f, 0.7f, 1f);

		[HideInInspector]
		public float PastTime = 5f;

		[HideInInspector]
		public float TimeScale = 1f;

		public TranslatePath[] Paths;

		public void Awake()
		{
			if ((bool)EndTrigger)
			{
				EndTrigger.tag = "Forbidden";
			}
		}

		public void Start()
		{
			if (RacePathManager.ActiveInstance != null)
			{
				RacePathManager.ActiveInstance.TranslateMap[Id] = this;
			}
		}

		public void OnDestroy()
		{
			if (RacePathManager.ActiveInstance != null)
			{
				TranslateStarter value = null;
				if (RacePathManager.ActiveInstance.TranslateMap.TryGetValue(Id, out value) && value == this)
				{
					RacePathManager.ActiveInstance.TranslateMap.Remove(Id);
				}
			}
		}

		public int GetPathRandom(ref TranslatePath path)
		{
			int num = Random.Range(0, Paths.Length - 1);
			if (num < Paths.Length)
			{
				path = Paths[num];
			}
			else
			{
				num = -1;
			}
			return num;
		}

		public int GetPathCloest(CarState state, ref TranslatePath path)
		{
			int pathCloest = GetPathCloest(state.rigidbody.position, ref path);
			TimeScale = ((state.N2State.Level == 1) ? TimeScaleN2Force : TimeScaleNormal);
			return pathCloest;
		}

		public int GetPathCloest(Vector3 pos, ref TranslatePath path)
		{
			int result = 0;
			float num = float.MaxValue;
			if (Paths != null)
			{
				for (int i = 0; i < Paths.Length; i++)
				{
					TranslatePath translatePath = Paths[i];
					if (translatePath != null)
					{
						translatePath.CheckPointlist();
						float sqrMagnitude = (pos - translatePath.PointList[0].Position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							path = translatePath;
							result = i;
						}
					}
				}
			}
			TimeScale = TimeScaleNormal;
			return result;
		}
	}
}
