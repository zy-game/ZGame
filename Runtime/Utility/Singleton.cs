using System;

namespace ZGame
{
    public class Singleton<T> where T : IEntity
    {
        public static T instance => SinglePipeline.GetInstance();

        class SinglePipeline
        {
            private static T _entity;

            public static T GetInstance()
            {
                if (_entity is not null)
                {
                    return _entity;
                }

                return _entity = Activator.CreateInstance<T>();
            }
        }
    }
}