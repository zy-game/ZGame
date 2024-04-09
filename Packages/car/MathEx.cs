public class MathEx
{
	public static int Mod(int number, int divisor)
	{
		if (divisor <= 0)
		{
			return 0;
		}
		while (number < 0)
		{
			number += divisor;
		}
		return number % divisor;
	}
}
