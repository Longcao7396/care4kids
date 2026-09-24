using System.Text.RegularExpressions;
using GiveAID.Application.Common.Interfaces;

namespace GiveAID.Infrastructure.Security;

public class HtmlSanitizer : IHtmlSanitizer
{
    // Whitelist of safe HTML tags
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "h1", "h2", "h3", "h4", "h5", "h6",
        "ul", "ol", "li", "a", "img", "blockquote", "code", "pre"
    };

    // Allowed attributes for each tag
    private static readonly Dictionary<string, HashSet<string>> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "a", new HashSet<string> { "href", "title", "target" } },
        { "img", new HashSet<string> { "src", "alt", "width", "height" } }
    };

    public string Sanitize(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Remove script and style tags completely
        html = Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove all tags except allowed ones
        html = Regex.Replace(html, @"</?(\w+)[^>]*>", match =>
        {
            var tagName = match.Groups[1].Value;
            if (AllowedTags.Contains(tagName))
            {
                // Keep the tag but strip dangerous attributes
                return StripDangerousAttributes(match.Value, tagName);
            }
            return string.Empty;
        }, RegexOptions.IgnoreCase);

        return html.Trim();
    }

    private static string StripDangerousAttributes(string tag, string tagName)
    {
        if (!AllowedAttributes.TryGetValue(tagName, out var allowedAttrs))
        {
            // No attributes allowed for this tag
            var openBracket = tag.IndexOf('<');
            var closeBracket = tag.IndexOf('>');
            if (openBracket >= 0 && closeBracket > openBracket)
            {
                var endIndex = tag.IndexOfAny(new[] { ' ', '>' }, openBracket + 1);
                if (endIndex > openBracket && tag[endIndex - 1] != '>')
                {
                    return tag[..(endIndex)] + ">";
                }
            }
            return tag;
        }

        // Remove all attributes first, then add back allowed ones
        var result = Regex.Replace(tag, @"\s+\w+=""[^""]*""", "");
        result = Regex.Replace(result, @"\s+\w+='[^']*'", "");
        result = Regex.Replace(result, @"\s+\w+=\S+", "");

        // Validate href/src URLs
        if (allowedAttrs.Contains("href") || allowedAttrs.Contains("src"))
        {
            var urlMatch = Regex.Match(result, @"(href|src)=""([^""]*)""");
            if (urlMatch.Success)
            {
                var url = urlMatch.Groups[2].Value;
                if (!IsSafeUrl(url))
                {
                    result = Regex.Replace(result, $@"{urlMatch.Groups[1].Value}=""{Regex.Escape(url)}""", "");
                }
            }
        }

        return result;
    }

    private static bool IsSafeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        // Only allow http, https, and mailto
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);
    }
}
