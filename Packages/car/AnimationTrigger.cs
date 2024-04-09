using System;
using System.Collections.Generic;
using CarLogic;
using UnityEngine;

public class AnimationTrigger : SpecialTriggerBase
{
	private List<GameObject> animObjs;

	public AnimationTriggerData AnimData { get; set; }

	public override SpecialType Type => SpecialType.AnimationTrigger;

	public AnimationTrigger(Collider col, AnimationTriggerData animData)
	{
		AnimData = animData;
		if (col == null)
		{
			return;
		}
		Transform parent = col.transform.parent;
		if (parent == null)
		{
			return;
		}
		if (animObjs == null)
		{
			animObjs = new List<GameObject>();
		}
		animObjs.Clear();
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.CompareTag("AnimationObj"))
			{
				animObjs.Add(child.gameObject);
			}
		}
	}

	public bool isPlaying()
	{
		for (int i = 0; i < animObjs.Count; i++)
		{
			GameObject gameObject = animObjs[i];
			Animation[] componentsInChildren = gameObject.GetComponentsInChildren<Animation>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j].isPlaying)
					{
						return true;
					}
				}
			}
			ParticleSystem[] componentsInChildren2 = gameObject.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren2 == null || componentsInChildren2.Length == 0)
			{
				continue;
			}
			for (int k = 0; k < componentsInChildren2.Length; k++)
			{
				if (componentsInChildren2[k].isPlaying)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PlayAll()
	{
		if (animObjs == null)
		{
			return;
		}
		for (int i = 0; i < animObjs.Count; i++)
		{
			GameObject gameObject = animObjs[i];
			gameObject.SetActive(value: true);
			Animation[] componentsInChildren = gameObject.GetComponentsInChildren<Animation>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].Stop();
					componentsInChildren[j].Play();
				}
			}
			ParticleSystem[] componentsInChildren2 = gameObject.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
			{
				for (int k = 0; k < componentsInChildren2.Length; k++)
				{
					componentsInChildren2[k].Simulate(0f);
					componentsInChildren2[k].Play();
				}
			}
		}
	}

	public override void Toggle(CarState state)
	{
		if (animObjs == null || isPlaying() || AnimData == null)
		{
			return;
		}
		base.Toggle(state);
		AnimData.State = AnimationTriggerData.TriggerState.RUNNING;
		PlayAll();
		Action acUpdate = null;
		acUpdate = delegate
		{
			if (!isPlaying())
			{
				Break();
				CarView view2 = state.view;
				view2.OnUpdate = (Action)Delegate.Remove(view2.OnUpdate, acUpdate);
			}
		};
		CarView view = state.view;
		view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, acUpdate);
	}

	public override void Break()
	{
		if (animObjs != null)
		{
			animObjs.Clear();
		}
		animObjs = null;
		AnimData.State = AnimationTriggerData.TriggerState.STANDBY;
		AnimData = null;
		base.Break();
	}
}
