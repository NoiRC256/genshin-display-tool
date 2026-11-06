namespace GenshinScreenTool;

public sealed record ScreenSettings(int Width, int Height, bool Fullscreen);

public static class ScreenSettingsValidation
{
    public const int MinWidth = 640;
    public const int MaxWidth = 7680;
    public const int MinHeight = 480;
    public const int MaxHeight = 4320;

    public static bool TryValidate(ScreenSettings settings, out string error)
    {
        if (settings.Width is < MinWidth or > MaxWidth)
        {
            error = $"Width must be between {MinWidth} and {MaxWidth} pixels.";
            return false;
        }
        if (settings.Height is < MinHeight or > MaxHeight)
        {
            error = $"Height must be between {MinHeight} and {MaxHeight} pixels.";
            return false;
        }
        error = string.Empty;
        return true;
    }
}
