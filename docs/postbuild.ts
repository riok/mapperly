const { createHash } = require('crypto');
const fsPromises = require('fs').promises;
const path = require('path');

const buildDir = './build';
const headersFile = path.join(buildDir, '_headers');
const externalScriptSources = [
  'https://*.cloudflareinsights.com',
  'https://cloudflareinsights.com',
];
const cspHeaderName = 'Content-Security-Policy';

function createSha256Hash(content: string): string {
  return `'sha256-${createHash('sha256').update(content, 'utf8').digest('base64')}'`;
}

async function findHtmlFiles(dir: string): Promise<string[]> {
  const entries = await fsPromises.readdir(dir, { withFileTypes: true });
  const files = await Promise.all(
    entries.map(async (entry) => {
      const entryPath = path.join(dir, entry.name);

      if (entry.isDirectory()) {
        return await findHtmlFiles(entryPath);
      }

      return entry.name.endsWith('.html') ? [entryPath] : [];
    }),
  );

  return files.flat();
}

function hashInlineScripts(html: string): string[] {
  const scriptRegex = /<script\b([^>]*)>([\s\S]*?)<\/script>/gi;
  const hashes = new Set<string>();
  let match: RegExpExecArray | null;

  while ((match = scriptRegex.exec(html)) !== null) {
    const [, attributes, body] = match;

    if (/\bsrc\s*=/.test(attributes) || body.length === 0) {
      continue;
    }

    hashes.add(createSha256Hash(body));
  }

  return [...hashes];
}

function hashInlineStyleAttributes(html: string): string[] {
  const styleRegex = /\sstyle=(['"])([\s\S]*?)\1/gi;
  const hashes = new Set<string>();
  let match: RegExpExecArray | null;

  while ((match = styleRegex.exec(html)) !== null) {
    hashes.add(createSha256Hash(match[2]));
  }

  return [...hashes];
}

function createCsp(
  scriptHashes: string[],
  styleAttributeHashes: string[],
): string {
  const scriptSrc = [
    'script-src',
    "'self'",
    ...scriptHashes.sort(),
    ...externalScriptSources,
  ].join(' ');

  const styleAttributeSrc = [
    'style-src-attr',
    "'unsafe-hashes'",
    ...styleAttributeHashes.sort(),
  ].join(' ');

  return [
    'upgrade-insecure-requests',
    'block-all-mixed-content',
    "default-src 'self'",
    scriptSrc,
    "script-src-attr 'none'",
    "style-src 'self' 'unsafe-hashes'",
    "style-src-elem 'self'",
    styleAttributeSrc,
    "img-src 'self' data: https://img.shields.io/",
    "object-src 'none'",
    "connect-src 'self' https://*.cloudflareinsights.com https://cloudflareinsights.com",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
  ].join('; ');
}

function insertCspHeader(headers: string, csp: string): string {
  const cspHeaderRegex = new RegExp(`^  ${cspHeaderName}: .+$`, 'm');
  const cspHeader = `  ${cspHeaderName}: ${csp}`;

  if (cspHeaderRegex.test(headers)) {
    return headers.replace(cspHeaderRegex, cspHeader);
  }

  const referrerPolicyHeader = /^  Referrer-Policy: .+$/m;

  if (!referrerPolicyHeader.test(headers)) {
    throw new Error(`Could not find insertion point for ${cspHeaderName}`);
  }

  return headers.replace(
    referrerPolicyHeader,
    (match) => `${match}\n${cspHeader}`,
  );
}

async function updateHeaders(): Promise<void> {
  const headers = await fsPromises.readFile(headersFile, 'utf-8');

  const scriptHashes = new Set<string>();
  const styleAttributeHashes = new Set<string>();
  const htmlFiles = await findHtmlFiles(buildDir);

  for (const file of htmlFiles) {
    const html = await fsPromises.readFile(file, 'utf-8');

    for (const hash of hashInlineScripts(html)) {
      scriptHashes.add(hash);
    }

    for (const hash of hashInlineStyleAttributes(html)) {
      styleAttributeHashes.add(hash);
    }
  }

  const csp = createCsp([...scriptHashes], [...styleAttributeHashes]);

  if (/\b(?:script-src|style-src)\b[^;]*'unsafe-inline'/.test(csp)) {
    throw new Error('Generated CSP unexpectedly allows unsafe-inline');
  }

  await fsPromises.writeFile(headersFile, insertCspHeader(headers, csp));
}

updateHeaders();
