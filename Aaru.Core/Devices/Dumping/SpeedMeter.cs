// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SpeedMeter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Measures the speed of a dump over a sliding window of wall-clock time.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics;

namespace Aaru.Core.Devices.Dumping;

// TODO: Separate the timing of the read and write paths, this also measures the time spent writing the image
/// <summary>
///     Measures dump speed over a sliding window of wall-clock time. Drives read ahead into their cache while the
///     host is busy, so the time spent inside read commands alone gives speeds a drive cannot physically reach, and a
///     short window would still fall inside a burst served from that cache.
/// </summary>
sealed class SpeedMeter
{
    const int BUCKET_COUNT = 20;

    static readonly TimeSpan _bucketLength = TimeSpan.FromMilliseconds(100);

    readonly ulong[]    _bucketBytes  = new ulong[BUCKET_COUNT];
    readonly TimeSpan[] _bucketStarts = new TimeSpan[BUCKET_COUNT];
    readonly Stopwatch  _stopwatch    = Stopwatch.StartNew();
    int                 _closedBuckets;
    int                 _currentBucket;
    bool                _started;

    /// <summary>Forgets everything measured so far, and ignores the next read as if it was the first one</summary>
    internal void Restart()
    {
        Array.Clear(_bucketBytes);
        _closedBuckets = 0;
        _currentBucket = 0;
        _started       = false;
    }

    /// <summary>Adds transferred bytes, using the time elapsed since this meter was created</summary>
    /// <param name="bytes">Bytes transferred since the previous call</param>
    /// <param name="speed">Set to the speed in MiB/s when a new value is available, otherwise left unchanged</param>
    /// <returns><c>true</c> if <paramref name="speed" /> was updated</returns>
    internal bool Update(ulong bytes, ref double speed) => Update(bytes, _stopwatch.Elapsed, ref speed);

    /// <summary>Adds transferred bytes at the specified time</summary>
    /// <param name="bytes">Bytes transferred since the previous call</param>
    /// <param name="now">Time elapsed since this meter was created</param>
    /// <param name="speed">Set to the speed in MiB/s when a new value is available, otherwise left unchanged</param>
    /// <returns><c>true</c> if <paramref name="speed" /> was updated</returns>
    internal bool Update(ulong bytes, TimeSpan now, ref double speed)
    {
        // The first read waits for the drive to spin up and seek, that would hold the speed down for a whole window
        if(!_started)
        {
            _started         = true;
            _bucketStarts[0] = now;

            return false;
        }

        _bucketBytes[_currentBucket] += bytes;

        if(now - _bucketStarts[_currentBucket] < _bucketLength) return false;

        if(_closedBuckets < BUCKET_COUNT) _closedBuckets++;

        ulong windowBytes = 0;

        for(int b = 0; b < _closedBuckets; b++)
            windowBytes += _bucketBytes[(_currentBucket - b + BUCKET_COUNT) % BUCKET_COUNT];

        TimeSpan windowStart = _bucketStarts[(_currentBucket - _closedBuckets + 1 + BUCKET_COUNT) % BUCKET_COUNT];

        speed = windowBytes / 1048576d / (now - windowStart).TotalSeconds;

        _currentBucket                = (_currentBucket + 1) % BUCKET_COUNT;
        _bucketBytes[_currentBucket]  = 0;
        _bucketStarts[_currentBucket] = now;

        return true;
    }
}