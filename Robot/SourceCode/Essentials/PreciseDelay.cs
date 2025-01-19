/*
 * File: PreciseDelay.cs
 * Author: Tomáš Bartoš
 * Date: November 3, 2023
 * Description: This file contains the PreciseDelay method, which provides functions connected with precise timind and delays.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot.Essentials
{
    public static class StopwatchExtension
    {
        // extend the functionality of System.Diagnostics stopwatch by adding function that gets elapsed microseconds
        public static long ElapsedMicroseconds(this Stopwatch stopwatch)
        {
            return stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1_000_000L));
        }
    }

    internal class PreciseDelay
    {
        public long DelayMicroseconds(long microseconds)
        {
            Stopwatch delayWatch = Stopwatch.StartNew();

            while (delayWatch.ElapsedMicroseconds() < microseconds)
            {
                // Do nothing, just wait
            }

            delayWatch.Stop();
            return delayWatch.ElapsedMicroseconds();
        }
    }
}
