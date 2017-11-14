using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using System.Configuration;
using System.Security.Principal;
using CacheManager.Core;
using CacheManager.Core.Logging;
using CacheManager.Serialization.Json;
using Newtonsoft.Json;
using Ninject.Web.Common.WebHost;

//using StructureMap.Attributes;


[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(cacheManTest.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(cacheManTest.App_Start.NinjectWebCommon), "Stop")]

namespace cacheManTest.App_Start
{

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ICacheHandler>().To<CacheHandler>().InSingletonScope();
            
            kernel.Bind<IIdentity>().ToConstant(WindowsIdentity.GetCurrent());

            var jsonSer = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };

            var jsonDeSer = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.None
            };

            //var gzSer = new GzJsonCacheSerializer(jsonSer, jsonDeSer);

            var endPoint = ConfigurationManager.AppSettings["RedisEndPoint"];
            var useDistCacheString = ConfigurationManager.AppSettings["UseDistCache"];
            var useDistCache = !string.IsNullOrEmpty(useDistCacheString) && Convert.ToBoolean(useDistCacheString) && !string.IsNullOrEmpty(endPoint);
            
            ICacheManagerConfiguration cacheConfig;

            if (string.IsNullOrEmpty(endPoint) && !useDistCache)
            {
                //const string redisKey = "redisConfig";
                cacheConfig = ConfigurationBuilder.BuildConfiguration("memandrediscache", settings =>
                {
                    settings.WithSystemWebCacheHandle("inProcessCache");
                });
            }
            else
            {
                var endPointUrl = endPoint.Split(':').Length > 0 ? endPoint.Split(':')[0] : "";
                var endPointPort = endPoint.Split(':').Length > 1 ? Convert.ToInt32(endPoint.Split(':')[1]) : 0;
                var password = ConfigurationManager.AppSettings["RedisPassword"];
                const string redisKey = "redisConfig";
                cacheConfig = ConfigurationBuilder.BuildConfiguration("memandrediscache", settings =>
                {
                    settings
                    .WithSystemWebCacheHandle("inProcessCache")
                    //.EnableStatistics()
                    .And
                    .WithRedisConfiguration(redisKey, config => config
                        .WithAllowAdmin()
                        .WithConnectionTimeout(5000)
                        .WithEndpoint(endPointUrl, endPointPort)
                        .WithPassword(password)
                    //.WithSsl()
                    )
                    .WithJsonSerializer(jsonSer, jsonDeSer)
                    .WithMaxRetries(5) //from 1000
                    .WithRetryTimeout(1000)
                    .WithRedisBackplane(redisKey)
                    .WithRedisCacheHandle(redisKey, true);
                });
            }

            kernel.Bind(typeof(ICacheManager<>)).ToMethod(context => 
                CacheFactory.FromConfiguration(context.GenericArguments[0], cacheConfig)).InSingletonScope();
            
        }
    }
}
