import DOMPurify from 'dompurify';

/**
 * SafeHtml
 *   Sanitize HTML before dangerouslySetInnerHTML to neutralise XSS even if
 *   the backend sanitizer (HtmlSanitizer.cs) misses something or a stale row
 *   predates the sanitizer. We use the same allowlist philosophy: strip
 *   <script>, <iframe>, event handlers, javascript: URIs. Keeps the standard
 *   formatting tags.
 */
export function sanitizeHtml(html) {
  if (!html) return '';
  return DOMPurify.sanitize(String(html), {
    USE_PROFILES: { html: true },
    ALLOWED_TAGS: [
      'p', 'br', 'strong', 'b', 'em', 'i', 'u', 's',
      'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
      'ul', 'ol', 'li',
      'blockquote', 'pre', 'code',
      'a', 'img',
      'table', 'thead', 'tbody', 'tr', 'th', 'td',
      'hr', 'span', 'div',
    ],
    ALLOWED_ATTR: ['href', 'title', 'alt', 'src', 'target', 'rel'],
    ALLOW_DATA_ATTR: false,
  });
}

/**
 * safeDangerouslySetInnerHTML — convenience: returns the prop object that
 * React expects for dangerouslySetInnerHTML after sanitization.
 *
 *     <div {...safeDangerouslySetInnerHTML(cms.content)} />
 */
export function safeDangerouslySetInnerHTML(html) {
  return { __html: sanitizeHtml(html) };
}
