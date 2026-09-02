import React from 'react';
import { JSX } from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

export default function HomepageHeader(): JSX.Element {
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Heading as={'h1'} className="hero__title">
          Fast, compile-time object mapping for .NET
        </Heading>
        <p className={clsx('hero__subtitle', styles.heroSubtitle)}>
          Mapperly generates readable C# mapping code at build time. Get the
          convenience of an object mapper with no runtime reflection, no runtime
          dependency, and compile-time diagnostics when models drift.
        </p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/getting-started/installation"
          >
            Install Mapperly
          </Link>
          <Link
            className={clsx('button button--lg', styles.secondaryButton)}
            to="/docs/intro"
          >
            Read the introduction
          </Link>
        </div>
      </div>
    </header>
  );
}
