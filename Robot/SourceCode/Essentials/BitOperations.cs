/*
 * File: BitOperations.cs
 * Author: Tomáš Bartoš
 * Date: November 3, 2023
 * Description: This file contains the BitOperations class, which provides functions for bitwise manipulation
 *              with the support for inverted logic.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot.Essentials
{
    internal class BitOperations
    {
        // Function to set a specific bit in the given byte.
        public byte SetBit(byte value, int bit, bool inversedLogic=false)
        {
            if (inversedLogic)
            {
                return (byte)(value & ~(0x01 << bit));
            }
            return (byte)(value | (0x01 << bit));
        }

        // Function to null (set to 0) a specific bit in the given byte.
        public byte NullBit(byte value, int bit, bool inversedLogic=false)
        {
            if (inversedLogic)
            {
                return (byte)(value | (0x01 << bit));
            }
            return (byte)(value & ~(0x01 << bit));
        }

        public byte InvertByte(byte value)
        {
            return (byte)~value;
        }

        // Function to toggle a specific bit in the given byte.
        public byte ChangeBit(byte value, int bit, bool inversedLogic=false)
        {
            if (inversedLogic)
            {
                return (byte)(value ^ (0x01 << bit));
            }
            return (byte)(value ^ (0x01 << bit));
        }

        // Function for merging two bytes together
        public byte MergeBytes(byte byte0, byte byte1)
        {
            return (byte)(byte0 & byte1);
        }

        // Function to check if a specific bit in the given byte is high (1).
        public bool IsHigh(byte value, int bit, bool inverseLogic=false)
        {
            if (inverseLogic)
            {
                return ((value & (0x01 << bit)) == 0);
            }
            return ((value & (0x01 << bit)) != 0);
        }

        // Function to check if a specific bit in the given byte is low (0).
        public bool IsLow(byte value, int bit, bool inverseLogic=false)
        {
            if (inverseLogic)
            {
                return ((value & (0x01 << bit)) != 0);
            }
            return ((value & (0x01 << bit)) == 0);
        }
    }
}
