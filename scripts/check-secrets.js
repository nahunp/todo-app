#!/usr/bin/env node
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

function getStagedFiles() {
  try {
    const out = execSync('git diff --cached --name-only --diff-filter=ACM', { encoding: 'utf8' });
    return out.split(/\r?\n/).map(s => s.trim()).filter(Boolean);
  } catch (e) {
    console.error('Failed to get staged files:', e.message);
    process.exit(0) // don't block commit on hook failures
  }
}

const files = getStagedFiles();
if (!files.length) process.exit(0);

const patterns = [
  /api[_-]?key/i,
  /secret/i,
  /access[_-]?token/i,
  /auth[_-]?token/i,
  /bearer\s+[A-Za-z0-9\-_\.]+/i,
  /password/i,
  /passwd/i,
  /client[_-]?secret/i,
  /-----BEGIN (RSA|OPENSSH|EC|PRIVATE) KEY-----/i,
  /ghp_[A-Za-z0-9]+/i,
  /github[_-]?token/i,
  /AKIA[0-9A-Z]{16}/i
];

const allowedBinaryExt = new Set(['.png','.jpg','.jpeg','.gif','.bmp','.ico','.zip','.gz','.tgz','.7z','.exe','.dll','.so','.bin']);

let found = [];

for (const file of files) {
  // skip our own hook files so the hook doesn't flag itself
  if (file.startsWith('.githooks') || file === 'scripts/check-secrets.js') continue;
  if (file.includes('node_modules') || file.includes('/dist/') || file.includes('dist\\')) continue;
  const ext = path.extname(file).toLowerCase();
  if (allowedBinaryExt.has(ext)) continue;
  let full = file;
  if (!fs.existsSync(full)) continue;
  try {
    const content = fs.readFileSync(full, 'utf8');
    for (const re of patterns) {
      const m = content.match(re);
      if (m) {
        const snippet = m[0].replace(/\r?\n/g, ' ');
        found.push({ file, pattern: re.toString(), snippet });
      }
    }
  } catch (e) {
  }
}

if (found.length) {
  console.error('\nERROR: Potential secrets detected in staged files. Commit aborted.');
  for (const f of found) {
    console.error(`- ${f.file}: matched ${f.pattern} -> "${f.snippet}"`);
  }
  console.error('\nIf these are false positives, remove the sensitive text from the staged changes or skip the hook temporarily with git commit --no-verify.');
  process.exit(1);
}

process.exit(0);
