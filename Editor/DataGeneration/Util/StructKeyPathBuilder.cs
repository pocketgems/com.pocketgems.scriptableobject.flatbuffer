using System;
using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    public class StructKeyPathBuilder
    {
        private string _rootKeyType;
        private string _rootKeyIdentifier;
        private List<string> _paths;
        private List<string> _pathStrings;
        private const char kStringDelimiter = '.';
        private const char kIdentifierContainerLeft = '[';
        private const char kIdentifierContainerRight = ']';
        private const char kArrayIndexContainerLeft = '[';
        private const char kArrayIndexContainerRight = ']';

        public StructKeyPathBuilder()
        {
            _paths = new List<string>();
            _pathStrings = new List<string>();
        }

        /// <summary>
        /// Deconstructs a KeyPath to return the root key
        /// </summary>
        /// <param name="keyPath">input keypath</param>
        /// <param name="rootKeyType">the root key type</param>
        /// <param name="rootKeyIdentifier">the root key identifier</param>
        /// <param name="error">error in parsing input, else null if no error</param>
        /// <returns>true on success, else false</returns>
        public static bool FetchRootKey(string keyPath, out string rootKeyType, out string rootKeyIdentifier, out string error)
        {
            if (string.IsNullOrEmpty(keyPath))
            {
                rootKeyType = null;
                rootKeyIdentifier = null;
                error = "keyPath is null or empty";
                return false;
            }

            rootKeyType = null;
            rootKeyIdentifier = null;
            var startIndex = keyPath.IndexOf(kIdentifierContainerLeft);
            var endIndex = keyPath.IndexOf(kIdentifierContainerRight);
            if (startIndex == -1 || endIndex == -1)
            {
                error = $"Couldn't Find {kIdentifierContainerLeft} or {kIdentifierContainerRight}";
                return false;
            }
            if (endIndex <= startIndex)
            {
                error = $"Found {kStringDelimiter} before {kIdentifierContainerLeft} or {kIdentifierContainerRight}";
                return false;
            }
            if (startIndex == 0)
            {
                error = $"No type set";
                return false;
            }
            if (endIndex - startIndex == 1)
            {
                error = $"No Identifier set";
                return false;
            }

            error = null;
            rootKeyType = keyPath.Substring(0, startIndex);
            rootKeyIdentifier = keyPath.Substring(startIndex + 1, endIndex - startIndex - 1);
            return true;
        }

        public static IComparer<string> Comparer() => new KeyPathComparer();

        public int Length => _paths.Count;

        public string RootKeyType => _rootKeyType;
        public string RootKeyIdentifier => _rootKeyIdentifier;

        public void PushRootKey(string type, string identifier)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(identifier))
                throw new InvalidDataException($"{nameof(PushRootKey)} called with empty {nameof(type)} or {nameof(identifier)}");

            _rootKeyType = type;
            _rootKeyIdentifier = identifier;

            PushKeyString($"{type}{kIdentifierContainerLeft}{identifier}{kIdentifierContainerRight}");
        }

        public void PushKey(string key, int index = -1)
        {
            if (string.IsNullOrEmpty(key))
                throw new InvalidDataException($"{nameof(PushKey)} called with empty {nameof(key)}");

            if (string.IsNullOrEmpty(_rootKeyType) || string.IsNullOrEmpty(_rootKeyIdentifier))
                throw new InvalidDataException($"{nameof(PushRootKey)} must be called before {nameof(PushKey)}");

            if (key.Contains(kStringDelimiter) ||
                key.Contains(kArrayIndexContainerLeft) ||
                key.Contains(kArrayIndexContainerRight))
                throw new InvalidDataException(
                    $"Key cannot contain {kStringDelimiter}, {kArrayIndexContainerLeft}, or {kArrayIndexContainerRight}");

            var keyString = key;
            if (index >= 0)
                keyString = $"{key}{kArrayIndexContainerLeft}{index}{kArrayIndexContainerRight}";
            PushKeyString(keyString);
        }

        public void PopKey()
        {
            if (_paths.Count == 0)
                throw new IndexOutOfRangeException("No values to pop");
            _paths.RemoveAt(_paths.Count - 1);
            _pathStrings.RemoveAt(_pathStrings.Count - 1);

            if (_paths.Count == 0)
            {
                _rootKeyType = null;
                _rootKeyIdentifier = null;
            }
        }

        public string KeyPath()
        {
            if (_pathStrings.Count == 0)
                return "";
            return _pathStrings[_pathStrings.Count - 1];
        }

        public void Clear()
        {
            _paths.Clear();
            _pathStrings.Clear();
            _rootKeyType = null;
            _rootKeyIdentifier = null;
        }

        private void PushKeyString(string key)
        {
            _paths.Add(key);
            if (_pathStrings.Count > 0)
                _pathStrings.Add(KeyPath() + kStringDelimiter + key);
            else
                _pathStrings.Add(key);
        }

        /// <summary>
        /// Comparer to sort the Struct KeyPath into a meaningful group order
        /// </summary>
        private class KeyPathComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                int? typeCompare = null;
                int xSearchIndex = 0;
                int ySearchIndex = 0;

                /*
                 * compare the type and save the result since it's the first part of the keypath
                 *
                 * Example:
                 *  SpecialEventInfo[Adventure].EventReward.Transaction
                 *  EventInfo[Adventure].EventReward.Transaction
                 *
                 * EventInfo should be sorted before SpecialEventInfo.
                 */
                while (typeCompare == null)
                {
                    // base exit cases - bad formatted keypath
                    if (xSearchIndex >= x.Length && ySearchIndex >= y.Length)
                        return 0;
                    if (xSearchIndex >= x.Length)
                        return -1;
                    if (ySearchIndex >= y.Length)
                        return 1;

                    char xChar = x[xSearchIndex];
                    char yChar = y[ySearchIndex];
                    if (xChar == kIdentifierContainerLeft && yChar == kIdentifierContainerLeft)
                        typeCompare = 0;
                    else if (xChar == kIdentifierContainerLeft)
                        typeCompare = -1;
                    else if (yChar == kIdentifierContainerLeft)
                        typeCompare = 1;
                    else if (xChar != yChar)
                        typeCompare = xChar < yChar ? -1 : 1;
                    else
                    {
                        xSearchIndex++;
                        ySearchIndex++;
                    }
                }

                // fast forward so the brackets match [
                while (xSearchIndex < x.Length && x[xSearchIndex] != kIdentifierContainerLeft)
                    xSearchIndex++;
                while (ySearchIndex < y.Length && y[ySearchIndex] != kIdentifierContainerLeft)
                    ySearchIndex++;

                // index for next search starts after the [
                xSearchIndex++;
                ySearchIndex++;

                int identifierXStart = xSearchIndex;
                int identifierYStart = ySearchIndex;

                // base exit cases - bad formatted keypath
                if (xSearchIndex >= x.Length && ySearchIndex >= y.Length)
                    return typeCompare.Value;
                if (xSearchIndex >= x.Length)
                    return -1;
                if (ySearchIndex >= y.Length)
                    return 1;

                // fast forward so index is after ]
                while (xSearchIndex < x.Length && x[xSearchIndex] != kIdentifierContainerRight)
                    xSearchIndex++;
                while (ySearchIndex < y.Length && y[ySearchIndex] != kIdentifierContainerRight)
                    ySearchIndex++;
                int identifierXEnd = xSearchIndex;
                int identifierYEnd = ySearchIndex;
                xSearchIndex++;
                ySearchIndex++;

                // base exit cases - bad formatted keypath
                if (xSearchIndex >= x.Length && ySearchIndex >= y.Length)
                    // both do not have key paths - compare types and then identifiers
                    return typeCompare != 0 ? typeCompare.Value :
                        FileNameComparer.CompareSubstring(
                            x, identifierXStart, identifierXEnd,
                            y, identifierYStart, identifierYEnd);
                // else the one with a path is sorted after the non path ones
                if (xSearchIndex >= x.Length)
                    return -1;
                if (ySearchIndex >= y.Length)
                    return 1;

                /*
                 * compare the keypath (the rest of the string)
                 *
                 *
                 * Example:
                 *  EventInfo[Adventure].BonusRewardTransaction
                 *  EventInfo[Adventure].EventReward.Transaction
                 *  EventInfo[Adventure].EventRewards[0].Transaction[0]
                 *  EventInfo[Adventure].EventRewards[1].Transaction[0]
                 *  EventInfo[Adventure].EventRewards[1].Transaction[1]
                 *  EventInfo[Surfing].EventRewards[0].Transaction[0]
                 *  EventInfo[Surfing].EventRewards[1].Transaction[0]
                 *  EventInfo[Surfing].EventRewards[1].Transaction[1]
                 *
                 * Sort by KeyPaths:
                 *  BonusRewardTransaction
                 *  EventReward.Transaction
                 *  EventRewards.Transaction (not using array indexes)
                 *
                 * For matching paths for array indexes, sort by index grouped on the the Type & Identifier
                 */
                int? keyPathCompare = null;
                bool isScanningArrayIndex = false;
                // bool isSkippingArrayIndex = false;
                int arrayIndexCompare = 0;
                int xArrayIndexValue = 0;
                int yArrayIndexValue = 0;
                while (keyPathCompare == null)
                {
                    // base exit cases - end of KeyPath
                    if (xSearchIndex >= x.Length && ySearchIndex >= y.Length)
                    {
                        if (isScanningArrayIndex && xArrayIndexValue != yArrayIndexValue)
                            keyPathCompare = xArrayIndexValue < yArrayIndexValue ? -1 : 1;
                        else
                            keyPathCompare = 0;
                        continue;
                    }
                    if (xSearchIndex >= x.Length)
                    {
                        keyPathCompare = -1;
                        continue;
                    }
                    if (ySearchIndex >= y.Length)
                    {
                        keyPathCompare = 1;
                        continue;
                    }

                    char xChar = x[xSearchIndex];
                    char yChar = y[ySearchIndex];
                    if (isScanningArrayIndex)
                    {
                        // searching & calculating the array index
                        if (xChar == kArrayIndexContainerRight && yChar == kArrayIndexContainerRight)
                        {
                            if (arrayIndexCompare == 0 && xArrayIndexValue != yArrayIndexValue)
                                arrayIndexCompare = xArrayIndexValue < yArrayIndexValue ? -1 : 1;
                            // finished parsing the array field
                            xSearchIndex++;
                            ySearchIndex++;
                            isScanningArrayIndex = false;
                        }
                        if (xChar != kArrayIndexContainerRight)
                        {
                            xArrayIndexValue = xArrayIndexValue * 10 + (xChar - '0');
                            xSearchIndex++;
                        }
                        if (yChar != kArrayIndexContainerRight)
                        {
                            yArrayIndexValue = yArrayIndexValue * 10 + (yChar - '0');
                            ySearchIndex++;
                        }
                        continue;
                    }

                    // KeyPath comparison
                    if (xChar != yChar)
                    {
                        keyPathCompare = xChar < yChar ? -1 : 1;
                        continue;
                    }

                    if (xChar == kArrayIndexContainerLeft)
                    {
                        xArrayIndexValue = 0;
                        yArrayIndexValue = 0;
                        isScanningArrayIndex = true;
                    }

                    xSearchIndex++;
                    ySearchIndex++;
                }

                /*
                 * PRIORITY OF SORT:
                 */

                /*
                 * Group the rows by keypaths first so the same nested depths in all Scriptable Objects
                 * are grouped together. (e.g. I want to param all rewards together).
                 */
                if (keyPathCompare != 0)
                    return keyPathCompare.Value;
                /*
                 * If the KeyPaths are the same, group by parameter type (e.g. I want all buildings together)
                 */
                if (typeCompare != 0)
                    return typeCompare.Value;
                /*
                 * If KeyPath & Types are the same, sort by Scriptable Object identifier.
                 */
                int identifierCompare = FileNameComparer.CompareSubstring(
                    x, identifierXStart, identifierXEnd,
                    y, identifierYStart, identifierYEnd);
                if (identifierCompare != 0)
                    return identifierCompare;

                /*
                 * Lastly, if everything matches, check the array indexes so that elements in an array are grouped together
                 */
                return arrayIndexCompare;
            }
        }
    }
}
