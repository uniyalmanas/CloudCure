const { HTTPParser } = require('http-parser-js');

// Add the constants that http-deceiver/spdy expect
HTTPParser.kOnHeaders = 1;
HTTPParser.kOnHeadersComplete = 2;
HTTPParser.kOnMessageComplete = 3;
HTTPParser.kOnBody = 4;

const methods = HTTPParser.methods || [
  'DELETE', 'GET', 'HEAD', 'POST', 'PUT', 'CONNECT', 'OPTIONS', 'TRACE', 
  'COPY', 'LOCK', 'MKCOL', 'MOVE', 'PROPFIND', 'PROPPATCH', 'SEARCH', 
  'UNLOCK', 'BIND', 'REBIND', 'UNBIND', 'ACL', 'REPORT', 'MKACTIVITY', 
  'CHECKOUT', 'MERGE', 'M-SEARCH', 'NOTIFY', 'SUBSCRIBE', 'UNSUBSCRIBE', 
  'PATCH', 'PURGE', 'MKCALENDAR', 'LINK', 'UNLINK'
];

HTTPParser.methods = methods;

// Polyfill process.binding for http_parser
const originalBinding = process.binding;
process.binding = function(name) {
  if (name === 'http_parser') {
    return {
      HTTPParser: HTTPParser,
      methods: methods
    };
  }
  if (typeof originalBinding === 'function') {
    return originalBinding.apply(this, arguments);
  }
  throw new Error("No process.binding for " + name);
};
