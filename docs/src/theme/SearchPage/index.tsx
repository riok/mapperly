import React from 'react';
import { JSX } from 'react';
import Head from '@docusaurus/Head';
import SearchPage from '@theme-original/SearchPage';

export default function SearchPageNoIndex(): JSX.Element {
  return (
    <>
      <Head>
        <meta name="robots" content="noindex, follow" />
      </Head>
      <SearchPage />
    </>
  );
}
