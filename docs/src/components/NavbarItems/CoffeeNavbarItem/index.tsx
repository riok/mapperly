import React from 'react';
import { JSX } from 'react';
import DefaultNavbarItem from '@theme/NavbarItem/DefaultNavbarItem';

export default function CoffeeNavbarItem(): JSX.Element {
  return (
    <DefaultNavbarItem
      label="☕ Buy us a coffee!"
      href="https://github.com/sponsors/riok"
    />
  );
}
