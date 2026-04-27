namespace TestTrace_V1.UI;

public static class OperatorSession
{
    public static OperatorProfile? Current { get; private set; }
    public static OperatorRegistry Registry { get; private set; } = new();

    public static void SignIn(OperatorProfile profile, OperatorRegistry registry)
    {
        Current = profile;
        Registry = registry;
        Registry.MarkActive(profile.OperatorId, DateTimeOffset.UtcNow);
        Registry.Save();
    }

    public static void Replace(OperatorProfile profile)
    {
        Current = profile;
        Registry.MarkActive(profile.OperatorId, DateTimeOffset.UtcNow);
        Registry.Save();
    }
}
