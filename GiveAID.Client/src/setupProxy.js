// Custom proxy setup for development.
// CRA reads `src/setupProxy.js` and uses http-proxy-middleware to forward
// API requests to the .NET WebApi running on http://127.0.0.1:5000.
//
// We bind to 127.0.0.1 (not `localhost`) to avoid IPv6/IPv4 ambiguity on
// Windows where localhost sometimes resolves to ::1 first.

const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'http://127.0.0.1:5000',
      changeOrigin: true,
      ws: true,
      logLevel: 'warn'
    })
  );
};
