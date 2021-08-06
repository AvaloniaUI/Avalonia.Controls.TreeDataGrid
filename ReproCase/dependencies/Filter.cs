using System;
using System.Collections.Generic;

namespace PlasticGui
{
    public class Filter
    {
        public interface IFilterableRow
        {
            string GetColumnText(string columnName);
        }

        public Filter(string filterString)
        {
            mFilterString = filterString;
            Parse();
        }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(mFilterString);
            }
        }

        public bool IsNonParameterizedFilter
        {
            get
            {
                return mParameters.Keys.Count == 0;
            }
        }

        public string FilterString
        {
            get
            {
                return mFilterString;
            }
        }

        public bool SameFilter(string filter)
        {
            return filter == mFilterString;
        }

        public bool IsMatch(IFilterableRow row, List<string> columnNames)
        {
            if (IsNonParameterizedFilter)
                return IsFullMatch(row, columnNames);

            return IsFilterParameterized(row, columnNames);
        }

        public bool MatchParameter(string value, string paramName)
        {
            if (value == null || paramName == null)
                return false;

            string storedValue;
            if (!mParameters.TryGetValue(paramName.ToLowerInvariant(), out storedValue))
                return true;
            return value.IndexOf(storedValue, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public bool MatchFull(string value)
        {
            if (value == null)
                return false;

            if (mbIsExactMatch)
                return value == mFilterString;
            return value.IndexOf(
                mFilterString, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        bool IsFullMatch(IFilterableRow row, List<string> columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (MatchFull(row.GetColumnText(columnName)))
                    return true;
            }
            return false;
        }

        bool IsFilterParameterized(IFilterableRow row, List<string> columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (!MatchParameter(row.GetColumnText(columnName), columnName))
                    return false;
            }
            return true;
        }

        void Parse()
        {
            if (string.IsNullOrEmpty(mFilterString))
                return;

            if (mFilterString[0] == QUOTE && mFilterString[mFilterString.Length - 1] == QUOTE)
                mbIsExactMatch = true;

            if (mFilterString.IndexOfAny(KEY_VALUE_SEPARATORS) == -1)
                return;

            string[] parameters = SplitFilterString();

            foreach (string parameter in parameters)
            {
                string[] keyValue = parameter.Split(KEY_VALUE_SEPARATORS, KEY_VALUE_COUNT);

                if (keyValue.Length < KEY_VALUE_COUNT)
                    continue;

                string key = keyValue[0].ToLowerInvariant();
                if (mParameters.ContainsKey(key))
                {
                    mParameters[key] = keyValue[1];
                    continue;
                }

                mParameters.Add(keyValue[0].ToLowerInvariant(), keyValue[1]);
            }
        }

        string[] SplitFilterString()
        {
            int startIdx = 0;
            bool inQuotes = false;
            IList<string> parameters = new List<string>();
            for (int i = 0; i < mFilterString.Length; i++)
            {
                if (mFilterString[i] == QUOTE)
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (mFilterString[i] == SEPARATOR)
                {
                    if (inQuotes)
                        continue;

                    int length = i - startIdx;
                    if (length == 0)
                        continue;

                    string param = ExtractParam(startIdx, length);
                    parameters.Add(param);

                    startIdx = i + 1;
                }
            }

            if (startIdx < mFilterString.Length)
                parameters.Add(ExtractParam(startIdx, mFilterString.Length - startIdx));

            string[] result = new string[parameters.Count];
            parameters.CopyTo(result, 0);
            return result;
        }

        string ExtractParam(int startIdx, int length)
        {
            string param = mFilterString.Substring(startIdx, length);
            if (param.IndexOf(QUOTE) != -1)
                param = param.Replace(QUOTE.ToString(), string.Empty);
            return param;
        }

        string mFilterString;
        bool mbIsExactMatch;
        IDictionary<string, string> mParameters = new Dictionary<string, string>();
        const char QUOTE = '"';
        const char SEPARATOR = ' ';
        static readonly char[] KEY_VALUE_SEPARATORS = new char[] { ':' };
        const int KEY_VALUE_COUNT = 2;
    }
}