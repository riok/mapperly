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

Try to rebuild the solution or restart the IDE. This is a bug of the IDE.

## My advanced use case isn't supported by Mapperly or needs lots of configuration. What should I do?

Write the mapping for that class manually. You can mix automatically generated mappings and [user implemented mappings](../configuration/user-implemented-methods.mdx) without problems.
