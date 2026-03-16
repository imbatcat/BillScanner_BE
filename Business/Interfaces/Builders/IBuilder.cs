namespace Business.Interfaces.Builders;

/// <summary>
/// Marker interface for all builders to facilitate automatic DI registration.
/// </summary>
public interface IBuilderMarker;

public interface IBuilder<out T> : IBuilderMarker
{
    T Build();
}
