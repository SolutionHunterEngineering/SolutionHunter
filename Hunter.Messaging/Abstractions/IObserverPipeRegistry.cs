namespace Messaging.Abstractions;

/// <summary>
/// Role-specific marker interface for ObserverHub registry.
/// Extends IPipeTargetRegistry but makes Hub injection unambiguous.
/// </summary>
public interface IObserverPipeRegistry: IPipeTargetRegistry { }
