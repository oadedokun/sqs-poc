using Autofac;

namespace StockQuantity2
{
    public static class IoCConfig
    {
        public static IContainer Container { get; private set; }

        public static void BuildContainer()
        {
            var builder = new ContainerBuilder();



            Container = builder.Build();
        }

        public static TService ResolveService<TService>()
        {
            return Container.Resolve<TService>();
        }

        public static void Inject<TService>(TService instance) where TService : class
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance)
                .As<TService>()
                .ExternallyOwned();
            builder.Update(Container);
        }
    }
}
