using UnityEngine;

[ExecuteInEditMode]
public class AnimationTriggerData : MonoBehaviour
{
	public enum TriggerType
	{
		EVERY_TIMES,
		RACE_END
	}

	public enum TriggerTarget
	{
		ONLY_SELF,
		ORTHERS,
		ALL
	}

	public enum TriggerState
	{
		RUNNING,
		STANDBY
	}

	public TriggerType Type;

	public TriggerTarget Target;

	public TriggerState State = TriggerState.STANDBY;

	private void Awake()
	{
		base.gameObject.layer = 23;
		State = TriggerState.STANDBY;
	}
}
