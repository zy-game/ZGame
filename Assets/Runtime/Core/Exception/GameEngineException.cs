using System;

public class GameEngineException : Exception
{
    public GameEngineException(object message) : base($"{message}")
    {
    }

    public static GameEngineException Create(object message)
    {
        return new GameEngineException(message);
    }
}