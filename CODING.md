Coding
======

Contributing with code to DiscImageChef has three very simple rules:

- Use same style as currently used. In a nutshell:
  - Tabs instead of spaces
  - Brackets in separate lines
  - UNIX line endings
  - Do not separate parenthesis, 
  - Indent every code block (for, foreach, while, if, switch, case)
- Do not modify the interfaces. If you need or want to, comment in an issue how and why you want to change it and we'll discuss it.
Same applies for creating new interfaces.
- Everything has a place, a module and an interface. Following is the list of interfaces.


[Claunia.RsrcFork](https://github.com/claunia/Claunia.RsrcFork)
---------------------------------------------------------------
- License: MIT

This library includes code for handling Mac OS resource forks, and decoding them, so any code relating to Mac OS resource forks should be added here.

[Claunia.Encoding](https://github.com/claunia/Claunia.Encoding)
---------------------------------------------------------------
- License: MIT

This library includes code for converting codepages not supported by .NET, like those used by ancient operating systems, to/from UTF-8.

[plist-cil](https://github.com/claunia/plist-cil)
-------------------------------------------------
- License: MIT

This library includes code for handling Apple property lists.

[SharpCompress](https://github.com/adamhathcock/sharpcompress)
--------------------------------------------------------------
- License: MIT

This library includes code for handling compression algorithms and compressed archives.
Any need you have of compression or decompression should be handled with this library, and any new algorithm should be added here.

[DiscImageChef](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef)
-----------------------------------------------------------------------------------
- License: GPL

This module contains the command line interface and core code.
In the future the core code will be separated from the CLI and a GUI will be added.

[DiscImageChef.Checksums](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Checksums)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains the checksum, hashing and error correction algorithms.

[DiscImageChef.Checksums](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.CommonTypes)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains structs and enumerations needed by more than one of the other modules.

[DiscImageChef.Decoders](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Decoders)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains internal disk, drive and protocol structures as well as code to marshal, decode and print them.

[DiscImageChef.Devices](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Devices)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains code to talk with hardware devices in different platforms.

Each platform has lowlevel calls in its own folder, and each device protocol has highlevel calls in its own folder.
Device commands are separated by protocol standard, or vendor name.

[DiscImageChef.DiscImages](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.DiscImages)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module provides reading capabilities for the disk/disc images, one per file.

[DiscImageChef.Filesystems](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Filesystems)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module provides the filesystem support. If only identification is implemented a single file should be used. For full read-only support, a folder should be used.

[DiscImageChef.Filters](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Filters)
-------------------------------------------------------------------------------------------------------
- License: LGPL

A filter is a modification of the data before it can be passed to the disk image module (compression, fork union, etc), and this module provides support for them.

[DiscImageChef.Helpers](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Helpers)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains a collection of helpers for array manipulation, big-endian marshalling, datetime conversion, hexadecimal printing, string manipulation and byte swapping.

[DiscImageChef.Interop](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Interop)
-------------------------------------------------------------------------------------------------------
- License: MIT

This module contains calls to the underlying operating system. Currently only OS detection is needed.

[DiscImageChef.Metadata](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Metadata)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains handling of CICM XML metadata, media types and dimensions.

[DiscImageChef.Partitions](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Partitions)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains code for reading partition schemes.

[DiscImageChef.Settings](https://github.com/claunia/DiscImageChef/tree/master/DiscImageChef.Settings)
-------------------------------------------------------------------------------------------------------
- License: LGPL

This module contains code for handling DiscImageChef settings.
