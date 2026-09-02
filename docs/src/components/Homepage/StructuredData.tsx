import React from 'react';
import { JSX } from 'react';
import Head from '@docusaurus/Head';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';

const description =
  'Mapperly generates fast, readable .NET object mapping code at build time, with no runtime reflection and compile-time diagnostics.';

export default function StructuredData(): JSX.Element {
  const { siteConfig } = useDocusaurusContext();
  const homepageUrl = new URL(siteConfig.baseUrl, siteConfig.url).toString();
  const organizationId = `${homepageUrl}#organization`;
  const softwareId = `${homepageUrl}#software`;
  const websiteId = `${homepageUrl}#website`;

  const structuredData = {
    '@context': 'https://schema.org',
    '@graph': [
      {
        '@type': 'Organization',
        '@id': organizationId,
        name: 'riok GmbH',
        url: 'https://riok.ch/',
        sameAs: ['https://github.com/riok'],
      },
      {
        '@type': 'WebSite',
        '@id': websiteId,
        url: homepageUrl,
        name: 'Mapperly',
        description,
        inLanguage: 'en',
        publisher: { '@id': organizationId },
        about: { '@id': softwareId },
      },
      {
        '@type': 'SoftwareSourceCode',
        '@id': softwareId,
        name: 'Mapperly',
        description,
        url: homepageUrl,
        image: new URL('img/logo.png', homepageUrl).toString(),
        codeRepository: 'https://github.com/riok/mapperly',
        license: 'https://github.com/riok/mapperly/blob/main/LICENSE',
        programmingLanguage: {
          '@type': 'ComputerLanguage',
          name: 'C#',
          url: 'https://learn.microsoft.com/dotnet/csharp',
        },
        runtimePlatform: '.NET',
        isAccessibleForFree: true,
        author: { '@id': organizationId },
        mainEntityOfPage: { '@id': websiteId },
        sameAs: [
          'https://github.com/riok/mapperly',
          'https://www.nuget.org/packages/Riok.Mapperly',
        ],
      },
    ],
  };

  return (
    <Head>
      <script type="application/ld+json">
        {JSON.stringify(structuredData)}
      </script>
    </Head>
  );
}
