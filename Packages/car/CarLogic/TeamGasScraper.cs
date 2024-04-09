namespace CarLogic
{
	public class TeamGasScraper : N2ScrapeController
	{
		private float ratio = 0.25f;

		public void Init(CarState state, float ratio)
		{
			if (state.view == null)
			{
				base.Init(state, null);
				return;
			}
			base.Init(state, state.view.carModel);
			this.ratio = ratio;
		}

		protected override void onDrifting()
		{
			float num = scrapeValue;
			base.onDrifting();
			scrapeValue = num + ((float)scrapeValue - num) * ratio;
		}

		public void AddGasValue(float gain)
		{
			scrapeValue = (float)scrapeValue + gain;
			if (state.CurDriftState.Stage != 0)
			{
				driftStartValue += gain;
			}
		}
	}
}
