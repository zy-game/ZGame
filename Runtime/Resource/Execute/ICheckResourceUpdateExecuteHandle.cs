using System.Collections;

namespace ZEngine.Resource
{
    public interface ICheckResourceUpdateExecuteHandle : IExecuteAsyncHandle<ICheckResourceUpdateExecuteHandle>
    {
        void SubscribeUpdateProgress(ISubscribe<float> subscribe);
    }

    class DefaultCheckResourceUpdateExecuteHandle : ExecuteAsyncHandle<ICheckResourceUpdateExecuteHandle>, ICheckResourceUpdateExecuteHandle
    {
        public override void Execute(params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeUpdateProgress(ISubscribe<float> subscribe)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerator GenericExeuteCoroutine()
        {
            throw new System.NotImplementedException();
        }
    }
}