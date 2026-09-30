import React from 'react';
import { JSX } from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

const scenarios = [
  {
    title: 'APIs and application layers',
    description:
      'Map domain models, commands, DTOs, and response types while checking that required members are handled.',
    to: '/docs/getting-started/best-practices',
    link: 'Mapping best practices',
  },
  {
    title: 'Collections and built-in conversions',
    description:
      'Convert objects, enumerables, dictionaries, strings, enums, and common framework types.',
    to: '/docs/configuration/conversions',
    link: 'Supported conversions',
  },
  {
    title: 'Database projections',
    description:
      'Generate IQueryable projections that data providers can translate without materializing full source objects first.',
    to: '/docs/configuration/queryable-projections',
    link: 'Queryable projections',
  },
  {
    title: 'Immutable models and records',
    description:
      'Populate constructor parameters and init-only members when mapping to immutable target types and C# records.',
    to: '/docs/configuration/ctor-mappings',
    link: 'Constructor mappings',
  },
  {
    title: 'Flattened and nested models',
    description:
      'Flatten nested members by convention or configure exact property paths for models with different shapes.',
    to: '/docs/configuration/flattening',
    link: 'Flattening and unflattening',
  },
  {
    title: 'Custom and incremental updates',
    description:
      'Update existing objects, reuse hand-written methods, and keep specialized business transformations explicit.',
    to: '/docs/configuration/existing-target',
    link: 'Existing-target mappings',
  },
];

export default function SupportedScenarios(): JSX.Element {
  return (
    <section className={clsx(styles.section, styles.sectionMuted)}>
      <div className="container">
        <div className={styles.sectionHeading}>
          <p className={styles.eyebrow}>Supported scenarios</p>
          <Heading as={'h2'}>
            From straightforward DTOs to real application models
          </Heading>
          <p>
            Start with convention-based property mapping, then configure only
            the cases where your source and target models intentionally differ.
          </p>
        </div>
        <div className={styles.scenarioGrid}>
          {scenarios.map((scenario) => (
            <article className={styles.card} key={scenario.title}>
              <Heading as={'h3'}>{scenario.title}</Heading>
              <p>{scenario.description}</p>
              <Link to={scenario.to}>{scenario.link} →</Link>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
