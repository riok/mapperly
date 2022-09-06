# Mapperly Documentation

The mapperly documentation is built using [Docusaurus 2](https://docusaurus.io/).

### Installation

```bash
npm i
```

### Generate API Documentation

To build the generated api documentation (located in `docs/99-api`)
ensure the dotnet solution is built or run `dontet build` in the solutions root directory.
Then run the prebuild script:

```bash
npm run prebuild
```

### Local Development

```bash
npm run start
```

This command starts a local development server and opens up a browser window. Most changes are reflected live without having to restart the server.

### Build

```bash
npm run build
```

This command generates static content into the `build` directory and can be served using any static contents hosting service or locally using `npm run serve`.
