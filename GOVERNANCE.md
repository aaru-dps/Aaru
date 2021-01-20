# Aaru Governance

This document defines the project governance for Aaru.

## Overview

**Aaru**, an open source project, is committed to building an open, inclusive, and productive open source community focused on delivering a high quality tool that for backing up (dumping) various types of computer and game console media.

The community is governed by this document with the goal of defining how community should work together to achieve this goal.

## Code Repositories

The following code repositories are governed by Aaru community and maintained under the `aaru-dps` organization.

* **[Aaru](https://github.com/aaru-dps/aaru):** Main Aaru codebase.
* **[Aaru.Documentation](https://github.com/aaru-dps/Aaru.Documentation):** Official documentation.
* **[Aaru.Decryption](https://github.com/aaru-dps/Aaru.Decryption):** Library for the decryption of media.
* **[Aaru.CommonTypes](https://github.com/aaru-dps/Aaru.CommonTypes):** Common types needed by the different Aaru modules.
* **[Aaru.Decoders](https://github.com/aaru-dps/Aaru.Decoders):** Library for the decoding of structures from media and drives.
* **[Aaru.Server](https://github.com/aaru-dps/Aaru.Server):** https://aaru.app server codebase.
* **[fstester](https://github.com/aaru-dps/fstester):** Toolkit for the generation of test filesystems for later reverse engineer.
* **[aaruremote](https://github.com/aaru-dps/aaruremote):** Small application allowing to send Aaru commands to a different computer.
* **[Aaru.Dto](https://github.com/aaru-dps/Aaru.Dto):** DTOs for interchange between client and server portions of Aaru.
* **[Aaru.Console](https://github.com/aaru-dps/Aaru.Console):** Text console handler.
* **[Aaru.Helpers](https://github.com/aaru-dps/Aaru.Helpers):** Helper functions.
* **[Aaru.Checksums](https://github.com/aaru-dps/Aaru.Checksums):** Library that implements the hashing functionality.
* **[010templates](https://github.com/aaru-dps/010templates):** Templates for [010editor](https://www.sweetscape.com/010editor).
* **[libaaruformat](https://github.com/aaru-dps/libaaruformat):** Main implementation of Aaru Media Image Format.
* **[Aaru.VideoNow](https://github.com/aaru-dps/Aaru.VideoNow):** VideoNow decoding and converting tool.
* **[archaaru](https://github.com/aaru-dps/archaaru):** Scripts for the generation of Arch Linux Rescue CD including Aaru Data Preservation Suite.
* **[Aaru.Dreamcast](https://github.com/aaru-dps/Aaru.Dreamcast):** Tool for dumping GD-ROM using a real Dreamcast.

## Community Roles

* **Users:** Members that engage with the Aaru community via any medium (Discord, GitHub, etc.).
* **Contributors:** Regular contributions to projects (documentation, code reviews, responding to issues, participation in proposal discussions, contributing code, etc.). 
* **Technical committee**: Provide input and feedback on roadmap items, grounded in common use cases for the committee member's organizations. Committee members might sponsor certain aspects of the project, however sponsorships are not a requirement for a committee member role.
* **Maintainers**: The Aaru project leaders. They are responsible for the overall health and direction of the project; final reviewers of PRs and responsible for releases. Some Maintainers are responsible for one or more components within a project, acting as technical leads for that component. Maintainers are expected to contribute code and documentation, review PRs including ensuring quality of code, triage issues, proactively fix bugs, and perform maintenance tasks for these components.

### Maintainer nomination

New maintainers must be nominated by an existing maintainer and must be elected by a supermajority of existing maintainers. Likewise, maintainers can be removed by a supermajority of the existing maintainers or can resign by notifying one of the maintainers.

### Technical committee member nomination

New technical committee members must be nominated by an existing member and must be elected by a supermajority of existing members. Likewise, members can be removed by a supermajority of the existing members or can resign by notifying one of the members.

### Supermajority

A supermajority is defined as two-thirds of members in the group.
A supermajority of [Maintainers](#maintainers) is required for certain decisions as outlined above. A supermajority vote is equivalent to the number of votes in favor being at least twice the number of votes against. For example, if you have 5 maintainers, a supermajority vote is 4 votes. Voting on decisions can happen on the mailing list, GitHub, Discord, email, or via a voting service, when appropriate. Maintainers can either vote "agree, yes, +1", "disagree, no, -1", or "abstain". A vote passes when supermajority is met. An abstain vote equals not voting at all.

### Decision Making

We try to operate more on consensus than on votes, seeking agreement from the people who will have to do the work.

Natalia Portillo ([@claunia](https://github.com/claunia)) is the self-appointed benevolent dictator for life (SABDFL) for Aaru.

The community functions best when it can reach broad consensus about a way forward. However, it is not uncommon in the open-source world for there to be multiple good arguments, no clear consensus, and for open questions to divide communities rather than enrich them. The debate absorbs the energy that might otherwise have gone towards the creation of a solution. In many cases, there is no one ‘right’ answer, and what is needed is a decision more than a debate. The SABDFL acts to provide clear leadership on difficult issues, and set the pace for the project.

## Lazy Consensus

To maintain velocity in Aaru, the concept of [Lazy
Consensus](http://en.osswiki.info/concepts/lazy_consensus) is practiced. Ideas
and / or proposals should be shared by maintainers via
GitHub with the appropriate maintainers tagged. Out of respect for other contributors,
major changes should also be accompanied by a notification on Discord or a note on the
mailing list (not created yet) as appropriate. Author(s) of proposal, Pull Requests,
issues, etc.  will give a time period of no less than five (5) working days for
comment and remain cognizant of popular observed world holidays.

Other maintainers may chime in and request additional time for review, but
should remain cognizant of blocking progress and abstain from delaying
progress unless absolutely needed. The expectation is that blocking progress
is accompanied by a guarantee to review and respond to the relevant action(s)
(proposals, PRs, issues, etc.) in short order.

Lazy Consensus is practiced for all projects in the `aaru-dps` org, including
the main project repository and the additional repositories.

Lazy consensus does _not_ apply to the process of:

* Removal of maintainers from Aaru

## Updating Governance

All substantive changes in Governance require a supermajority agreement by all maintainers.
