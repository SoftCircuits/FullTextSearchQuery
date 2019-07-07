// Copyright (c) 2019 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//
using System;

namespace SoftCircuits.FullTextSearchQuery
{
    /// <summary>
    /// Helper class for parsing text.
    /// </summary>
    internal class ParsingHelper
    {
        /// <summary>
        /// Represents an invalid character. This character is returned when a valid character
        /// is not available, such as when returning a character beyond the end of the text.
        /// </summary>
        public const char NullChar = '\0';

        /// <summary>
        /// Returns the current text being parsed.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Returns the current position within the text being parsed.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Constructs a TextParse instance.
        /// </summary>
        /// <param name="text">Text to be parsed.</param>
        public ParsingHelper(string text = null)
        {
            Reset(text);
        }

        /// <summary>
        /// Sets the text to be parsed and resets the current position to the start of that text.
        /// </summary>
        /// <param name="text">The text to be parsed.</param>
        public void Reset(string text)
        {
            Text = text ?? string.Empty;
            Index = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the text being parsed.
        /// </summary>
        public bool EndOfText => (Index >= Text.Length);

        /// <summary>
        /// Returns the character at the current position, or <see cref="NullChar"/>
        /// if we're at the end of the text being parsed.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Peek() => Peek(0);

        /// <summary>
        /// Returns the character at the specified number of characters beyond the current
        /// position, or <see cref="NullChar"/> if the specified position is beyond the
        /// end of the text being parsed.
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position.</param>
        /// <returns>The character at the specified position.</returns>
        public char Peek(int ahead)
        {
            int pos = (Index + ahead);
            return (pos < Text.Length) ? Text[pos] : NullChar;
        }

        /// <summary>
        /// Extracts a substring from the specified range of the text being parsed.
        /// </summary>
        /// <param name="start">0-based position of first character to extract.</param>
        /// <param name="end">0-based position of the character that follows the last
        /// character to extract.</param>
        /// <returns>Returns the extracted string</returns>
        public string Extract(int start, int end) => Text.Substring(start, end - start);

        /// <summary>
        /// Moves the current position ahead one character. The position will not
        /// be placed beyond the end of the text being parsed.
        /// </summary>
        public void MoveAhead() => MoveAhead(1);

        /// <summary>
        /// Moves the current position ahead the specified number of characters. The position
        /// will not be placed beyond the end of the text being parsed.
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead)
        {
            Index = Math.Min(Index + ahead, Text.Length);
        }

        /// <summary>
        /// Moves the current position to the next character that is not a whitespace.
        /// </summary>
        public void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Peek()))
                MoveAhead();
        }

        /// <summary>
        /// Moves the current text position to the next character for which
        /// the given predicate returns false.
        /// </summary>
        /// <param name="predicate">Method that returns true if the character
        /// should be skipped.</param>
        public void SkipWhile(Func<char, bool> predicate)
        {
            while (predicate(Peek()) && !EndOfText)
                MoveAhead();
        }

        /// <summary>
        /// Moves the current text position to the next character for which
        /// the given predicate returns false. And returns a string with
        /// the characters that were skipped.
        /// </summary>
        /// <param name="predicate">Method that returns true if the character
        /// should be skipped.</param>
        /// <returns>A string with the characters that were skipped.</returns>
        public string ParseWhile(Func<char, bool> predicate)
        {
            int start = Index;
            while (predicate(Peek()) && !EndOfText)
                MoveAhead();
            return Extract(start, Index);
        }
    }
}
