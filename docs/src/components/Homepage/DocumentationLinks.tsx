import React from 'react';
import { JSX } from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

export default function DocumentationLinks(): JSX.Element {
  return (
    <section
      className={clsx(styles.section, styles.sectionMuted, styles.finalSection)}
    >
      <div className="container">
        <div className={styles.sectionHeading}>
          <p className={styles.eyebrow}>Documentation</p>
          <Heading as={'h2'}>Build your first production-ready mapper</Heading>
          <p>
            Install Mapperly, create a mapping, then use the focused guides for
            configuration, diagnostics, and generated code.
          </p>
        </div>
        <div className={styles.docLinks}>
          <Link
            className="button button--primary button--lg"
            to="/docs/getting-started/installation"
          >
            Installation guide
          </Link>
          <Link
            className="button button--outline button--primary button--lg"
            to="/docs/category/usage-and-configuration"
          >
            Explore configuration
          </Link>
          <Link
            className="button button--outline button--primary button--lg"
            to="/docs/configuration/analyzer-diagnostics"
          >
            Diagnostic reference
          </Link>
        </div>
      </div>
    </section>
  );
}
