using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
#if DEBUG
using MultiCommentViewer.Test;
#endif //DEBUG
namespace MultiCommentViewer
{
    class DiContainer
    {
        private readonly IContainer _container;
        private DiContainer()
        {
            var builder = new ContainerBuilder();
#if DEBUG            
            builder.RegisterType<OptionsLoaderTest>().As<IOptionsLoader>();
            builder.RegisterType<SitePluginLoaderTest>().As<ISitePluginLoader>();
            //builder.RegisterType<SitePluginManager>().As<ISitePluginManager>();
            builder.RegisterType<BrowserLoader>().As<IBrowserLoader>();
            builder.RegisterType<UserStoreTest>().As<IUserStore>();
            builder.RegisterType<LoggerTest>().As<ILogger>();
            //builder.RegisterType<PluginManager>().As<IPluginManager>();
            //builder.RegisterType<>().As<>();

#endif //DEBUG


            _container = builder.Build();
        }
        private static readonly DiContainer _instance = new DiContainer();
        public static DiContainer Instance { get { return _instance; } }
        public T GetNewInstance<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
