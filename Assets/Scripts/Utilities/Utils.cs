using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Globalization;
using System;
using System.Text.RegularExpressions;

public class Utils
{
    public static IEnumerable<string> SortByLength(IEnumerable<string> e) {
        // Use LINQ to sort the array received and return a copy.
        var sorted = from s in e
                     orderby s.Length ascending
                     select s;
        return sorted;
    }

    public static string ReturnFormattedUSNumber(string val, string decimalDigits = "C0") {

        val = ReturnFormattedPrice(double.Parse(val));

        /*NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        nfi.CurrencyDecimalSeparator = ",";
        nfi.CurrencyGroupSeparator = ".";
        nfi.CurrencySymbol = "";*/
        return val;
        //return Convert.ToDecimal(val).ToString(decimalDigits, nfi);
    }

    public static float ParseInvariantCulture(string num) {
        return float.Parse(num, CultureInfo.InvariantCulture);
    }

    public static IEnumerator ExecuteAfterWait(float wait, Action callBack) {

        yield return new WaitForSeconds(wait);

        callBack();

    }

    public static string ReturnFormattedPrice(double price) {

        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        nfi.CurrencyDecimalSeparator = ".";
        nfi.CurrencyGroupSeparator = " ";
        nfi.CurrencySymbol = "";

        if (price >= 10000f)
            return price.ToString("C0", nfi);
        if (price >= 0.99f)
            return price.ToString("C2", nfi);
        else if (price >= 0.01f)
            return price.ToString("C4", nfi);
        else if (price >= 0.0001f)
            return price.ToString("C6", nfi);
        else return price.ToString();
    }

    public static string CheckStringEllipsis(string str, int maxChars) {
        if (str.Length > maxChars)
            return str.Remove(maxChars) + "...";
        else
            return str;
    }

    public static string RemoveRichText(string str) {
        string regex = "(\\<.*\\>)";
        string output = Regex.Replace(str, regex, "");

        return output;
    }

    public static string ReturnVolumeString(double vol) {
        return ReturnFormattedPrice(vol);
    }
    
}
