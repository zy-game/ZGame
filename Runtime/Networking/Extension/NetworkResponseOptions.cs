namespace ZGame.Networking
{
    public class NetworkResponseOptions
    {
        public int code;
        public string msg;
        public string error;

        public virtual bool IsSuccess()
        {
            return (code == 0 || code == 200) && error.IsNullOrEmpty();
        }
    }

    public class NetworkResponseOptions<T> : NetworkResponseOptions
    {
        public T data;

        public override bool IsSuccess()
        {
            return base.IsSuccess() && data is not null;
        }
    }
}