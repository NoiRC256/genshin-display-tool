using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace GenshinDisplayTool;

public partial class MainWindow : Window
{
    private readonly RegistrySettingsService _registry = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        try { SetFields(_registry.Read()); SetStatus("Current settings loaded.", false); }
        catch (Exception ex) { SetStatus(ex.Message, true); }
    }

    private void SetFields(ScreenSettings settings)
    {
        WidthBox.Text = settings.Width.ToString(CultureInfo.InvariantCulture); HeightBox.Text = settings.Height.ToString(CultureInfo.InvariantCulture);
        FullscreenRadio.IsChecked = settings.Fullscreen; WindowedRadio.IsChecked = !settings.Fullscreen;
    }

    private void ApplyClick(object sender, RoutedEventArgs e)
    {
        if (!TryReadFields(out var settings)) return;
        var confirmation = MessageBox.Show("These settings are persistent registry changes. Close Genshin Impact before applying so it cannot overwrite them. Continue?", "Confirm persistent change", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes) return;
        try { SetFields(_registry.Write(settings)); SetStatus($"Applied and verified {settings.Width} × {settings.Height} ({(settings.Fullscreen ? "fullscreen" : "windowed")}).", false); }
        catch (Exception ex) { SetStatus($"Could not apply settings: {ex.Message}", true); }
    }

    private bool TryReadFields(out ScreenSettings settings)
    {
        settings = new(0, 0, false);
        if (!int.TryParse(WidthBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) ||
            !int.TryParse(HeightBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
        {
            SetStatus("Enter a whole number for both width and height.", true);
            return false;
        }
        settings = new(width, height, FullscreenRadio.IsChecked == true);
        if (!ScreenSettingsValidation.TryValidate(settings, out var error)) { SetStatus(error, true); return false; }
        return true;
    }

    private void WidthTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        WidthPlaceholder.Visibility = string.IsNullOrEmpty(WidthBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void HeightTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        HeightPlaceholder.Visibility = string.IsNullOrEmpty(HeightBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void SetStatus(string text, bool error)
    {
        StatusText.Text = text;
        StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(error ? "#B33B55" : "#727A8D"));
    }

}
