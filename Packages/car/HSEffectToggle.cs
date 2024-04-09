using CarLogic;
using UnityEngine;

[AddComponentMenu("CarLogic/HighSpeed Toggle")]
public class HSEffectToggle : EffectToggleBase
{
	public AnimationToggle StartEffect;

	public AnimationToggle EndEffect;

	public override void Init()
	{
		base.Init();
		if (StartEffect != null)
		{
			StartEffect.Init();
		}
		if (EndEffect != null)
		{
			EndEffect.Init();
		}
	}

	public override void OnToggle()
	{
		if (StartEffect == null || EndEffect == null)
		{
			return;
		}
		if (!Active)
		{
			if (!StartEffect.Active)
			{
				EndEffect.Active = false;
			}
			else
			{
				EndEffect.Active = true;
				RaceCallback.View.CallDelay(delegate
				{
					if (!Active)
					{
						EndEffect.Active = false;
					}
				}, EndEffect.Duration);
			}
		}
		else
		{
			EndEffect.Active = !Active;
		}
		StartEffect.Active = Active;
	}
}
