using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CacheManager.Core;

namespace cacheManTest.Models
{
    public class HomeViewmodel
    {
        public const string RandomData = "a,b,c,d,e,f,g";
        private readonly ICacheHandler _cacheHandler;

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
            var cacheName = $"_cache.test.GetDateTime.{num}";
            var cacheObj = _cacheHandler.GetObj<DateTime?>(cacheName);
            var cacheDefault = _cacheHandler.IsObjectDefault(cacheName);

            if (!cacheDefault && cacheObj != null) return cacheObj;

            cacheObj = DateTime.Now;
            _cacheHandler.Insert(cacheName, cacheObj, ExpirationMode.Absolute, 3);

            return cacheObj;
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