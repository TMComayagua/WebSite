using Autofac;

using Nancy.Bootstrapper;

namespace Starscream.Web.Api.Infrastructure.Configuration
{
    public class DataBaseErrorHandling
    {
        public static void Enable(IPipelines pipelines, ILifetimeScope container)
        {
            var handler = new DataBaseErrorInTransactionHandler(container);

            handler.Register(pipelines);
        }
    }
}