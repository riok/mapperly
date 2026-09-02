import React from 'react';
import { JSX } from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import CodeBlock from '@theme/CodeBlock';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

export default function QuickStart(): JSX.Element {
  return (
    <section className={clsx(styles.section, styles.sectionMuted)}>
      <div className="container">
        <div className={styles.sectionHeading}>
          <p className={styles.eyebrow}>Quick start</p>
          <Heading as={'h2'}>
            Define the mapping. Mapperly writes the code.
          </Heading>
          <p>
            Add the NuGet package, mark a partial class with{' '}
            <code>[Mapper]</code>, and declare the mapping method you want.
            Mapperly generates its implementation during compilation.
          </p>
        </div>
        <div className={styles.codeGrid}>
          <div>
            <Heading as={'h3'}>1. Install the source generator</Heading>
            <CodeBlock className={styles.codeBlock} language="bash">
              dotnet add package Riok.Mapperly
            </CodeBlock>
          </div>
          <div>
            <Heading as={'h3'}>2. Declare and use a mapper</Heading>
            <CodeBlock className={styles.codeBlock} language="csharp">{`[Mapper]
public static partial class CarMapper
{
    public static partial CarDto Map(Car source);
}

var dto = CarMapper.Map(car);`}</CodeBlock>
          </div>
        </div>
        <p className={styles.sectionLink}>
          Continue with{' '}
          <Link to="/docs/getting-started/first-mapper">
            your first Mapperly mapper
          </Link>{' '}
          or inspect a{' '}
          <Link to="/docs/getting-started/generated-mapper-example">
            complete generated mapper example
          </Link>
          .
        </p>
      </div>
    </section>
  );
}
