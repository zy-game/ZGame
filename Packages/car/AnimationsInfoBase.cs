using System.Collections.Generic;
using CarLogic;

public abstract class AnimationsInfoBase
{
	protected Dictionary<string, AnimationsConfigContainer> _characterAnimations;

	protected AnimationsConfigContainer _defaultAnimations;

	protected bool _isInit;

	public bool IsInit => _isInit;

	protected AnimationsInfoBase()
	{
		_characterAnimations = new Dictionary<string, AnimationsConfigContainer>();
		_isInit = false;
	}

	public virtual void Init()
	{
		_isInit = true;
	}

	public AnimationsConfigContainer GetAnimationsConfig(string key, CarModelType carModelType = CarModelType.Car)
	{
		if (!string.IsNullOrEmpty(key) && _characterAnimations != null && _characterAnimations.ContainsKey(key))
		{
			return _characterAnimations[key];
		}
		_defaultAnimations.carModelType = carModelType;
		return _defaultAnimations;
	}
}
