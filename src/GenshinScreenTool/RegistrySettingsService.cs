using System.IO;
using Microsoft.Win32;

namespace GenshinScreenTool;

public sealed class RegistrySettingsService
{
    private const string KeyPath = @"Software\miHoYo\原神";
    private const string WidthName = "Screenmanager Resolution Width_h182942802";
    private const string HeightName = "Screenmanager Resolution Height_h2627697771";
    private const string FullscreenName = "Screenmanager Is Fullscreen mode_h3981298716";

    public ScreenSettings Read()
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: false);
        if (key is null) throw new InvalidOperationException("The Genshin Impact settings key was not found. Start the game once, then try again.");
        var width = ReadInt(key, WidthName, "width");
        var height = ReadInt(key, HeightName, "height");
        var fullscreenValue = ReadInt(key, FullscreenName, "fullscreen");
        if (fullscreenValue is not 0 and not 1) throw new InvalidDataException("The fullscreen registry value must be a REG_DWORD containing only 0 or 1.");
        var fullscreen = fullscreenValue == 1;
        return new ScreenSettings(width, height, fullscreen);
    }

    public ScreenSettings Write(ScreenSettings settings)
    {
        if (!ScreenSettingsValidation.TryValidate(settings, out var validationError))
            throw new InvalidDataException(validationError);

        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true);
        if (key is null) throw new InvalidOperationException("The Genshin Impact settings key was not found. Start Genshin Impact once, close it, then try again. Apply will not create this key.");
        var oldValues = new Dictionary<string, object?>
        {
            [WidthName] = key.GetValue(WidthName, null, RegistryValueOptions.DoNotExpandEnvironmentNames),
            [HeightName] = key.GetValue(HeightName, null, RegistryValueOptions.DoNotExpandEnvironmentNames),
            [FullscreenName] = key.GetValue(FullscreenName, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
        };
        try
        {
            _ = key.GetValueKind(WidthName); _ = key.GetValueKind(HeightName); _ = key.GetValueKind(FullscreenName);
            key.SetValue(WidthName, settings.Width, RegistryValueKind.DWord);
            key.SetValue(HeightName, settings.Height, RegistryValueKind.DWord);
            key.SetValue(FullscreenName, settings.Fullscreen ? 1 : 0, RegistryValueKind.DWord);
            var verified = Read();
            if (verified != settings) throw new InvalidOperationException("Windows reported success, but the values did not verify after writing.");
            return verified;
        }
        catch
        {
            TryRollback(key, oldValues);
            throw;
        }
    }

    private static int ReadInt(RegistryKey key, string name, string label)
    {
        if (key.GetValueKind(name) != RegistryValueKind.DWord)
            throw new InvalidDataException($"The {label} registry value is missing or is not a REG_DWORD.");
        var value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        if (value is int integer) return integer;
        if (value is uint unsigned && unsigned <= int.MaxValue) return (int)unsigned;
        throw new InvalidDataException($"The {label} registry value is missing or is not a REG_DWORD.");
    }

    private static void TryRollback(RegistryKey key, Dictionary<string, object?> oldValues)
    {
        foreach (var pair in oldValues)
        {
            try
            {
                if (pair.Value is null) key.DeleteValue(pair.Key, throwOnMissingValue: false);
                else key.SetValue(pair.Key, pair.Value, RegistryValueKind.DWord);
            }
            catch
            {
                // Preserve the original write error; verification on the next read will expose a partial rollback.
            }
        }
    }
}
