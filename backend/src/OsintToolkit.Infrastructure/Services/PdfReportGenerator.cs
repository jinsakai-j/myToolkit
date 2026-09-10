using System.Text;
using System.Text.Json;
using OsintToolkit.Core.Entities;
using OsintToolkit.Core.Interfaces;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace OsintToolkit.Infrastructure.Services;

/// <summary>
/// Generates a PDF report for a scan using PdfSharp. The layout is a simple,
/// dependency-light report: header with scan metadata, then one section per
/// module result with a summary and the raw JSON payload formatted with newlines.
/// Fonts are limited to the PDF base-14 set so no font files need to be shipped.
/// </summary>
public sealed class PdfReportGenerator : IReportPdfGenerator
{
    static PdfReportGenerator()
    {
        GlobalFontSettings.FontResolver ??= new FontResolver();
    }

    public Task<byte[]> GenerateAsync(Scan scan, CancellationToken cancellationToken = default)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"OSINT Scan Report - {scan.Target}";
        document.Info.Author = "OSINT Toolkit Local";
        document.Info.Subject = $"Scan {scan.Id}";

        var writer = new PdfWriter(document);

        writer.DrawHeader(scan);
        writer.DrawMetadata(scan);

        if (scan.Results.Count == 0)
        {
            writer.WriteText("This scan contains no module results.");
        }
        else
        {
            foreach (var result in scan.Results)
            {
                writer.WriteSection($"Module: {result.ModuleName}");
                writer.WriteKeyValue("Status", result.Status.ToString());
                writer.WriteKeyValue("Summary", result.Summary ?? "-");
                writer.WriteKeyValue("Created", result.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
                writer.WriteLabeledBlock("Raw data", PrettifyJson(result.RawData));
            }
        }

        using var stream = new MemoryStream();
        document.Save(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static string PrettifyJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "{}";
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return raw;
        }
    }

    private sealed class PdfWriter(PdfDocument document)
    {
        private const double Margin = 48;
        private const double PageWidth = 595.28;  // A4 portrait
        private const double PageHeight = 841.89; // A4 portrait
        private const double BottomLimit = PageHeight - Margin;
        private const int SoftLineWidth = 100;

        private double _y = Margin;
        private XGraphics? _gfx;

        private static readonly XFont TitleFont = new("Helvetica", 20, XFontStyleEx.Bold);
        private static readonly XFont SubtitleFont = new("Helvetica", 11, XFontStyleEx.Regular);
        private static readonly XFont HeadingFont = new("Helvetica", 12, XFontStyleEx.Bold);
        private static readonly XFont BodyFont = new("Helvetica", 10, XFontStyleEx.Regular);
        private static readonly XFont LabelFont = new("Helvetica", 10, XFontStyleEx.Bold);

        private static readonly XBrush DarkBrush = new XSolidBrush(XColor.FromArgb(30, 41, 59));
        private static readonly XBrush MutedBrush = new XSolidBrush(XColor.FromArgb(100, 116, 139));
        private static readonly XPen LinePen = new(XColor.FromArgb(148, 163, 184), 0.8);

        public void DrawHeader(Scan scan)
        {
            EnsurePage();
            _gfx!.DrawString("OSINT Toolkit - Scan Report", TitleFont, DarkBrush, Margin, _y);
            Advance(TitleFont.Height + 4);

            _gfx.DrawString(scan.Target, SubtitleFont, MutedBrush, Margin, _y);
            Advance(SubtitleFont.Height + 16);
            _gfx.DrawLine(LinePen, Margin, _y, PageWidth - Margin, _y);
            Advance(14);
        }

        public void DrawMetadata(Scan scan)
        {
            WriteKeyValue("Target type", scan.TargetType.ToString());
            WriteKeyValue("Status", scan.Status.ToString());
            WriteKeyValue("Risk score", scan.RiskScore.HasValue ? $"{scan.RiskScore.Value}/100" : "N/A");
            WriteKeyValue("Created", scan.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
            WriteKeyValue("Started", scan.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "-");
            WriteKeyValue("Completed", scan.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "-");
            WriteKeyValue("Scan ID", scan.Id.ToString());
            if (!string.IsNullOrWhiteSpace(scan.Notes))
            {
                WriteKeyValue("Notes", scan.Notes);
            }
            Advance(12);
            _gfx!.DrawLine(LinePen, Margin, _y, PageWidth - Margin, _y);
            Advance(14);
        }

        public void WriteSection(string title)
        {
            EnsureSpaceFor(HeadingFont.Height + 10);
            _gfx!.DrawString(title, HeadingFont, DarkBrush, Margin, _y);
            Advance(HeadingFont.Height + 8);
        }

        public void WriteKeyValue(string key, string value)
        {
            EnsureSpaceFor(LabelFont.Height + 4);
            _gfx!.DrawString($"{key}:", LabelFont, DarkBrush, Margin, _y);
            var labelWidth = _gfx.MeasureString($"{key}:", LabelFont).Width;
            WriteWrapped(value, LabelFont, Margin + labelWidth + 8);
        }

        public void WriteLabeledBlock(string label, string text)
        {
            EnsureSpaceFor(LabelFont.Height + 8);
            _gfx!.DrawString(label, LabelFont, MutedBrush, Margin, _y);
            Advance(LabelFont.Height + 6);
            WriteWrapped(text, BodyFont, Margin);
        }

        public void WriteText(string text)
        {
            EnsureSpaceFor(BodyFont.Height + 2);
            WriteWrapped(text, BodyFont, Margin);
        }

        private void WriteWrapped(string text, XFont font, double x)
        {
            var usableWidth = PageWidth - Margin - Margin;
            foreach (var line in SplitLines(text))
            {
                var remaining = line;
                while (remaining.Length > 0)
                {
                    var fitted = FitIntoWidth(remaining, font, usableWidth);
                    EnsureSpaceFor(font.Height + 2);
                    _gfx!.DrawString(Sanitize(fitted), font, DarkBrush, x, _y);
                    Advance(font.Height + 2);
                    remaining = remaining.Length > fitted.Length ? remaining[fitted.Length..] : string.Empty;
                }
            }

            Advance(6);
        }

        private string FitIntoWidth(string text, XFont font, double maxWidth)
        {
            if (text.Length <= SoftLineWidth || _gfx == null)
            {
                return text;
            }

            var candidate = text;
            for (var length = Math.Min(text.Length, SoftLineWidth + 10); length > 10; length -= 5)
            {
                candidate = text[..length];
                if (_gfx.MeasureString(Sanitize(candidate), font).Width <= maxWidth)
                {
                    var breakIndex = candidate.LastIndexOf(' ');
                    if (breakIndex > 0 && _gfx.MeasureString(Sanitize(candidate[..breakIndex]), font).Width <= maxWidth)
                    {
                        return candidate[..breakIndex];
                    }

                    return candidate;
                }
            }

            return candidate;
        }

        private static IEnumerable<string> SplitLines(string text)
        {
            return text.Replace("\r\n", "\n")
                .Split('\n')
                .Select(line => line.TrimEnd())
                .Where(line => line.Length > 0)
                .SelectMany(SplitWideLine);
        }

        private static IEnumerable<string> SplitWideLine(string line)
        {
            if (line.Length <= SoftLineWidth)
            {
                yield return line;
                yield break;
            }

            for (var i = 0; i < line.Length; i += SoftLineWidth)
            {
                yield return line.Substring(i, Math.Min(SoftLineWidth, line.Length - i));
            }
        }

        private static string Sanitize(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                sb.Append(c <= 0xFF ? c : '?');
            }
            return sb.ToString();
        }

        private void EnsureSpaceFor(double needed)
        {
            if (_y + needed > BottomLimit)
            {
                NewPage();
            }
        }

        private void EnsurePage()
        {
            if (_gfx == null)
            {
                NewPage();
            }
        }

        private void NewPage()
        {
            var page = new PdfPage
            {
                Size = PageSize.A4,
                Orientation = PageOrientation.Portrait
            };
            document.AddPage(page);
            _gfx = XGraphics.FromPdfPage(page);
            _y = Margin;
        }

        private void Advance(double height)
        {
            _y += height;
        }
    }
}