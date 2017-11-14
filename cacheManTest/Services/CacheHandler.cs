using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Web;
using CacheManager.Core;
using CacheManager.Core.Internal;
using Newtonsoft.Json;
using Ninject;

namespace cacheManTest
{
    public class CacheHandler : ICacheHandler
    {
        private readonly ICacheManager<object> _cache;
        //[Inject]
        //public ICacheManager<object> _cache { get; set; }
        public CacheHandler(ICacheManager<object> cache)
        {
            _cache = cache;
        }

        private class MethodTimer
        {
            public int Iterations { get; set; }
            public long TotalElapsedTime { get; set; }
        }

        private static Dictionary<string, MethodTimer> TimerDictionary = new Dictionary<string, MethodTimer>();

        public string GetCacheRegionFromCacheName(string cacheName)
        {
            return !cacheName.StartsWith("_cache.", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : cacheName.Split('.')[1];
        }

        public void Insert(string name, object obj, ExpirationMode expireMode = ExpirationMode.None, double expireMinutes = 0.0)
        {
            //var time = Stopwatch.StartNew();
            
            var cacheItem = new CacheItem<object>(name.ToLowerInvariant(), 
                GetCacheRegionFromCacheName(name.ToLowerInvariant()), 
                obj, expireMode, TimeSpan.FromMinutes(expireMinutes));

            //if (expireMinutes <= 0.0)
            //{
            //    LogToFile(name);
            //}

            _cache.Put(cacheItem); 
            
            //time.Stop();
            //var milliseconds = time.ElapsedMilliseconds;
            //AddTime("Insert", milliseconds);
        }


        private const string _emptyCacheObject = "_empty";

        /// <summary>
        /// Used to return default object instead of null so that it doesn't keep querying the db or cache.
        /// This should only be expensive if the object is not loaded into memory.
        /// When doing an object insert, if it's null, it will insert with the default object instead of null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsObjectDefault(string name)
        {
            //var time = Stopwatch.StartNew();
            
            var cacheObj = _cache.Get(name.ToLowerInvariant(), GetCacheRegionFromCacheName(name.ToLowerInvariant()));

            //time.Stop();
            //var milliseconds = time.ElapsedMilliseconds;
            //AddTime("IsObjectDefault", milliseconds);

            return cacheObj is string && cacheObj.ToString() == _emptyCacheObject;

            ////logToFile(name);
        }

        public T GetObj<T>(string name)
        {
            //var time = Stopwatch.StartNew();
            var ret = default(T);
            try
            {
                ret = _cache.Get<T>(name.ToLowerInvariant(), GetCacheRegionFromCacheName(name.ToLowerInvariant()));
            }
            catch (Exception)
            {
                //to log or not to log? (likely a null entry)
            }

            //var ret = _cache.Get<T>(name.ToLowerInvariant(), GetCacheRegionFromCacheName(name.ToLowerInvariant()));

            //time.Stop();
            //var milliseconds = time.ElapsedMilliseconds;
            //AddTime("GetObj", milliseconds);

            return ret;
        }

        public IEnumerable<BaseCacheHandle<object>> GetAllCacheItems()
        {
            return _cache.CacheHandles;
        }

        public void RemoveRegionFromCache(CacheType region)
        {
            _cache.ClearRegion(region.ToLowerObj());
        }

        public bool RemoveObjectFromCache(string name)
        {
            var region = GetCacheRegionFromCacheName(name.ToLowerInvariant());
            return _cache.Remove(name.ToLowerInvariant(), region);
        }

        //temp debug code
        private static void LogToFile(string text)
        {
            try
            {
                File.AppendAllText(@"C:\_temp\getObj.txt", text + "\n");
            }
            catch (Exception)
            {
                //don't log failure
            }
        }

        private void logToConsole(string text)
        {
            Debug.WriteLine(text);
        }

        public void ClearTimer()
        {
            TimerDictionary.Clear();
        }

        public void WriteTimer()
        {
            var sb = new StringBuilder();
            foreach (var t in TimerDictionary)
            {
                sb.AppendFormat("{0}, i:{1}, t:{2}\n", t.Key, t.Value.Iterations, t.Value.TotalElapsedTime);
            }
            logToConsole(sb.ToString());
            LogToFile(sb.ToString());
        }

        public void WriteStatistics()
        {
            //var cache = CreateInMemoryCacheWithRedisBackplane();
            var sb = new StringBuilder();
            var i = 0;
            foreach (var handle in _cache.CacheHandles)
            {
                i++;
                var stats = handle.Stats;
                sb.AppendFormat(
                        "Handle:{9}...Items: {0}, Hits: {1}, Miss: {2}, Remove: {3}, ClearRegion: {4}, Clear: {5}, Adds: {6}, Puts: {7}, Gets: {8} - ",
                            stats.GetStatistic(CacheStatsCounterType.Items),
                            stats.GetStatistic(CacheStatsCounterType.Hits),
                            stats.GetStatistic(CacheStatsCounterType.Misses),
                            stats.GetStatistic(CacheStatsCounterType.RemoveCalls),
                            stats.GetStatistic(CacheStatsCounterType.ClearRegionCalls),
                            stats.GetStatistic(CacheStatsCounterType.ClearCalls),
                            stats.GetStatistic(CacheStatsCounterType.AddCalls),
                            stats.GetStatistic(CacheStatsCounterType.PutCalls),
                            stats.GetStatistic(CacheStatsCounterType.GetCalls),
                            i
                        );
            }
            LogToFile(sb.ToString());
        }

        private void AddTime(string method, long time)
        {
            //method = method.ToLowerInvariant();
            //if (!TimerDictionary.ContainsKey(method))
            //{
            //    var tmr = new MethodTimer {Iterations = 1, Method = method, TotalElapsedTime = time};
            //    TimerDictionary.Add(method, tmr);
            //}
            //else
            //{
            //    MethodTimer mtd;
            //    if (!TimerDictionary.TryGetValue(method, out mtd)) return;
            //    mtd.Iterations += 1;
            //    mtd.TotalElapsedTime += time;
            //    TimerDictionary.Remove(method);
            //    TimerDictionary.Add(method, mtd);
            //}
        }
    }
}
