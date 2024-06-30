---
sidebar_position: 6
description: How Mapperly versions are released and deployed to NuGet
---

# Release process

Every time a push to the main branch occurs,
a GitHub actions workflow creates or updates
the upcoming next and stable GitHub release drafts by using [release-drafter](https://github.com/release-drafter/release-drafter).
release-drafter generates a changelog based on the PR titles and commit-messages.
The upcoming version is determined by looking at the labels of each PR.
Each merged PR needs to have at least one of the following labels:

- `no-changelog` or `dependencies` (ignored)
- `breaking-change` (major)
- `enhancement` (minor)
- `bug` (patch)

To build and publish a Mapperly version,
publish the GitHub draft release.
The release notes can be modified before the publication as needed.

:::note
If the release notes are changed and another commit is pushed to `main'
after the changes have been saved,
the changes will be lost.
:::

When the release is published,
GitHub will create a tag pointing to the latest commit of `main`.
A GitHub action will export the release notes,
strip the Markdown (unfortunately NuGet release notes don't support Markdown),
build the Mapperly package,
deploy it on NuGet
and attach the built NuGet package to the GitHub release.

After the NuGet package is successfully built and deployed,
the documentation will be built and deployed.

To release a version from another commit than the `HEAD` of `main`,
manually create and publish a GitHub release targeting the desired commit.
