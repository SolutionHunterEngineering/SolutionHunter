namespace Observer.Features; // adjust to your project

public static class FeatureFlags
{
#if DoPing
    public const bool DoPing = true;
#else
    public const bool DoPing = false;
#endif
}
