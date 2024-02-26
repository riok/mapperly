const { exec } = require('child_process');
const { readFile, writeFile, copyFile, mkdir, readdir, rm } =
  require('fs').promises;
const { existsSync } = require('fs');
const { join } = require('path');
const util = require('util');
const { marked } = require('marked');
const execPromise = util.promisify(exec);

const generatedDataDir = './src/data/generated';

async function emptyDirectory(dir: string): Promise<void> {
  try {
    await rm(dir, { recursive: true });
  } catch {}

  await mkdir(dir, { recursive: true });
}

async function buildApiDocs(): Promise<void> {
  const targetDir = './docs/api';
  const dll =
    '../artifacts/bin/Riok.Mapperly.Abstractions/debug/Riok.Mapperly.Abstractions.dll';

  // clean target directory
  await emptyDirectory(targetDir);

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

async function buildAnalyzerRulesData(): Promise<void> {
  // extract analyzer rules from AnalyzerReleases.Shipped.md and write to a json file
  const targetFile = join(generatedDataDir, 'analyzer-rules.json');
  const sourceFile = '../src/Riok.Mapperly/AnalyzerReleases.Shipped.md';
  const analyzerDiagnosticsDocsDir =
    './docs/configuration/analyzer-diagnostics';

  let rules = {};
  let removingRules = true;
  const walkTokens = (token) => {
    if (token.type === 'heading' && token.depth === 3) {
      removingRules = token.text === 'Removed Rules';
      return token;
    }

    if (token.type !== 'table') {
      return token;
    }

    for (const row of token.rows) {
      const id = row[0].text;
      if (removingRules) {
        delete rules[id];
        continue;
      }

      rules[id] = {
        id,
        category: row[1].text,
        severity: row[2].text,
        notes: row[3].text,
        hasDocumentation: existsSync(
          join(analyzerDiagnosticsDocsDir, `${id}.mdx`),
        ),
      };
    }

    return token;
  };
  marked.use({ walkTokens });

  const analyzersMd = await readFile(sourceFile);
  marked.parse(analyzersMd.toString());
  await writeFile(
    targetFile,
    JSON.stringify(Object.values(rules), undefined, '  '),
  );
}

async function buildSamples(): Promise<void> {
  const targetDir = join(generatedDataDir, 'samples');
  await mkdir(targetDir);

  // Copy generated mapper to target dir
  const generatedMapperFile =
    '../artifacts/obj/Riok.Mapperly.Sample/debug/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/CarMapper.g.cs';
  await copyFile(generatedMapperFile, join(targetDir, 'CarMapper.g.cs'));

  // Copy sample project files to target dir
  const sampleProject = '../samples/Riok.Mapperly.Sample';
  const projectFilesToCopy = ['CarMapper.cs', 'Car.cs', 'CarDto.cs'];
  for (const file of projectFilesToCopy) {
    await copyFile(join(sampleProject, file), join(targetDir, file));
  }
}

async function buildRobotsTxt(): Promise<void> {
  const targetFile = 'static/robots.txt';
  const content =
    process.env.ENVIRONMENT === 'next'
      ? 'User-agent: *\nDisallow: /\n'
      : 'User-agent: *\n';
  await writeFile(targetFile, content);
}

(async () => {
  await emptyDirectory(generatedDataDir);
  await buildApiDocs();
  await buildAnalyzerRulesData();
  await buildSamples();
  await buildRobotsTxt();
})();
