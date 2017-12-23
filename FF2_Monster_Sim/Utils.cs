using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    public class Utils
    {
        /// <summary>
        /// Converts a string-representation of an array, to the List of strings it represents
        /// </summary>
        /// <param name="str">String formatted as "[item1,item2]"</param>
        /// <returns>Return format ["item1","item2"]</returns>
        public static List<string> StringToStringList(string str)
        {
            if (String.IsNullOrEmpty(str))
                return new List<string>(); // Consider returning null instead of an empty list?

            // Trim brackets, split on commas, upper case entries, and return it
            string trim = str.Substring(1, str.Length - 2);
            if (String.IsNullOrEmpty(trim))
                return new List<string>();
            return trim.Split(',').Select(x => x.ToUpper()).ToList();
        }

        /// <summary>
        /// Get the capped value of a given number (min - max)
        /// </summary>
        public static int EnforceNumCap(int num, int min, int max)
        {
            if (num < min)
                return min;
            if (num > max)
                return max;
            return num;
        }

        public static int EnforceNumCap(int num, int max)
        {
            if (num < 0)
                return 0;
            if (num > max)
                return max;
            return num;
        }

        /// <summary>
        /// Returns whether a number is within a given range
        /// </summary>
        public static bool NumIsWithinRange(int number, int min, int max)
        {
            return (number >= min && number <= max);
        }
    }
}
