using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Conservative HTML sanitizer for user-supplied CMS / FAQ content.
    /// CMS content is stored raw in the database and rendered with
    /// dangerouslySetInnerHTML on the public site, so any malicious script
    /// stored here executes in every visitor's browser. This helper strips
    /// dangerous tags/attributes using an allowlist approach.
    ///
    /// SECURITY NOTES:
    ///   1. This is a best-effort sanitizer, not a full HTML parser. For
    ///      defence-in-depth, the frontend should also run DOMPurify over
    ///      the same content before dangerouslySetInnerHTML.
    ///   2. We allow a minimal tag set (formatting + headings + lists +
    ///      links + images). No &lt;script&gt;, no &lt;iframe&gt;, no inline
    ///      event handlers (onclick, onerror, etc.), no javascript: URIs.
    ///   3. CSS in style attributes is stripped — too easy to exfiltrate
    ///      with background-image: url() and data exfil tricks.
    /// </summary>
    public static class HtmlSanitizer
    {
        private static readonly HashSet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "strong", "b", "em", "i", "u", "s",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li",
            "blockquote", "pre", "code",
            "a", "img",
            "table", "thead", "tbody", "tr", "th", "td",
            "hr", "span", "div"
        };

        private static readonly HashSet<string> AllowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href", "title", "alt", "src",
            "target", "rel"
        };

        private static readonly Regex ScriptTagPattern = new Regex(
            @"<\s*script[^>]*>.*?<\s*/\s*script\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex AnyTagPattern = new Regex(
            @"<\s*(/?)\s*([a-zA-Z][a-zA-Z0-9]*)\b([^>]*)>",
            RegexOptions.Compiled);

        private static readonly Regex AttributePattern = new Regex(
            @"\s+([a-zA-Z_:][a-zA-Z0-9_:\-\.]*)\s*(=\s*(""(?:[^""]|"""")*""|'(?:[^']|'')*'|[^\s>]*))?",
            RegexOptions.Compiled);

        private static readonly Regex EventHandlerPattern = new Regex(
            @"^on[a-z]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            // Step 1: drop <script>...</script> entirely (tag + content).
            html = ScriptTagPattern.Replace(html, string.Empty);

            // Step 2: drop <iframe>, <object>, <embed>, <form>, <style>, <link>.
            html = Regex.Replace(html,
                @"<\s*(iframe|object|embed|form|style|link|meta|base)\b[^>]*>.*?<\s*/\s*\1\s*>",
                string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Self-closing variants
            html = Regex.Replace(html,
                @"<\s*(iframe|object|embed|form|style|link|meta|base)\b[^>]*/?>",
                string.Empty, RegexOptions.IgnoreCase);

            // Step 3: walk every remaining tag; drop disallowed tags + attributes.
            html = AnyTagPattern.Replace(html, match =>
            {
                var isClosing = !string.IsNullOrEmpty(match.Groups[1].Value);
                var tagName = match.Groups[2].Value;
                var rawAttrs = match.Groups[3].Value;

                if (!AllowedTags.Contains(tagName))
                {
                    // Strip disallowed tags entirely (e.g. <video>, <source>).
                    return string.Empty;
                }

                if (isClosing)
                {
                    return "</" + tagName + ">";
                }

                var cleanAttrs = ExtractSafeAttributes(rawAttrs, tagName);
                return "<" + tagName + (cleanAttrs.Length > 0 ? " " + cleanAttrs : "") + ">";
            });

            // Step 4: javascript: URI defence (catch any escape attempts).
            html = Regex.Replace(html, @"javascript\s*:", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"vbscript\s*:", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"data\s*:\s*text/html", "", RegexOptions.IgnoreCase);

            return html;
        }

        private static string ExtractSafeAttributes(string rawAttrs, string tagName)
        {
            if (string.IsNullOrWhiteSpace(rawAttrs)) return string.Empty;

            var matches = AttributePattern.Matches(rawAttrs);
            var kept = new List<string>();

            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;
                var value = m.Groups[3].Success ? m.Groups[3].Value : null;

                // Drop event handlers (onclick, onerror, onload, ...)
                if (EventHandlerPattern.IsMatch(name)) continue;
                // Drop style attributes (CSS exfil)
                if (string.Equals(name, "style", StringComparison.OrdinalIgnoreCase)) continue;

                if (!AllowedAttributes.Contains(name)) continue;

                if (value == null) { kept.Add(name); continue; }
                value = value.Trim('"', '\'');

                // Reject javascript:/vbscript:/data: in href / src.
                if ((string.Equals(name, "href", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(name, "src", StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.IsNullOrEmpty(value)) continue;
                    if (Regex.IsMatch(value, @"^\s*(javascript|vbscript|data)\s*:", RegexOptions.IgnoreCase)) continue;
                    // Only allow http(s), mailto, relative paths, #anchors.
                    if (Regex.IsMatch(value, @"^\s*(https?:|mailto:|/|#)", RegexOptions.IgnoreCase) ||
                        !value.Contains(":"))
                    {
                        kept.Add(name + "=\"" + EscapeAttr(value) + "\"");
                    }
                    continue;
                }

                kept.Add(name + "=\"" + EscapeAttr(value) + "\"");
            }

            // External links: force rel=noopener noreferrer + target=_blank.
            if (string.Equals(tagName, "a", StringComparison.OrdinalIgnoreCase))
            {
                bool hasTarget = kept.Any(a => a.StartsWith("target=", StringComparison.OrdinalIgnoreCase));
                bool hasRel = kept.Any(a => a.StartsWith("rel=", StringComparison.OrdinalIgnoreCase));
                if (!hasTarget) kept.Add("target=\"_blank\"");
                if (!hasRel) kept.Add("rel=\"noopener noreferrer\"");
            }

            return string.Join(" ", kept);
        }

        private static string EscapeAttr(string v)
        {
            return v.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;");
        }
    }
}
