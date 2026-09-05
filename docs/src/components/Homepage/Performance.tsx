import React from 'react';
import { JSX } from 'react';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

export default function Performance(): JSX.Element {
  return (
    <section className={styles.section}>
      <div className="container">
        <div className={styles.sectionHeading}>
          <p className={styles.eyebrow}>Performance by design</p>
          <Heading as={'h2'}>
            Runtime code that looks like you wrote it by hand
          </Heading>
          <p>
            Mapperly resolves mappings at build time and emits direct C#. That
            removes runtime configuration and reflection from the hot path,
            keeps allocations low, and lets the JIT optimize ordinary mapping
            code. The generated source is readable, debuggable, trimming-safe,
            and friendly to Native AOT applications.
          </p>
        </div>
        <div className={styles.benefitGrid}>
          <article className={styles.card}>
            <Heading as={'h3'}>No reflection at runtime</Heading>
            <p>
              Mapping plans are compiled into your assembly instead of being
              discovered or interpreted while the application is running.
            </p>
          </article>
          <article className={styles.card}>
            <Heading as={'h3'}>Useful compiler feedback</Heading>
            <p>
              Strict mappings report unmapped members when you build, helping
              model changes fail early instead of becoming production bugs.
            </p>
          </article>
          <article className={styles.card}>
            <Heading as={'h3'}>Code you can inspect</Heading>
            <p>
              Step into generated methods, review the emitted assignments, and
              keep hand-written methods beside generated mappings when needed.
            </p>
          </article>
        </div>
        <p className={styles.sectionLink}>
          See the published numbers in the{' '}
          <Link to="/docs/intro#performance">
            Mapperly performance benchmark
          </Link>{' '}
          and learn how to{' '}
          <Link to="/docs/configuration/generated-source">
            inspect generated source
          </Link>
          .
        </p>
      </div>
    </section>
  );
}
