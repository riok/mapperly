---
sidebar_position: 0
description: A guide on how to contribute to Mapperly.
---

# Contributing

We would love for you to contribute to Mapperly and help make it even better than it is today!
As a contributor, here are the guidelines we would like you to follow.

## Code of Conduct

Help us keep OpenThread open and inclusive. Please read and follow our [code of conduct](https://github.com/riok/mapperly/blob/main/CODE_OF_CONDUCT.md).

## Got a question or problem?

If you have a question or a problem create a new [GitHub Discussion](https://github.com/riok/mapperly/discussions/new/choose).

## Found a Bug?

If you find a bug in the source code, you can help us by [submitting a GitHub Issue](https://github.com/riok/mapperly/issues/new).
Even better, you can [submit a Pull Request](#submit-pr) with a fix.

## Missing a Feature?

You can _request_ a new feature by [submitting an issue](#submit-issue) to our GitHub repository.
If you would like to _implement_ a new feature, please consider the size of the change in order to determine the right steps to proceed:

- For a **major feature**, first [open an issue](https://github.com/riok/mapperly/issues/new) and outline your proposal so that it can be discussed.
  Getting early feedback will help ensure your implementation work is accepted by the maintainers.
  This will also allow us to better coordinate our efforts and minimize duplicated effort.

  Note: Adding a new topic to the documentation, or significantly re-writing a topic, counts as a major feature.

- **Small Features** can be crafted and directly [submitted as a Pull Request](#submit-pr).

## Submission Guidelines

### <a name="submit-issue"></a> Submitting an issue

Before you submit an issue, please search the issue tracker and discussions.
An issue for your problem might already exist and the discussion might inform you of workarounds readily available.

You can file new issues by filling out the new issue template.

### <a name="submit-pr"></a> Submitting a pull request (PR)

Before you submit your Pull Request (PR) consider the following guidelines:

1. Search [GitHub](https://github.com/riok/mapperly/pulls) for an open or closed PR that relates to your submission.
   You don't want to duplicate existing efforts.

2. Be sure that an issue describes the problem you're fixing, or documents the design for the feature you'd like to add.
   Discussing the design upfront helps to ensure that we're ready to accept your work.

3. Get an overview on how Mapperly works by reading this contributing documentation, the [architectural overview](./architecture) and related documentation.

4. [Fork](https://docs.github.com/en/github/getting-started-with-github/fork-a-repo) the riok/mapperly repository.

5. In your forked repository, make your changes in a new git branch:

   ```shell
   git checkout -b my-fix-branch main
   ```

6. Create your patch, including appropriate [test](./tests) cases and [documentation](./docs) updates.

7. Commit your changes using a descriptive commit message.
   Adherence to these conventions is necessary because release notes are automatically generated from these messages.
   [Husky](https://alirezanet.github.io/Husky.Net/) and [csharpier](https://csharpier.com/) automatically format changed files when commited.

   ```shell
   git commit --all
   ```

   Note: the optional commit `-a` command-line option will automatically "add" and "rm" edited files.

8. If any commits have been made to the upstream main branch,
   you should rebase your development branch so that merging it will be a simple fast-forward that won't require any conflict resolution work.

9. ```shell
   git checkout main
   git pull upstream main
   git checkout my-fix-branch
   git rebase main
   ```

10. Now, it may be desirable to squash some of your smaller commits down into a small number of larger more cohesive commits. You can do this with an interactive rebase:

    ```bash
    # Rebase all commits on your development branch
    git checkout
    git rebase -i main
    ```

11. Push your branch to GitHub:

    ```shell
    git push origin my-fix-branch
    ```

12. In GitHub, send a pull request to `riok:main` and request a review from a maintainer.

#### Checks failure

Once you've submitted a pull request, all continuous-integration checks are triggered.
If some of these checks fail, it could be either problems with the pull request or a failure of some test cases.
For more information on the failure, check the output logs of the jobs.

#### Reviewing a pull request

The reviewers will provide you feedback and approve your changes as soon as they are satisfied.
If we ask you for changes in the code, you can follow the [GitHub Guide](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/incorporating-feedback-in-your-pull-request) to incorporate feedback in your pull request.
