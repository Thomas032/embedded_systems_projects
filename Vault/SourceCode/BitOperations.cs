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

namespace Trezor.Essentials
{
    public class BitOperations
    {
        /// <summary>
        /// Method for setting a specific bit in the given byte.
        /// </summary>
        public byte SetBit(byte value, int bit, bool inversedLogic = false)
        {
            if (inversedLogic)
            {
                return (byte)(value & ~(0x01 << bit));
            }
            return (byte)(value | (0x01 << bit));
        }

        /// <summary>
        /// Method for nulling (setting to 0) a specific bit in the given byte.
        /// </summary>
        public byte NullBit(byte value, int bit, bool inversedLogic = false)
        {
            if (inversedLogic)
            {
                return (byte)(value | (0x01 << bit));
            }
            return (byte)(value & ~(0x01 << bit));
        }

        /// <summary>
        /// Method for inverting all bits in the given byte.
        /// </summary>
        public byte InvertByte(byte value)
        {
            return (byte)~value;
        }

        /// <summary>
        /// Method for toggling a specific bit in the given byte.
        /// </summary>
        public byte ChangeBit(byte value, int bit, bool inversedLogic = false)
        {
            if (inversedLogic)
            {
                return (byte)(value ^ (0x01 << bit));
            }
            return (byte)(value ^ (0x01 << bit));
        }

        /// <summary>
        /// Method for merging two bytes together.
        /// </summary>
        public byte MergeBytes(byte byte0, byte byte1)
        {
            return (byte)(byte0 & byte1);
        }

        /// <summary>
        /// Method for checking if a specific bit of the given byte is high (1).
        /// </summary>
        public bool IsHigh(byte value, int bit, bool inverseLogic = false)
        {
            if (inverseLogic)
            {
                return ((value & (0x01 << bit)) == 0);
            }
            return ((value & (0x01 << bit)) != 0);
        }

        /// <summary>
        /// Method for checking if a specific bit in a given byte is low (0)
        /// </summary>
        public bool IsLow(byte value, int bit, bool inverseLogic = false)
        {
            if (inverseLogic)
            {
                return ((value & (0x01 << bit)) != 0);
            }
            return ((value & (0x01 << bit)) == 0);
        }
    }
}
