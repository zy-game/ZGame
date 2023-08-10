using System;

public class EngineException : Exception
{
    public EngineException(object message) : base($"{message}")
    {
    }

    public static EngineException Create(object message)
    {
        return new EngineException(message);
    }

    public static EngineException Create<T>(object message) where T : Exception
    {
        return new EngineException(Activator.CreateInstance(typeof(T), new object[] { message }));
    }
}