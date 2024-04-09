public class DistanceParticleEffectToggle : EffectToggleBase
{
	protected override void SetParticleSystemActive(bool active)
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
				ps[i].enableEmission = true;
			}
		}
		else
		{
			for (int j = 0; j < ps.Length; j++)
			{
				ps[j].Stop();
				ps[j].enableEmission = false;
			}
		}
	}
}
