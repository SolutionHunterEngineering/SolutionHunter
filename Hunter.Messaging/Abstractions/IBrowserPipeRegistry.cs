namespace Messaging.Abstractions;

/// <summary>
/// Role-specific marker interface for BrowserHub registry.
/// Extends IPipeTargetRegistry but makes Hub injection unambiguous.
/// </summary>
public interface IBrowserPipeRegistry: IPipeTargetRegistry { }
