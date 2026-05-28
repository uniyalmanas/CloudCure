// Monkeypatch for retired internal 'http_parser' in modern Node.js versions
try {
  const { HTTPParser } = require('http-parser-js');
  
  // Save original process.binding
  const originalBinding = process.binding;
  
  process.binding = function(name) {
    if (name === 'http_parser') {
      return { HTTPParser };
    }
    if (originalBinding) {
      return originalBinding.apply(this, arguments);
    }
    throw new Error('process.binding is not defined and name is not http_parser');
  };
  
  console.log('Successfully monkeypatched process.binding("http_parser") for modern Node.js compatibility.');
} catch (err) {
  console.warn('Failed to monkeypatch http_parser:', err.message);
}
