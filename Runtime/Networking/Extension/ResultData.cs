namespace ZGame.Networking
{
    public class ResultData
    {
        public int code;
        public string msg;
        public string error;

        public virtual bool IsSuccess()
        {
            return (code == 0 || code == 200) && (error == null || error.IsNullOrEmpty());
        }
    }

    public class ResultData<T> : ResultData
    {
        public T data;

        public override bool IsSuccess()
        {
            return base.IsSuccess() && data is not null;
        }
    }
}