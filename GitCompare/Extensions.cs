using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitCompare
{
    public static class Extensions
    {
        public static string RemoveLeadingStar(this string str)
        {
            Regex leadingStar = new Regex(@"^\*");
            return leadingStar.Replace(str, string.Empty);
        }

        public static string PascalToLowerWithSpaces(this string str)
        {
            Regex uppercaseLetter = new Regex(@"([A-Z])");
            return uppercaseLetter.Replace(str, match => $" {match.Value.ToLower()}").Trim();
        }

        public static string ToStatusString(this RepoStatusFlags value)
        {
            string[] splitAndLowered = value.ToString()
                .Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                .Reverse()
                .Select(i => i.PascalToLowerWithSpaces())
                .ToArray();

            return string.Join(", ", splitAndLowered);
        }
    }
}
