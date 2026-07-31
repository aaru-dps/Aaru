using NUnit.Framework;

// Disk images can be memory-hungry; keep the parallel worker count conservative.
// Only fixtures explicitly marked [Parallelizable] (e.g. FilesystemTest) run concurrently.

[assembly: LevelOfParallelism(4)]