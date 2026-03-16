namespace Business.Interfaces.Builders;

public interface IBuilderFactory
{
    TBuilder Builder<TBuilder>() where TBuilder : class, IBuilderMarker;
}
