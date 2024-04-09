using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("CarLogic/Effect Toggle")]
public class EffectToggleBase : MonoBehaviour, IInit
{
	protected bool isActive;

	protected ParticleSystem[] ps;

	protected Renderer[] ms;

	protected AudioSource[] ass;

	protected EffectToggleBase[] toggles;

	protected Animator[] ats;

	public bool IgnoreRenders;

	public virtual bool Active
	{
		get
		{
			return isActive;
		}
		set
		{
			isActive = value;
			OnToggle();
		}
	}

	public void Awake()
	{
	}

	public virtual void Reinit()
	{
		ps = null;
		ms = null;
		ass = null;
		ats = null;
		toggles = null;
		Init();
	}

	public virtual void Init()
	{
		if (ps == null || ps.Length == 0)
		{
			ps = GetComponentsInChildren<ParticleSystem>();
		}
		if (ms == null || ms.Length == 0)
		{
			ms = GetComponentsInChildren<Renderer>();
		}
		if (ass == null || ass.Length == 0)
		{
			ass = GetComponentsInChildren<AudioSource>();
		}
		if (ats == null || ats.Length == 0)
		{
			ats = GetComponentsInChildren<Animator>();
		}
		List<EffectToggleBase> list = new List<EffectToggleBase>(GetComponentsInChildren<EffectToggleBase>());
		if (list.Count > 0)
		{
			list.Remove(this);
			toggles = list.ToArray();
		}
	}

	protected virtual void SetParticleSystemActive(bool active)
	{
		if (ps == null)
		{
			return;
		}
		if (active)
		{
			for (int i = 0; i < ps.Length; i++)
			{
				ps[i].Play();
			}
		}
		else
		{
			for (int j = 0; j < ps.Length; j++)
			{
				ps[j].Stop();
			}
		}
	}

	protected virtual void SetRendererActive(bool active)
	{
		if (ms != null && !IgnoreRenders)
		{
			for (int i = 0; i < ms.Length; i++)
			{
				ms[i].enabled = active;
			}
		}
	}

	protected virtual void SetAudioSourceActive(bool active)
	{
		if (ass != null)
		{
			for (int i = 0; i < ass.Length; i++)
			{
				ass[i].enabled = active;
			}
		}
	}

	protected virtual void SetAnimatorActive(bool active)
	{
		if (ats != null)
		{
			for (int i = 0; i < ats.Length; i++)
			{
				ats[i].enabled = active;
			}
		}
	}

	public virtual void HandleActive()
	{
		SetParticleSystemActive(Active);
		SetRendererActive(Active);
		SetAudioSourceActive(Active);
		SetAnimatorActive(Active);
	}

	public virtual void OnToggle()
	{
		if (toggles != null)
		{
			for (int i = 0; i < toggles.Length; i++)
			{
				toggles[i].Active = isActive;
			}
		}
		HandleActive();
	}
}
