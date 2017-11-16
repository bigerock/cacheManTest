using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CacheManager.Core;
using System.Runtime.Caching;

namespace cacheManTest.Models
{
    public class HomeViewmodel
    {
        public const string RandomData = "a,b,c,d,e,f,g";
        private readonly ICacheHandler _cacheHandler;
        private const string _datetimeCache = "_cache.test.GetDateTime.";
        private const string _stringCache = "_cache.test.GetString.";

        public HomeViewmodel(ICacheHandler cacheHandler)
        {
            _cacheHandler = cacheHandler;
        }

        public IEnumerable<string> GetRandomDataFromDb()
        {
            var list = new List<string>();
            System.Threading.Thread.Sleep(1000); // getting something from the db
            var r = new Random();
            list.AddRange(RandomData.Split(','));
            return list.OrderBy(x => r.Next());
        }

        public DateTime? GetDateTime(int num)
        {
            var cacheName = $"{_datetimeCache}{num}";
            var cacheObj = _cacheHandler.GetObj<DateTime?>(cacheName);
            //var cacheDefault = _cacheHandler.IsObjectDefault(cacheName);

            //if (!cacheDefault && cacheObj != null) return cacheObj;
            if (cacheObj != null) return cacheObj;

            cacheObj = DateTime.Now;
            _cacheHandler.Insert(cacheName, cacheObj, ExpirationMode.Absolute, 5);

            return cacheObj;
        }

        public string GetString(int num)
        {
            var cacheName = $"{_stringCache}{num}";
            //var runtimeObj = HttpRuntime.Cache.Get(cacheName);
            var cacheObj = _cacheHandler.GetObj<string>(cacheName);
            
            if (cacheObj != null) return cacheObj;

            cacheObj = $"{num}";
            _cacheHandler.Insert(cacheName, cacheObj, ExpirationMode.Absolute, 5);

            return cacheObj;
        }

        public bool RemoveString(int num)
        {
            var cacheName = $"{_stringCache}{num}";
            var removed = _cacheHandler.RemoveObjectFromCache(cacheName);
            return removed;
        }

        public bool RemoveLocalString(int num)
        {
            var cacheName = $"{_stringCache}{num}";
            try
            {
                //var cache = MemoryCache.Default;
                //var obj = cache.Get(cacheName);
                //cache.Remove(cacheName);
                //var obj = HttpRuntime.Cache.Get(cacheName);
                
                HttpRuntime.Cache.Remove(cacheName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        

        //public DateTime? GetDateTime2()
        //{
        //    var cacheName = "_cache.test.GetDateTime.2";
        //    var cacheObj = _cacheHandler.GetObj<DateTime?>(cacheName);
        //    var cacheDefault = _cacheHandler.IsObjectDefault(cacheName);

        //    if (!cacheDefault && cacheObj != null) return cacheObj;

        //    cacheObj = DateTime.Now;
        //    _cacheHandler.Insert(cacheName, cacheObj, ExpirationMode.Absolute, 5);

        //    return cacheObj;
        //}
    }
}