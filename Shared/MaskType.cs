namespace Shared
  /// <summary>
  /// MaskType
  ///     When a message is sent from server or client code, it most often is of a single
  ///     class of information (in this list).
  ///     However a combination of types is possible and in particular, the Hunter Engineer
  ///     using Observer will set the mask, which then is the list of message *types* being
  ///     requested.
  /// </summary>
{
    /// <summary>
    /// Enumeration of log message masks used throughout the system.
    /// 
    /// IMPORTANT:
    ///   This enum uses [Flags], so values can be combined (OR’ed)
    ///   into a single mask in order to represent multiple log types.
    /// 
    /// Example:
    ///   var mask = MaskType.Information | MaskType.Warning | MaskType.Error;
    /// </summary>
    [Flags]
    public enum MaskType
    {
        /// <summary>
        /// No logging type specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Very detailed tracing information, typically for function entry/exit.
        /// Usually only enabled during deep debugging sessions.
        /// </summary>
        Trace = 1 << 1,

        /// <summary>
        /// General informational messages that might interest a developer.
        /// Used for normal operational flow documentation.
        /// </summary>
        Information = 1 << 2,

        /// <summary>
        /// Warning messages indicating values are still functional but approaching limits.
        /// Suggests potential issues that should be monitored.
        /// </summary>
        Warning = 1 << 3,

        /// <summary>
        /// Error messages for unexpected values or conditions that may not work correctly.
        /// Indicates problems that need attention but don't stop execution.
        /// </summary>
        Error = 1 << 4,

        /// <summary>
        /// Danger-level messages for values in ranges that may not produce proper results.
        /// More severe than warnings, indicates high risk of incorrect behavior.
        /// </summary>
        Danger = 1 << 5,

        /// <summary>
        /// Exception messages captured from try/catch blocks.
        /// Used to log caught exceptions with context information.
        /// </summary>
        Exception = 1 << 6,

        /// <summary>
        /// Important messages that should always be visible to developers.
        /// Reserved for critical system events and major state changes.
        /// </summary>
        Important = 1 << 7,

        /// <summary>
        /// Convenience flag representing all possible log types.
        /// Useful for debugging or when you want to capture *everything*.
        /// </summary>
        All = ~0
    }
}
