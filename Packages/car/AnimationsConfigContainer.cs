using System.Collections.Generic;
using CarLogic;
using UnityEngine;

public class AnimationsConfigContainer
{
	public CarModelType carModelType;

	private Dictionary<string, List<AnimationInfo>> _animationsConfig;

	public AnimationsConfigContainer()
	{
		_animationsConfig = new Dictionary<string, List<AnimationInfo>>();
	}

	public void AddAnimationInfo(string key, List<AnimationInfo> animations)
	{
		if (_animationsConfig != null)
		{
			_animationsConfig[key] = animations;
		}
	}

	public string GetAnimationRandom(string key)
	{
		string result = "";
		if (_animationsConfig != null && _animationsConfig.ContainsKey(key))
		{
			result = ((carModelType != CarModelType.Motorbike) ? _animationsConfig[key][Random.Range(0, _animationsConfig[key].Count)].AnimationName : _animationsConfig[key][Random.Range(0, _animationsConfig[key].Count)].MotorbikeAnimationName);
		}
		return result;
	}

	public string GetAnimaionByIndex(string key, int index)
	{
		string result = "";
		if (_animationsConfig != null && _animationsConfig.ContainsKey(key) && index >= 0 && _animationsConfig[key].Count > index)
		{
			result = ((carModelType != CarModelType.Motorbike) ? _animationsConfig[key][index].AnimationName : _animationsConfig[key][index].MotorbikeAnimationName);
		}
		return result;
	}

	public List<string> GetAnimations(string key)
	{
		List<string> list = new List<string>();
		if (_animationsConfig != null && _animationsConfig.ContainsKey(key))
		{
			List<AnimationInfo> list2 = _animationsConfig[key];
			for (int i = 0; i < list2.Count; i++)
			{
				if (carModelType == CarModelType.Motorbike)
				{
					list.Add(list2[i].MotorbikeAnimationName);
				}
				else
				{
					list.Add(list2[i].AnimationName);
				}
			}
		}
		return list;
	}
}
