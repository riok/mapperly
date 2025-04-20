import React from 'react';
import { JSX } from 'react';
import DropdownNavbarItem from '@theme/NavbarItem/DropdownNavbarItem';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './styles.module.css';
import type { LinkLikeNavbarItemProps } from '@theme/NavbarItem';
import { CustomFields } from '@site/src/custom-fields';

export default function VersionsNavbarItem(): JSX.Element {
  const { environment, mapperlyVersion } = useDocusaurusContext().siteConfig
    .customFields as unknown as CustomFields;

  const items: LinkLikeNavbarItemProps[] = [
    {
      label: 'stable',
      to: environment.stable ? '#' : 'https://mapperly.riok.app',
      isActive: () => environment.stable,
    },
    {
      label: 'next',
      to: environment.next ? '#' : 'https://next.mapperly.riok.app',
      isActive: () => environment.next,
    },
  ];

  if (environment.local) {
    items.push({
      label: 'local',
      to: '#',
      isActive: () => true,
    });
  }

  return (
    <DropdownNavbarItem
      label={<>{mapperlyVersion}</>}
      items={items}
      className={environment.stable ? undefined : styles.versionAlert}
    />
  );
}
