using CarLogic;

internal class SustainedSpeedUpTrigger : SpecialTriggerBase
{
	private const int MAX_DURATION = 120;

	private CommonGasItem _gasItem;

	private bool _isRunning;

	public override SpecialType Type => SpecialType.SustainedSpeedUp;

	public bool IsRunning => _isRunning;

	public override void Toggle(CarState state)
	{
		base.Toggle(state);
		if (target != null && target.view != null)
		{
			if (!_isRunning && target.N2State.Level == 0)
			{
				ItemParams itemParams = new ItemParams(null, null, 0);
				itemParams.user = target;
				itemParams.targets = new CarState[1] { target };
				_gasItem = new CommonGasItem(1, 120f, useShake: false);
				if (_gasItem.Usable(itemParams))
				{
					_gasItem.Toggle(itemParams);
					_isRunning = true;
				}
			}
		}
		else
		{
			Stop();
		}
	}

	public override void Break()
	{
		if (target != null)
		{
			base.Break();
		}
		if (_gasItem != null)
		{
			_gasItem.Break();
		}
	}

	public override void Stop()
	{
		if (target != null)
		{
			base.Stop();
		}
		_isRunning = false;
	}
}
