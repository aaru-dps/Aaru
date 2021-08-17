# Contributing to Aaru

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

The following is a set of guidelines for contributing to Aaru and its modules. These are mostly guidelines, not rules.
Use your best judgment, and feel free to propose changes to this document in a pull request.

#### Table Of Contents

[Code of Conduct](#code-of-conduct)

[I don't want to read this whole thing, I just have a question!!!](#i-dont-want-to-read-this-whole-thing-i-just-have-a-question)

[What should I know before I get started?](#what-should-i-know-before-i-get-started)

* [Aaru and modules](#aaru-and-modules)

[How Can I Contribute?](#how-can-i-contribute)

* [Reporting Devices](#reporting-devices)
* [Reporting Bugs](#reporting-bugs)
* [Suggesting Enhancements](#suggesting-enhancements)
* [Your First Code Contribution](#your-first-code-contribution)
* [Pull Requests](#pull-requests)
* [Patronizing us](#patronizing)
* [Donating hardware to test](#donating)
* [Providing information](#needed-information)

[Styleguides](#styleguides)

* [Git Commit Messages](#git-commit-messages)
* [Code Styleguide](#code-styleguide)

## Code of Conduct

This project and everyone participating in it is governed by the
[Aaru Code of Conduct](.github/CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please
report unacceptable behavior to [claunia@claunia.com](mailto:claunia@claunia.com).

## I don't want to read this whole thing I just have a question!!!

> **Note:** Please don't file an issue to ask a question. You'll get faster results by using the resources below.

You can join our IRC channel on irc.libera.chat at channel #Aaru

## What should I know before I get started?

### Aaru and modules

Aaru is a large open source project &mdash; it's made up of 18 modules. When you initially consider contributing to
Aaru, you might be unsure about which of those modules implements the functionality you want to change or report a bug
for. This section should help you with that.

Aaru is intentionally very modular. Here's a list of them:

* [Claunia.RsrcFork](https://github.com/claunia/Claunia.RsrcFork) - This library includes code for handling Mac OS
  resource forks, and decoding them, so any code related to Mac OS resource forks should be added here.
* [Claunia.Encoding](https://github.com/claunia/Claunia.Encoding) - This library includes code for converting codepages
  not supported by .NET, like those used by ancient operating systems, to/from UTF-8.
* [plist-cil](https://github.com/claunia/plist-cil) - This library includes code for handling Apple property lists.
* [SharpCompress](https://github.com/adamhathcock/sharpcompress) - This library includes code for handling compression
  algorithms and compressed archives. Any need you have of compression or decompression should be handled with this
  library, and any new algorithm should be added here.
* [Aaru](https://github.com/aaru-dps/Aaru/tree/master/Aaru) - This module contains the command line interface. In the
  future a GUI will be added.
* [AaruRemote](https://github.com/aaru-dps/aaruremote) - Standalone small application designed to run on older machines
  where Aaru does not run to allow device commands to be executed remotely.
* [Aaru.Checksums](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Checksums) - This module contains the checksum,
  hashing and error correction algorithms.
* [Aaru.CommonTypes](https://github.com/aaru-dps/Aaru.CommonTypes) - This module contains interfaces, structures and
  enumerations needed by more than one of the other modules.
* [Aaru.Console](https://github.com/aaru-dps/Aaru.Console) - This module abstracts consoles used by other modules to
  output information, so they can be redirected to a CLI or to a GUI output.
* [Aaru.Core](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Core) - This module contains the implementation of the
  functions and commands that are called by the user interface itself.
* [Aaru.Decoders](https://github.com/aaru-dps/Aaru.Decoders) - This module contains internal disk, drive and protocol
  structures as well as code to marshal, decode and print them.
* [Aaru.Devices](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Devices) - This module contains code to talk with
  hardware devices in different platforms. Each platform has lowlevel calls in its own folder, and each device protocol
  has highlevel calls in its own folder. Device commands are separated by protocol standard, or vendor name.
* [Aaru.Device.Report](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Device.Report) - This is a separate application
  in C89 designed to create device reports on enviroments where you can't run .NET or Mono but can run Linux.
* [Aaru.DiscImages](https://github.com/aaru-dps/Aaru/tree/master/Aaru.DiscImages) - This module provides reading
  capabilities for the disk/disc images, one per file.
* [Aaru.DiscImages](https://github.com/aaru-dps/Aaru.Dto) - This module provides common structures between Aaru and
  Aaru.Server.
* [Aaru.Filesystems](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Filesystems) - This module provides the
  filesystem support. If only identification is implemented a single file should be used. For full read-only support, a
  folder should be used.
* [Aaru.Filters](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Filters) - A filter is a modification of the data
  before it can be passed to the disk image module
  (compression, fork union, etc), and this module provides support for them. If a image is compressed, say in gzip, or
  encoded, say in AppleDouble, a filter is the responsible of decompressing or decoding it on-the-fly.
* [Aaru.Helpers](https://github.com/aaru-dps/Aaru.Helpers) - This module contains a collection of helpers for array
  manipulation, big-endian marshalling, datetime conversion, hexadecimal printing, string manipulation and byte
  swapping.
* [Aaru.Partitions](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Partitions) - This module contains code for
  reading partition schemes.
* [Aaru.Server](https://github.com/aaru-dps/Aaru.Server) - This module contains the server-side code that's running
  at https://www.aaru.app
* [Aaru.Settings](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Settings) - This module contains code for handling
  Aaru settings.
* [Aaru.Tests](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Tests) - This module contains the unit tests for the
  rest of the modules. You should add new unit tests here but cannot run all of them because the test images they
  require amount to more than 100GiB.
* [Aaru.Tests.Devices](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Tests.Devices) - This module presents a menu
  driven interface to send commands to devices, as a way to test the Core module, as those tests cannot be automated. It
  can be used to debug drive responses.

## How Can I Contribute?

### Reporting Devices

Aaru tries to be as universal as possible. However some devices do not behave in the expected ways, some media is
unknown and needs to be known prior to enabling dumping of it, etc.

For that reason, Aaru includes
the [device-report command](https://github.com/aaru-dps/Aaru/wiki/Reporting-physical-device-capabilities). Using this
command will guide you thru a series of questions about the device, and if it contains removable media, for you to
insert the different media you have, and create a report of its abilities. The report will automatically be sent to our
server and saved on your computer. Please note that we do not store any personal information and when possible remove
the drive serial numbers from the report.

If you have a drive attached to a computer that you cannot run the full Aaru on it but can compile a C89 application,
you can use [AaruRemote](https://github.com/aaru-dps/aaruremote)
and an ethernet connection between that machine and a machine that can run the full Aaru.

### Reporting Bugs

This section guides you through submitting a bug report for Aaru. Following these guidelines helps maintainers and the
community understand your report :pencil:, reproduce the behavior :computer: :computer:, and find related reports :
mag_right:.

Before creating bug reports, please check [this list](#before-submitting-a-bug-report) as you might find out that you
don't need to create one. When you are creating a bug report, please
[include as many details as possible](#how-do-i-submit-a-good-bug-report). Fill out
[the required template](.github/ISSUE_TEMPLATE.md), the information it asks for helps us resolve issues faster.

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

#### Before Submitting A Bug Report

* **Check the [wiki](https://github.com/aaru-dps/Aaru/wiki)** for a list of common questions and problems.
* **Determine [which module the problem should be reported in](#aaru-and-modules)**.
* **Perform a [cursory search](https://github.com/search?q=+is%3Aissue+user%3Aclaunia)**
  to see if the problem has already been reported. If it has **and the issue is still open**, add a comment to the
  existing issue instead of opening a new one.

#### How Do I Submit A (Good) Bug Report?

Bugs are tracked as [GitHub issues](https://guides.github.com/features/issues/). After you've
determined [which module](#aaru-and-modules) your bug is related to, create an issue on that repository and provide the
following information by filling in
[the template](.github/ISSUE_TEMPLATE.md).

Explain the problem and include additional details to help maintainers reproduce the problem:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the exact steps which reproduce the problem** in as many details as possible. For example, start by
  explaining how you started Aaru, e.g. which command exactly you used in the terminal. Also note that some device
  commands requires you to have administrative privileges, be in a specific group, or be the root user, so try it again
  with escalated privileges.
* **Provide specific examples to demonstrate the steps**. Include links to media images, reports of the devices, or the
  output of using [Aaru.Tests.Devices](https://github.com/aaru-dps/Aaru/tree/master/Aaru.Tests.Devices).
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that
  behavior.
* **Explain which behavior you expected to see instead and why.**
* **Include a copy of the output in the terminal** enabling both verbose, using the `-v`
  command line parameter, and debug, using the `-d` command line parameter, outputs.
* **If you're reporting that Aaru crashed**, try doing the same with the debug version and include a crash report with a
  stack trace. Include the crash report in the issue in
  a [code block](https://help.github.com/articles/markdown-basics/#multiple-lines), a
  [file attachment](https://help.github.com/articles/file-attachments-on-issues-and-pull-requests/), or put it in
  a [gist](https://gist.github.com/) and provide link to that gist.
* **If the problem wasn't triggered by a specific action**, describe what you were doing before the problem happened and
  share more information using the guidelines below.

Include details about your configuration and environment:

* **Which version of Aaru are you using?**
* **What's the name and version of the OS you're using**?
* **Are you running Aaru in a virtual machine?** If so, which VM software are you using and which operating systems and
  versions are used for the host and the guest?
* **Are you trying to execute a device command?** If so, who manufactured the device, which model is it, and how is it
  connected to the computer?

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion for Aaru, including completely new features and
minor improvements to existing functionality. Following these guidelines helps maintainers and the community understand
your suggestion :pencil: and find related suggestions :mag_right:.

Before creating enhancement suggestions,
please [include as many details as possible](#how-do-i-submit-a-good-enhancement-suggestion). Fill
in [the template](.github/ISSUE_TEMPLATE.md), including the steps that you imagine you would take if the feature you're
requesting existed.

#### How Do I Submit A (Good) Enhancement Suggestion?

Enhancement suggestions are tracked as [GitHub issues](https://guides.github.com/features/issues/). After you've
determined [which module](#aaru-and-modules) your enhancement suggestion is related to, create an issue on that
repository and provide the following information:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Provide specific examples to demonstrate the steps**. If the feature is about a media image, filesystem,
  partitioning scheme, or filter, please include as many test files as possible, and if applicable which software
  created them.
* **Describe the current behavior** and **explain which behavior you expected to see instead** and why.
* **List some other applications where this enhancement exists.**
* **Specify which version of Aaru you're using.**
* **Specify the name and version of the OS you're using.**

### Your First Code Contribution

Unsure where to begin contributing to Aaru? You can start by looking through these `beginner` and `help-wanted` issues:

* [Beginner issues][beginner] - issues which should only require a few lines of code, and a test or two.
* [Help wanted issues][help-wanted] - issues which should be a bit more involved than `beginner` issues.

Both issue lists are sorted by total number of comments. While not perfect, number of comments is a reasonable proxy for
impact a given change will have.

If you want to read about using Aaru, the [wiki](https://github.com/aaru-dps/Aaru/wiki) is available.

Do not modify the interfaces. If you need or want to, comment in an issue how and why you want to change it and we'll
discuss it. Same applies for creating new interfaces.

Aaru uses C# 7 language features (inline declaration, Tuples, etc.) so it can only be compiled
with [VisualStudio](http://www.visualstudio.com) 2017 or higher, [Xamarin Studio](https://www.xamarin.com/download)
7 or higher, [MonoDevelop](http://www.monodevelop.com) 7 or higher,
or [JetBrains Rider](https://www.jetbrains.com/rider/) 2017.2 or higher.

### Pull Requests

* Fill in [the required template](.github/PULL_REQUEST_TEMPLATE.md)
* Do not include issue numbers in the PR title
* Follow the [code styleguide](#code-styleguide).
* Include test files as applicable, that do not have software under copyright inside them, if possible.
* Document new code based using XML documentation wherever possible.
* DO NOT end files with a newline.
* Avoid platform-dependent code, unless absolutely needed. Any call to a part of the .NET framework that doesn't start
  with `System.` is probably platform-dependent.
* Do not call libraries external to .NET. Only Interop calls to the operating system kernel
  (that is `KERNEL32.DLL` in Windows and `libc` in others) will be accepted. If you need to talk with a USB devices your
  pull request must implement calls both to `WinUsb` and `libusb`.

### Patronizing

If you want to donate money you can become a patron at https://www.patreon.com/claunia

### Donating

You may donate us one of the [devices we need](NEEDED.md).

### Needed information

If you have test images, imaging applications that generate formats we do not support, or documentation about media dump
formats, filesystems or partitioning schemes we do not support, you can provide us with that information to add support
for them.

## Styleguides

### Git Commit Messages

* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line

### Code Style Guide

- Braces are unindented at next line (BSD style).
- Braces with no content should be opened and closed in the same line.
- Constants should be ALL_UPPER_CASE.
- Do not use braces for statements that don't need them.
- Do not use more than one blank line.
- Do not use spaces before or after parentheses.
- Do not use `var` ever.
- `else`, `while`, `catch` and `finally` should be on a new line.
- If you know C apply a simple rule: Be as C as and as less C# or C++ as possible.
- If you will only store variables, use a struct. If you need it to be nullable, use a nullable struct if applicable.
- Indent statements and cases.
- Indent using 4 spaces (soft tab).
- Instace and static fields should be lowerCamelCase.
- Public fields should be UpperCamelCase.
- Separate attributes.
- Use 120 columns margins.
- Use built-in keywords: `uint` instead of `UInt32`.
- Use expression bodies only for properties, indexes and events. For the rest use block bodies.
- Use implicit modifiers.
- Use inline variable declaration.
- Use struct implicit constructor.
- Use UNIX (`\n`) endline character.

> Note: There is an included editorconfig file that sets the appropriate code style.

> Note: Aaru is quite low-level so unneeded object-oriented abstractions
(e.g. using classes when a struct suffices) will be rejected. LINQ is accepted.