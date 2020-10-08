<!-- omit in toc -->
# Contributing

Pull requests are always welcome - we value every contribution and your input! If you are interested in becoming a maintainer, please email talen.fisher@cythral.com.

<!-- omit in toc -->
## Table of Contents

- [1. Source and Versioning Control](#1-source-and-versioning-control)
- [2. Feature & Bug Tracking](#2-feature--bug-tracking)
- [3. Editing](#3-editing)
  - [3.1. Auto-Formatting](#31-auto-formatting)
- [4. Testing](#4-testing)
- [5. Versioning](#5-versioning)
  - [5.1. Patch Fixes to Earlier Versions](#51-patch-fixes-to-earlier-versions)
  - [5.2. Starting a new Version](#52-starting-a-new-version)
- [6. Gotchas & Tips](#6-gotchas--tips)
- [7. Licensing](#7-licensing)

## 1. Source and Versioning Control

We use GitHub and git for source and versioning control.

## 2. Feature & Bug Tracking

We use [JIRA Cloud](https://cythral.atlassian.net/jira/software/c/projects/LAMBJ/issues) for feature and bug tracking. You do not need an account to comment or report issues, however issue editing and deletion do require an account. Please indicate edits or your intent to delete via comments. If you would like to be notified when an issue's status changes, please include your github username and/or other contact details in the issue summary or comments.

## 3. Editing

This project was setup for Visual Studio Code, but you may use any editor - preferably one with omnisharp support.

Before opening a pull request, please perform the following checks:

- The project builds without any errors (`dotnet build`).
- All tests pass (`dotnet test`).
- Formatting errors [are fixed](#31-auto-formatting).
- Your code makes minimal use of reflection in favor of code generation wherever possible.
- Documentation comments are added to public facing APIs.
- README documentation should be updated if applicable.
- If you know what version your change will be released in, please add an item to the release notes for that version. Release note files are located in the .github/releases folder.
- If your change introduces a major feature / epic, please add an example to the examples folder.

### 3.1. Auto-Formatting

To automatically fix any formatting errors, run the following command:

```shell
dotnet msbuild -t:format
```

## 4. Testing

If you are fixing a bug or introducing a new feature, please write unit and/or integration tests for your work. If you introduce a new test fixture, please add the `Category` attribute with the appropriate category - "Unit" for unit tests and "Integration" for integration tests.

Tests are run using the dotnet CLI command:

```shell
dotnet test
```

To only run unit tests, run:

```shell
dotnet test --filter Category=Unit
```

To only run integration tests, run:

```shell
dotnet test --filter Category=Integration
```

If you use Visual Studio Code, tasks have been added for the three above commands.

Testing is done with NUnit, NSubstitute and FluentAssertions. Please use FluentAssertions wherever possible over the assertion utilities that NUnit provides.

## 5. Versioning

We use SemVer for versioning our project, and every tag pushed to the repository indicates a release (packages will automatically deploy to NuGet from tags and Github Packages everywhere else).

### 5.1. Patch Fixes to Earlier Versions

We try to use a roll-forward method of making changes, rather than going back and fixing bugs in earlier versions. (For instance, if v0.3.0 just came out, we won't go back and fix bugs in v0.2.0). However, if an exception needs to be made for urgent security reasons, version branches can be merged into release/{version} to trigger a release. Another PR should be opened afterwards to cherry-pick those changes into master if applicable.

### 5.2. Starting a new Version

1. Create a new version in JIRA
2. Create a ticket in JIRA for release preparations
3. Update version.json with the new version number
4. Create a release notes file in .github/releases
5. Create a PR with the release preparations done in step 3 and 4.

## 6. Gotchas & Tips

- To add a new dependency to a project, run `dotnet add package [package] --no-restore` then `dotnet restore --force-evaluate`
- The [Markdown All-In-One VSCode extension](https://marketplace.visualstudio.com/items?itemName=yzhang.markdown-all-in-one) can be used to update table of contents in the README and contributing guide.
- To run a build without checking formatting, do `dotnet build -p:RunFormatter=false` (formatting can sometimes take awhile, be sure to run formatters after you're done iterating though).

## 7. Licensing

By contributing, you agree that your contributions will licensed under the [MIT License](LICENSE.txt).
