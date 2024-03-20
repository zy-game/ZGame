namespace ZGame.Networking
{
    public class Response
    {
        public int code;
        public string msg;
        public string error;

        public virtual bool IsSuccess()
        {
            return (code == 0 || code == 200) && error.IsNullOrEmpty();
        }
    }

    public class Response<T> : Response
    {
        public T data;
    }
}