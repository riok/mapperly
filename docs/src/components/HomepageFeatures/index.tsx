import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Easy to Use Object-Object Mapper',
    Svg: require('@site/static/img/easy-to-use.svg').default,
    description: (
      <>
        Mapperly is a .NET Source Generator that simplifies the implementation
        of object to object mappings. One only needs to define the mapping
        methods signature. The implementation is provided by Mapperly.
      </>
    ),
  },
  {
    title: 'Fast & Readable',
    Svg: require('@site/static/img/fast-reliable.svg').default,
    description: (
      <>
        Because Mapperly creates the mapping code at build time, there is
        minimal overhead at runtime. Even better, the generated code is
        perfectly readable, allowing you to verify the generated mapping code
        easily.
      </>
    ),
  },
  {
    title: 'Powered by source generators',
    Svg: require('@site/static/img/powered-by.svg').default,
    description: (
      <>
        Mapperly works by using .NET Source Generators. Since no reflection is
        used at runtime, the generated code is completely trimming safe and AOT
        friendly.
      </>
    ),
  },
];

function Feature({ title, Svg, description }: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
