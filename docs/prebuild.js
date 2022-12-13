const {exec} = require('child_process');
const {readFile, writeFile, mkdir, rmdir, readdir, rm} = require('fs').promises;
const {join} = require('path');
const util = require('util');
const {marked} = require('marked');
const execPromise = util.promisify(exec);

const generatedDataDir = './src/data/generated';

async function clearGeneratedFiles() {
  try {
    await rmdir(generatedDataDir);
  } catch {}

  await mkdir(generatedDataDir, { recursive: true });
}

async function deleteFilesWithExtension(dir, extension) {
  const fileNames = await readdir(dir);
  for (const fileName of fileNames) {
    if (fileName.endsWith(extension)) {
      await rm(join(dir, fileName));
    }
  }
}

async function buildApiDocs() {
  const targetDir = './docs/99-api';
  const dll = '../src/Riok.Mapperly.Abstractions/bin/Debug/netstandard2.0/Riok.Mapperly.Abstractions.dll';

  // clean target directory
  await deleteFilesWithExtension(targetDir, '.md');

  // use xmldoc2md to convert the dotnet xml documentation to markdown
  await execPromise('dotnet tool restore');
  await execPromise(`dotnet xmldoc2md ${dll} ${targetDir}`);

  // we instead use the docusaurus generated index
  await rm(join(targetDir, 'index.md'));

  const fileNames = await readdir(targetDir);
  for (const fileName of fileNames) {
    const filePath = join(targetDir, fileName);

    let content = await readFile(filePath, 'utf-8');

    // this replacement is required due to jsx limitations
    content = content.replace(/<br>/g, '<br />');
    await writeFile(filePath, content);
  }
}

async function buildAnalyzerRulesData() {
  // extract analyzer rules from AnalyzerReleases.Shipped.md and write to a json file
  const targetFile = join(generatedDataDir, 'analyzer-rules.json');
  const sourceFile = '../src/Riok.Mapperly/AnalyzerReleases.Shipped.md';

  let rules = [];
  const walkTokens = token => {
    if (token.type !== 'table') {
      return token;
    }

    for (const row of token.rows) {
      rules.push({
        id: row[0].text,
        category: row[1].text,
        severity: row[2].text,
        notes: row[3].text,
      });
    }

    return token;
  };
  marked.use({walkTokens});

  const analyzersMd = await readFile(sourceFile);
  marked.parse(analyzersMd.toString());
  await writeFile(targetFile, JSON.stringify(rules, undefined,  '  '));
}

(async () => {
  await clearGeneratedFiles();
  await buildApiDocs();
  await buildAnalyzerRulesData();
})();
