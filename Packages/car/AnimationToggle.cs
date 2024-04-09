using UnityEngine;

[AddComponentMenu("CarLogic/Animation Toggle")]
public class AnimationToggle : EffectToggleBase
{
	private Animation[] ans;

	private float duration;

	public float Duration => duration;

	public override void Init()
	{
		base.Init();
		ans = GetComponentsInChildren<Animation>();
		duration = 0f;
		if (ans == null)
		{
			return;
		}
		for (int i = 0; i < ans.Length; i++)
		{
			Animation animation = ans[i];
			if (animation != null && (bool)animation.clip && animation.clip.length > duration)
			{
				duration = animation.clip.length;
			}
		}
	}

	public override void OnToggle()
	{
		base.OnToggle();
		if (ans == null)
		{
			return;
		}
		for (int i = 0; i < ans.Length; i++)
		{
			Animation animation = ans[i];
			if (animation != null)
			{
				if (Active)
				{
					animation.Play();
				}
				else
				{
					animation.Stop();
				}
			}
		}
	}
}
