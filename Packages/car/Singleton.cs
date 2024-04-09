public class Singleton<T> where T : IInit, new()
{
	protected static T instance;

	public static T Instance => instance;

	static Singleton()
	{
		instance = new T();
		instance.Init();
	}

	protected Singleton()
	{
	}
}
