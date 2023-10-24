using System;

namespace ZGame
{
    /// <summary>
    /// 错误信息
    /// </summary>
    public interface IError : IEntity
    {
        int errorCode { get; }
        string message { get; }

        public static IError Create(int code, string message)
        {
            return new Errored()
            {
                errorCode = code,
                message = message
            };
        }

        class Errored : IError
        {
            public void Dispose()
            {
                errorCode = 0;
                message = String.Empty;
            }

            public int errorCode { get; set; }
            public string message { get; set; }
        }
    }
}