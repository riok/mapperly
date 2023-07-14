// @ts-check

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

async function createConfig() {
  const rehypeFaq = (await import('./src/plugins/rehype/rehype-faq/index.js'))
    .default;

  /** @type {import('@docusaurus/types').Config} */
  return {
    title: 'Mapperly',
    tagline:
      'A .NET source generator for generating object mappings. No runtime reflection. Inspired by MapStruct.',
    url: process.env.DOCUSAURUS_URL || 'https://mapperly.riok.app',
    baseUrl: process.env.DOCUSAURUS_BASE_URL || '/',
    trailingSlash: true,
    onBrokenLinks: 'throw',
    onBrokenMarkdownLinks: 'throw',
    favicon: 'img/logo.svg',
    organizationName: 'riok',
    projectName: 'mapperly',
    i18n: {
      defaultLocale: 'en',
      locales: ['en'],
    },
    presets: [
      [
        'classic',
        /** @type {import('@docusaurus/preset-classic').Options} */
        ({
          docs: {
            sidebarPath: require.resolve('./sidebars.js'),
            rehypePlugins: [rehypeFaq],
          },
          theme: {
            customCss: require.resolve('./src/css/custom.css'),
          },
        }),
      ],
    ],

    themeConfig:
      /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
      ({
        metadata: [
          {
            name: 'keywords',
            content: '.NET, SourceGenerator, Mapping, Roslyn, dotnet',
          },
        ],
        colorMode: {
          disableSwitch: true,
        },
        navbar: {
          title: 'Mapperly',
          logo: {
            alt: 'Mapperly Logo',
            src: 'img/logo.svg',
          },
          items: [
            {
              type: 'doc',
              docId: 'intro',
              position: 'left',
              label: 'Documentation',
              sidebarId: 'docs',
            },
            {
              type: 'doc',
              docId: '/category/api',
              position: 'left',
              label: 'API',
              sidebarId: 'api',
            },
            {
              type: 'doc',
              docId: 'contributing/index',
              position: 'left',
              label: 'Contributing',
              sidebarId: 'contributing',
            },
            {
              href: 'https://github.com/riok/mapperly',
              className: 'headerGithubLink',
              'aria-label': 'GitHub repository',
              position: 'right',
            },
          ],
        },
        footer: {
          style: 'dark',
          logo: {
            alt: 'riok Logo',
            src: '/img/riok-logo-grayscale.svg',
            href: 'https://riok.ch',
            width: '180px',
          },
          copyright:
            'Mapperly is an open source project of <a href="https://riok.ch">riok</a>.',
          links: [
            {
              title: 'Docs',
              items: [
                {
                  label: 'Introduction',
                  to: '/docs/intro',
                },
                {
                  label: 'Installation',
                  to: '/docs/getting-started/installation',
                },
                {
                  label: 'Configuration',
                  to: '/docs/category/usage-and-configuration',
                },
              ],
            },
            {
              title: 'Community',
              items: [
                {
                  label: 'Q&A',
                  href: 'https://github.com/riok/mapperly/discussions',
                },
                {
                  label: 'Open an issue',
                  href: 'https://github.com/riok/mapperly/issues/new/choose',
                },
                {
                  label: 'Contributing',
                  to: '/docs/contributing',
                },
              ],
            },
            {
              title: 'More',
              items: [
                {
                  label: 'GitHub Repository',
                  href: 'https://github.com/riok/mapperly',
                },
                {
                  label: 'NuGet',
                  href: 'https://www.nuget.org/packages/Riok.Mapperly',
                },
                {
                  label: 'Releases',
                  href: 'https://github.com/riok/mapperly/releases',
                },
              ],
            },
            {
              title: 'Legal',
              items: [
                {
                  label: 'License',
                  href: 'https://github.com/riok/mapperly/blob/main/LICENSE',
                },
              ],
            },
          ],
        },
        prism: {
          theme: lightCodeTheme,
          darkTheme: darkCodeTheme,
          additionalLanguages: ['csharp', 'powershell', 'editorconfig'],
        },
      }),
    plugins: [
      [
        '@docusaurus/plugin-ideal-image',
        /** @type {import('@docusaurus/plugin-ideal-image').PluginOptions} */
        ({
          max: 1600,
          min: 400,
          // Use false to debug, but it incurs huge perf costs
          disableInDev: true,
        }),
      ],
      '@easyops-cn/docusaurus-search-local',
    ],
    customFields: {
      mapperlyVersion: process.env.MAPPERLY_VERSION || '0.0.1-dev',
      environment: process.env.ENVIRONMENT,
    },
  };
}

module.exports = createConfig;
