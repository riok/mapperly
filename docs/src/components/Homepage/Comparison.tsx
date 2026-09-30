import React from 'react';
import { JSX } from 'react';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';

import styles from './styles.module.css';

export default function Comparison(): JSX.Element {
  return (
    <section className={styles.section}>
      <div className="container">
        <div className={styles.sectionHeading}>
          <p className={styles.eyebrow}>Choose the right mapping approach</p>
          <Heading as={'h2'}>
            Mapperly compared with common alternatives
          </Heading>
          <p>
            Mapperly is a strong fit when you want generated, inspectable code
            and compile-time safety without maintaining every assignment by
            hand.
          </p>
        </div>
        <div className={styles.tableWrapper}>
          <table className={styles.comparisonTable}>
            <thead>
              <tr>
                <th scope="col">Approach</th>
                <th scope="col">Configuration time</th>
                <th scope="col">Runtime reflection</th>
                <th scope="col">Generated code</th>
                <th scope="col">Model-drift diagnostics</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <th scope="row">Mapperly</th>
                <td>Build time</td>
                <td>No</td>
                <td>Readable C#</td>
                <td>Built in</td>
              </tr>
              <tr>
                <th scope="row">Hand-written mapping</th>
                <td>Development time</td>
                <td>No</td>
                <td>Not applicable</td>
                <td>Compiler checks types, not mapping completeness</td>
              </tr>
              <tr>
                <th scope="row">Runtime-configured mappers like AutoMapper</th>
                <td>Application startup or runtime</td>
                <td>Depends on the library and configuration</td>
                <td>Usually not available as source</td>
                <td>Often deferred until startup or execution</td>
              </tr>
            </tbody>
          </table>
        </div>
        <p className={styles.sectionLink}>
          Mapperly stays flexible: combine generated mappings with{' '}
          <Link to="/docs/configuration/user-implemented-methods">
            user-implemented methods
          </Link>{' '}
          and configure{' '}
          <Link to="/docs/configuration/mapper">mapper defaults</Link> only
          where conventions are not enough.
        </p>
      </div>
    </section>
  );
}
