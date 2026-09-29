using System.Net;
using System.Text;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Common.Emails;

// Wraps email content in the Prodify look: logo, white card, one optional
// button, small print. Inline styles only, as email apps ignore stylesheets.
public static class EmailLayout
{
    private const string Brand = "#0e7c66";

    public static EmailContent Build(
        string subject,
        string heading,
        IEnumerable<string> paragraphs,
        (string Label, string Url)? button = null,
        string? smallPrint = null)
    {
        var lines = paragraphs.ToList();
        var html = new StringBuilder();

        html.Append("<!doctype html><html><body style=\"margin:0;padding:24px;background:#f8fafc;font-family:Arial,Helvetica,sans-serif;color:#0f172a\">");
        html.Append("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\"><tr><td align=\"center\">");
        html.Append("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:560px\">");
        html.Append($"<tr><td style=\"padding:0 0 16px;font-size:24px;font-weight:bold;color:{Brand}\">prodify<span style=\"color:#f59e0b\">.</span></td></tr>");
        html.Append("<tr><td style=\"background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;padding:24px\">");
        html.Append($"<h1 style=\"margin:0 0 16px;font-size:20px\">{Encode(heading)}</h1>");

        foreach (var line in lines)
            html.Append($"<p style=\"margin:0 0 12px;font-size:15px;line-height:1.5\">{Encode(line)}</p>");

        if (button is { } b)
        {
            html.Append($"<p style=\"margin:20px 0\"><a href=\"{Encode(b.Url)}\" style=\"display:inline-block;background:{Brand};color:#ffffff;text-decoration:none;font-weight:bold;padding:12px 20px;border-radius:8px\">{Encode(b.Label)}</a></p>");
            html.Append($"<p style=\"margin:0;font-size:12px;color:#64748b\">If the button doesn't work, copy this link into your browser:<br><span style=\"word-break:break-all\">{Encode(b.Url)}</span></p>");
        }

        html.Append("</td></tr>");
        if (smallPrint is not null)
            html.Append($"<tr><td style=\"padding:16px 4px 0;font-size:12px;color:#64748b\">{Encode(smallPrint)}</td></tr>");
        html.Append("<tr><td style=\"padding:8px 4px 0;font-size:12px;color:#64748b\">Prodify &middot; Shop from sellers across Nigeria</td></tr>");
        html.Append("</table></td></tr></table></body></html>");

        var text = new StringBuilder();
        text.AppendLine(heading).AppendLine();
        foreach (var line in lines)
            text.AppendLine(line).AppendLine();
        if (button is { } t)
            text.AppendLine($"{t.Label}: {t.Url}").AppendLine();
        if (smallPrint is not null)
            text.AppendLine(smallPrint);

        return new EmailContent(subject, html.ToString(), text.ToString().TrimEnd());
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
