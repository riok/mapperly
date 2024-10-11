import ComponentTypes from '@theme-original/NavbarItem/ComponentTypes';
import VersionsNavbarItem from '@site/src/components/NavbarItems/VersionsNavbarItem';
import CoffeeNavbarItem from '@site/src/components/NavbarItems/CoffeeNavbarItem';

// see https://github.com/facebook/docusaurus/issues/7227
export default {
  ...ComponentTypes,
  'custom-coffeeNavbarItem': CoffeeNavbarItem,
  'custom-versionsNavbarItem': VersionsNavbarItem,
};
