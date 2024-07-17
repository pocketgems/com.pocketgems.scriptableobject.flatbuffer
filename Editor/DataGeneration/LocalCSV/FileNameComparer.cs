using System;
using System.Collections.Generic;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    /// <summary>
    /// Comparer that mimic's basic Unity file sorting (doesn't cover all cases).
    /// </summary>
    internal class FileNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return CompareSubstring(x, 0, x.Length, y, 0, y.Length);
        }

        /// <summary>
        /// Compare a substring allowing to specify and end indexes for reuse.
        /// </summary>
        /// <param name="x">string to compare</param>
        /// <param name="xStart">start index</param>
        /// <param name="xEnd">end index (equivalent to length)</param>
        /// <param name="y">string to compare</param>
        /// <param name="yStart">start index</param>
        /// <param name="yEnd">end index (equivalent to length)</param>
        /// <returns></returns>
        public static int CompareSubstring(string x, int xStart, int xEnd, string y, int yStart, int yEnd)
        {
            int xIndex = xStart;
            int yIndex = yStart;

            // skip all non numerical characters that match first
            while (xIndex < xEnd && yIndex < yEnd)
            {
                if (x[xIndex] >= '0' && x[xIndex] <= '9')
                    break;
                if (y[yIndex] >= '0' && y[yIndex] <= '9')
                    break;
                if (x[xIndex] != y[yIndex])
                    break;
                xIndex++;
                yIndex++;
            }

            if (xIndex >= xEnd && yIndex >= yEnd)
                return 0;
            if (xIndex >= xEnd)
                return -1;
            if (yIndex >= yEnd)
                return 1;

            // search prefix numbers to detect number and padding
            int xNumLength = 0;
            int xInt = 0;
            while (xIndex < xEnd)
            {
                if (x[xIndex] < '0' || x[xIndex] > '9')
                    break;
                xNumLength++;
                xInt *= 10;
                xInt += x[xIndex] - '0';
                xIndex++;
            }

            int yNumLength = 0;
            int yInt = 0;
            while (yIndex < yEnd)
            {
                if (y[yIndex] < '0' || y[yIndex] > '9')
                    break;
                yNumLength++;
                yInt *= 10;
                yInt += y[yIndex] - '0';
                yIndex++;
            }

            if (xNumLength > 0 && yNumLength > 0)
            {
                // compare the number
                var intCompare = xInt.CompareTo(yInt);
                if (intCompare != 0)
                    return intCompare;
                // in case of padded 0's
                if (xNumLength != yNumLength)
                    return xNumLength > yNumLength ? -1 : 1;
            }

            if (xIndex >= xEnd && yIndex >= yEnd)
                return 0;
            if (xIndex >= xEnd)
                return -1;
            if (yIndex >= yEnd)
                return 1;

            // search the rest by case insensitive/sensitive at the same time.
            int caseCompare = 0;
            while (xIndex < xEnd && yIndex < yEnd)
            {
                char xChar = x[xIndex];
                char yChar = y[yIndex];
                char xCharOriginal = xChar;
                char yCharOriginal = yChar;
                if (xChar != yChar)
                {
                    bool xAlphabet = false;
                    bool yAlphabet = false;
                    if (xChar >= 'a' && xChar <= 'z')
                    {
                        xAlphabet = true;
                    }
                    else if (xChar >= 'A' && xChar <= 'Z')
                    {
                        // turn to lower case
                        xChar = (char)(xChar - 'A' + 'a');
                        xAlphabet = true;
                    }

                    if (yChar >= 'a' && yChar <= 'z')
                    {
                        yAlphabet = true;
                    }
                    else if (yChar >= 'A' && yChar <= 'Z')
                    {
                        // turn to lower case
                        yChar = (char)(yChar - 'A' + 'a');
                        yAlphabet = true;
                    }

                    // if we find a sort order in ignore case first, then return right away
                    int ignoreCaseCompare = xChar.CompareTo(yChar);
                    if (ignoreCaseCompare != 0)
                        return ignoreCaseCompare;

                    // case compare if we haven't figured that out prior
                    if (xAlphabet && yAlphabet && caseCompare == 0)
                        caseCompare = xCharOriginal.CompareTo(yCharOriginal);
                }
                xIndex++;
                yIndex++;
            }

            if (xIndex >= xEnd && yIndex >= yEnd)
                // return the case compare result as a last tie breaker
                return caseCompare;
            if (xIndex >= xEnd)
                return -1;
            if (yIndex >= yEnd)
                return 1;

            // we shouldn't get here - if so something bad happened
            return caseCompare;
        }
    }
}
