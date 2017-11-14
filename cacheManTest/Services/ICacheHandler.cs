using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheManager.Core;
using CacheManager.Core.Internal;

namespace cacheManTest
{
    public interface ICacheHandler
    {
        string GetCacheRegionFromCacheName(string cacheName);
        void Insert(string name, object obj, ExpirationMode expireMode = ExpirationMode.None, double expireMinutes = 0.0);
        bool IsObjectDefault(string name);
        T GetObj<T>(string name);
        IEnumerable<BaseCacheHandle<object>> GetAllCacheItems();
        void RemoveRegionFromCache(CacheType region);
        bool RemoveObjectFromCache(string name);
        //void LogToFile(string text);
        void WriteTimer();
        void WriteStatistics();
        void ClearTimer();
    }
}
