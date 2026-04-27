namespace TestTrace_V1.UI;

internal static class BrandAssets
{
    private const string BrandingFolder = "Assets\\Branding";

    public static Image? LoadBadge() => LoadImage("testtrace-badge.png");

    public static Image? LoadLogo() => LoadImage("testtrace-logo.png");

    public static Image? LoadWordmark() => LoadImage("testtrace-wordmark.png");

    public static PictureBox CreatePictureBox(Image? image, Size size, Padding margin)
    {
        var pictureBox = new PictureBox
        {
            BackColor = Color.Transparent,
            Image = image,
            Margin = margin,
            Size = size,
            SizeMode = PictureBoxSizeMode.Zoom,
            Visible = image is not null
        };

        return pictureBox;
    }

    private static Image? LoadImage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, BrandingFolder, fileName);
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, fileName);
        }

        if (!File.Exists(path))
        {
            return null;
        }

        using var source = Image.FromFile(path);
        return new Bitmap(source);
    }
}
