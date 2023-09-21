namespace ZEngine.Cache
{
    public static class CacheExtension
    {
        public static void HandleCache(this object target, string key)
        {
            Engine.Cache.Handle(key, target);
        }
    }
}