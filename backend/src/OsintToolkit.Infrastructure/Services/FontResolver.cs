using PdfSharp.Fonts;

namespace OsintToolkit.Infrastructure.Services;

/// <summary>
/// Resolves the PDF base-14 font names to real TrueType fonts available on the
/// current machine. PdfSharp 6 no longer auto-bundles the base-14 fonts, so a
/// resolver is mandatory. On typical Linux installs DejaVu mirrors Helvetica
/// metrics closely enough for a text report. Falls back to the first found
/// font file when a specific variant is missing.
/// </summary>
public sealed class FontResolver : IFontResolver
{
    private static readonly Dictionary<string, string> FontFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Helvetica#r"] = "DejaVuSans.ttf",
        ["Helvetica#b"] = "DejaVuSans-Bold.ttf",
        ["Helvetica#i"] = "DejaVuSans-Oblique.ttf",
        ["Helvetica#bi"] = "DejaVuSans-BoldOblique.ttf",
        ["Courier#r"] = "DejaVuSansMono.ttf",
        ["Courier#b"] = "DejaVuSansMono-Bold.ttf",
        ["Courier#i"] = "DejaVuSansMono-Oblique.ttf",
        ["Courier#bi"] = "DejaVuSansMono-BoldOblique.ttf"
    };

    private static string? _fontsDirectory;
    private static readonly Dictionary<string, byte[]> FontCache = new(StringComparer.OrdinalIgnoreCase);

    private static string? FontsDirectory
    {
        get
        {
            if (_fontsDirectory != null)
            {
                return _fontsDirectory;
            }

            var candidates = new[]
            {
                "/usr/share/fonts/truetype/dejavu",
                "/usr/share/fonts/dejavu",
                "/usr/local/share/fonts",
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
            };

            _fontsDirectory = candidates.FirstOrDefault(Directory.Exists);
            return _fontsDirectory;
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
    {
        var normalized = familyName.ToLowerInvariant();
        var styleKey = normalized switch
        {
            "courier" or "monospace" or "consolas" => "Courier",
            _ => "Helvetica"
        };
        styleKey += bold switch
        {
            _ when bold && italic => "#bi",
            _ when bold => "#b",
            _ when italic => "#i",
            _ => "#r"
        };

        var faceName = styleKey;
        if (FontFiles.TryGetValue(styleKey, out var file) && FontsDirectory != null && File.Exists(Path.Combine(FontsDirectory, file)))
        {
            return new FontResolverInfo(faceName);
        }

        // Fallback: return the resolved name anyway; GetFont will pick a fallback file.
        return new FontResolverInfo(faceName);
    }

    public byte[]? GetFont(string faceName)
    {
        if (FontCache.TryGetValue(faceName, out var cached))
        {
            return cached;
        }

        var directory = FontsDirectory;
        if (directory == null)
        {
            return null;
        }

        if (FontFiles.TryGetValue(faceName, out var file) && File.Exists(Path.Combine(directory, file)))
        {
            return Cache(faceName, Path.Combine(directory, file));
        }

        // Best-effort fallback: any available TrueType font in the directory.
        var fallback = Directory.EnumerateFiles(directory, "*.ttf").FirstOrDefault();
        if (fallback == null)
        {
            return null;
        }

        return Cache(faceName, fallback);
    }

    private static byte[] Cache(string faceName, string path)
    {
        var bytes = File.ReadAllBytes(path);
        FontCache[faceName] = bytes;
        return bytes;
    }
}