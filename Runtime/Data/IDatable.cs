using System.Collections.Generic;

namespace ZGame.Data
{
    public interface IDatable : IReference
    {
        long id { get; }
    }
}