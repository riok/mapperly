---
sidebar_position: 3
description: Frequently asked questions and answers.
title: FAQ
---

<!-- if updated, make sure the comment in plugins/rehype/rehype-faq/index.js is considered  -->

# Frequently asked questions and answers {#faq}

Here you can find answers to frequently asked questions and common problems about Mapperly.

## Mapperly does not work when I use source generator X.

Chaining source generators is not supported by Roslyn.

## I updated the Mapperly version, but the generated code still looks the same.

Restart the IDE to make it load the new version of Mapperly. This is a bug of the IDE.

## Everything is configured correctly and dotnet build works, but the IDE shows the error "[Mapper method] must have an implementation part because it has accessibility modifiers"

Make sure your project meets the [requirements](../intro.md#requirements).
Try rebuilding the solution or restarting the IDE. This is a bug in the IDE.

## My advanced use case isn't supported by Mapperly or needs lots of configuration. What should I do?

Write the mapping for that class manually. You can mix automatically generated mappings and [user implemented mappings](../configuration/user-implemented-methods.mdx) without problems.

## My code throws `FileNotFoundException` with `Riok.Mapperly.Abstractions`. What should I do?

Are you using [reference handling](../configuration/reference-handling.md)
or have you enabled the [preservation of Mapperly attributes at runtime](installation.mdx#preserving-the-attributes-at-runtime)?
Make sure `ExcludeAssets` on the `PackageReference` does not include `runtime` as these features require runtime assets.
