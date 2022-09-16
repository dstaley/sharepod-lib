using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using Icu.Collation;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace SharePodLib.Parsers.iTunesCDB
{
    /// <summary>
    /// ICU is what the iPod uses to decide how to order items taking internationalisation into consideration.
    /// http://site.icu-project.org/
    /// http://code.google.com/p/icu-dotnet/
    /// </summary>
    static class Icu
    {
        public static RuleBasedCollator Collator { get; private set; }

        public static void Initialize()
        {
            if (Collator == null)
            {
                //unpack native dlls required for icu.net to talk to
                SharePodLib.UnpackEmbeddedResource("icudt4n.dll", true);
                SharePodLib.UnpackEmbeddedResource("icuin4n.dll", true);
                SharePodLib.UnpackEmbeddedResource("icuuc4n.dll", true);

                Collator = new RuleBasedCollator("");
                Collator.NumericCollation = NumericCollation.On;
            }
        }
    }

    [SQLiteFunction(Name = "icu_data_for_string", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class IcuDataForString : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            if (args[0] is DBNull)
                return null;

            Icu.Initialize();
            string arg0 = (string)args[0];

            if (String.IsNullOrEmpty(arg0))
                return new byte[] { 0xFF };

            SortKey key = Icu.Collator.GetSortKey(arg0);
            byte[] key2 = new byte[key.KeyData.Length + 1];
            if (Char.IsNumber(arg0[0]))
            {
                key2[0] = 0xFF;
                key2[1] = 0x42;
            }
            else
            {
                key2[0] = 0x01;
                key2[1] = 0x41;
            }
            int len = key.KeyData.Length - 1;
            Array.Copy(key.KeyData, 0, key2, 2, len);
            return key2;
        }
    }

    [SQLiteFunction(Name = "icu_section_data_for_string", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class IcuSectionDataForString : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            if (args[0] is DBNull)
                return null;

            Icu.Initialize();
            string arg0 = (string)args[0];

            if (String.IsNullOrEmpty(arg0) || Char.IsNumber(arg0[0]))
            {
                return new byte[] { 0xFF };
            }
            else
            {
                SortKey key = Icu.Collator.GetSortKey(arg0);
                byte[] key2 = new byte[4];
                key2[0] = 0x01;
                key2[1] = 0x41;
                key2[2] = key.KeyData[0];
                return key2;
            }
        }
    }

    [SQLiteFunction(Name = "ML3Section", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class ML3Section : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            if (args[0] is DBNull)
                return 0;
            else
            {
                string arg0 = (string)args[0];
                if (arg0.Length == 0) return 0;
                arg0 = arg0.ToUpperInvariant();
                int chr = (int)arg0[0];
                if (chr >= 65) //65 is uppercase 'a'
                    chr -= 64;
                return chr;
            }
        }
    }

    [SQLiteFunction(Name="ML3SortCollation", FuncType=FunctionType.Collation)]
    public class ML3SortCollation : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return null;   
        }
    }

    [SQLiteFunction(Name = "ML3SearchCollation", FuncType = FunctionType.Collation)]
    public class ML3SearchCollation : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return null;
        }
    }
}
