using UnityEngine;

public class DelayEndEffectToogle : EffectToggleBase
{
	protected bool stopFlag = true;

	public override void Init()
	{
		base.Init();
		stopFlag = true;
		StopAll();
	}

	protected void FixedUpdate()
	{
		if (Active || !stopFlag || ps == null)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < ps.Length; i++)
		{
			if (!ps[i].isStopped)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			StopAll();
		}
	}

	public void PlayAll()
	{
		if (ps != null)
		{
			for (int i = 0; i < ps.Length; i++)
			{
				ps[i].loop = true;
				ps[i].Simulate(0f, withChildren: true, restart: true);
				ps[i].Play();
			}
		}
		if (ms != null && !IgnoreRenders)
		{
			for (int j = 0; j < ms.Length; j++)
			{
				ms[j].enabled = true;
			}
		}
		if (ass != null)
		{
			for (int k = 0; k < ass.Length; k++)
			{
				ass[k].enabled = true;
			}
		}
		if (ats == null)
		{
			return;
		}
		for (int l = 0; l < ats.Length; l++)
		{
			Animator animator = ats[l];
			animator.enabled = true;
			for (int m = 0; m < animator.layerCount; m++)
			{
				animator.Play(animator.GetCurrentAnimatorStateInfo(m).nameHash, m, 0f);
			}
		}
	}

	public void StopAll()
	{
		for (int i = 0; i < ps.Length; i++)
		{
			if (null != ps[i])
			{
				ps[i].Stop();
			}
		}
		if (ms != null && !IgnoreRenders)
		{
			for (int j = 0; j < ms.Length; j++)
			{
				if (null != ms[j])
				{
					ms[j].enabled = false;
				}
			}
		}
		if (ass != null)
		{
			for (int k = 0; k < ass.Length; k++)
			{
				if (null != ass[k])
				{
					ass[k].enabled = false;
				}
			}
		}
		if (ats != null)
		{
			for (int l = 0; l < ats.Length; l++)
			{
				if (null != ats[l])
				{
					ats[l].enabled = false;
				}
			}
		}
		stopFlag = false;
	}

	public override void HandleActive()
	{
		if (Active)
		{
			if (stopFlag)
			{
				StopAll();
			}
			stopFlag = false;
			PlayAll();
			return;
		}
		stopFlag = true;
		if (ps == null)
		{
			return;
		}
		for (int i = 0; i < ps.Length; i++)
		{
			if (ps[i] != null)
			{
				ps[i].loop = false;
			}
		}
	}
}
