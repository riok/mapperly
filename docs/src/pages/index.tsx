import React from 'react';
import { JSX } from 'react';
import Layout from '@theme/Layout';
import Comparison from '@site/src/components/Homepage/Comparison';
import DocumentationLinks from '@site/src/components/Homepage/DocumentationLinks';
import HomepageHeader from '@site/src/components/Homepage/HomepageHeader';
import Performance from '@site/src/components/Homepage/Performance';
import QuickStart from '@site/src/components/Homepage/QuickStart';
import StructuredData from '@site/src/components/Homepage/StructuredData';
import SupportedScenarios from '@site/src/components/Homepage/SupportedScenarios';
import HomepageFeatures from '@site/src/components/HomepageFeatures';

export default function Home(): JSX.Element {
  return (
    <Layout
      title="Source-Generated .NET Object Mapping"
      description="Mapperly generates fast, readable .NET object mapping code at build time, with no runtime reflection and compile-time diagnostics."
    >
      <StructuredData />
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <QuickStart />
        <Performance />
        <SupportedScenarios />
        <Comparison />
        <DocumentationLinks />
      </main>
    </Layout>
  );
}
