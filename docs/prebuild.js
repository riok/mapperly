const {exec} = require('child_process');
const {readFileSync, writeFileSync, readdirSync, rmSync} = require('fs');
const {join} = require('path');
const util = require("util");
const execPromise = util.promisify(exec);

async function buildApiDocs() {
  const targetDir = './docs/99-api';
  const dll = '../src/Riok.Mapperly.Abstractions/bin/Debug/netstandard2.0/Riok.Mapperly.Abstractions.dll';

  // use xmldoc2md to convert the dotnet xml documentation to markdown
  await execPromise('dotnet tool restore');
  await execPromise(`dotnet xmldoc2md ${dll} ${targetDir}`);

  // we instead use the docusaurus generated index
  rmSync(join(targetDir, 'index.md'));

  readdirSync(targetDir).forEach(fileName => {
    const filePath = join(targetDir, fileName);

    let content = readFileSync(filePath, 'utf-8');

    // this replacement is required due to jsx limitations
    content = content.replace(/<br>/g, '<br />');
    writeFileSync(filePath, content);
  });
}

(async () => {
  await buildApiDocs();
})();
