namespace System.Data.JsonRpc
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides extension methods to the <see cref="string">System.String</see> object.
    /// </summary>
    public static partial class StringExts
    {
        /// <summary>
        /// "Borrowed" from the Json.NET project https://github.com/JamesNK/Newtonsoft.Json
        /// and optimized
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>Converted string.</returns>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            var chars = s.ToCharArray();
            var len = chars.Length;

            chars[0] = char.ToLowerInvariant(chars[0]);

            if (len > 1)
            {
                if (!char.IsUpper(chars[1]))
                {
                    return new string(chars);
                }

                chars[1] = char.ToLowerInvariant(chars[1]);
            }
            else
            {
                return new string(chars);
            }

            for (int i = 2; i < len; i++)
            {
                if (i + 1 < len && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }
    }
}
