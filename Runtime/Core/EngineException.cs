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
}