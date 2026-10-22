using System.Drawing;

namespace GeradorMateriaisDesktop;

internal enum ThemeRole { Banner, Heading, Muted, PrimaryButton, Surface }

internal sealed class AppTheme(bool dark)
{
    public Color Background { get; } = ColorTranslator.FromHtml(dark ? "#171E29" : "#EEF3F8");
    public Color Surface { get; } = ColorTranslator.FromHtml(dark ? "#232E3E" : "#FFFFFF");
    public Color Text { get; } = ColorTranslator.FromHtml(dark ? "#E6EDF5" : "#203145");
    public Color Muted { get; } = ColorTranslator.FromHtml(dark ? "#B3C2D4" : "#696969");
    public Color Warning { get; } = ColorTranslator.FromHtml(dark ? "#FFB4AB" : "#8B0000");
    private Color Heading { get; } = ColorTranslator.FromHtml(dark ? "#9DCDFF" : "#173B63");
    private Color Accent { get; } = ColorTranslator.FromHtml(dark ? "#246AA5" : "#2879B8");
    private Color Border { get; } = ColorTranslator.FromHtml(dark ? "#536780" : "#C7D4E1");

    public void Apply(Control control, Color? parentBackground = null, bool bannerChild = false)
    {
        Color background = parentBackground ?? Background;
        bool banner = control.Tag is ThemeRole.Banner || bannerChild;
        if (control is Form) background = Background;
        if (control.Tag is ThemeRole.Surface || control is TextBoxBase or ComboBox) background = Surface;
        if (banner) background = ColorTranslator.FromHtml("#173B63");
        control.BackColor = background;
        control.ForeColor = banner ? Color.White : control.Tag switch
        {
            ThemeRole.Heading => Heading,
            ThemeRole.Muted => Muted,
            _ => Text
        };
        if (control is Button button)
        {
            bool primary = button.Tag is ThemeRole.PrimaryButton;
            button.UseVisualStyleBackColor = false;
            button.BackColor = primary ? Accent : Surface;
            button.ForeColor = primary ? Color.White : Heading;
            button.FlatAppearance.BorderColor = primary ? Accent : Border;
            button.FlatAppearance.MouseOverBackColor = primary ? Accent : Background;
            button.FlatAppearance.MouseDownBackColor = primary ? Accent : Background;
        }
        if (control is CheckBox check) check.UseVisualStyleBackColor = false;
        foreach (Control child in control.Controls) Apply(child, background, banner);
    }
}
