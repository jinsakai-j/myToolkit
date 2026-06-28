using OsintToolkit.Core.Enums;

namespace OsintToolkit.Core.Services;

public static class TargetClassifier
{
    public static bool IsSupported(TargetType targetType)
    {
        return Enum.IsDefined(targetType);
    }
}

