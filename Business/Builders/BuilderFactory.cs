using Business.Interfaces.Builders;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Builders;

[UsedImplicitly]
public class BuilderFactory(IServiceProvider serviceProvider) : IBuilderFactory
{
    public TBuilder Builder<TBuilder>() where TBuilder : class, IBuilderMarker
    {
        return serviceProvider.GetRequiredService<TBuilder>();
    }
}
