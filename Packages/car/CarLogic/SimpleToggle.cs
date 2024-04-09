namespace CarLogic
{
	public class SimpleToggle : EffectToggleBase
	{
		public bool ToggleActiveOnly = true;

		public override void Init()
		{
		}

		public override void OnToggle()
		{
			if (ToggleActiveOnly)
			{
				if (isActive)
				{
					base.gameObject.SetActive(value: false);
					base.gameObject.SetActive(value: true);
				}
			}
			else
			{
				base.gameObject.SetActive(isActive);
			}
		}
	}
}
