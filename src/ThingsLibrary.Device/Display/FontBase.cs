﻿namespace ThingsLibrary.Device.Display.Font
{
    /// <summary>
    /// Abstract class for a Font.
    /// </summary>
    public abstract class FontBase
    {
        /// <summary>
        /// Width of a character in the font.
        /// </summary>
        public abstract byte Width { get; }

        /// <summary>
        /// Height of a character in the font.
        /// </summary>
        public abstract byte Height { get; }

        /// <summary>
        /// Get the binary representation of the ASCII character from the font table.
        /// </summary>
        /// <param name="character">Character to look up.</param>
        /// <returns>Array of bytes representing the binary bit pattern of the character.</returns>
        public abstract byte[] this[char character] { get; }
    }
}