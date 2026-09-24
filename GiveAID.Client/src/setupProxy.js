// Custom proxy setup for development.
// CRA reads `src/setupProxy.js` and uses http-proxy-middleware to forward
// API requests to the .NET WebApi.
//
// We bind to 127.0.0.1 (not `localhost`) to avoid IPv6/IPv4 ambiguity on
// Windows where localhost sometimes resolves to ::1 first.
// Target is configurable via REACT_APP_PROXY_TARGET env var.

const { createProxyMiddleware } = require('http-proxy-middleware');

const proxyTarget = process.env.REACT_APP_PROXY_TARGET || 'http://127.0.0.1:5231';

module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: proxyTarget,
      changeOrigin: true,
      ws: true,
      logLevel: 'warn'
    })
  );
};
