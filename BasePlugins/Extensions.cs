using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NIHT.Plugins.Base {
    public static class Extensions {
        public static bool IsEqual(this string value, string[] vergleich) {
            foreach (string one in vergleich) {
                if (one.IndexOf("*") > -1) {
                    if (Regex.IsMatch(value, "^" + Regex.Escape(one).Replace("\\*", ".*") + "$"))
                        return true;
                } else if (value == one)
                    return true;
            }
            return false;
        }
        public static bool IsEqual(this string value, string vergleich) {
            if (vergleich.IndexOf("*") > -1)
                return Regex.IsMatch(value, "^" + Regex.Escape(vergleich).Replace("\\*", ".*") + "$");
            else if (value == vergleich)
                return true;
            return false;

        }
    }
}
