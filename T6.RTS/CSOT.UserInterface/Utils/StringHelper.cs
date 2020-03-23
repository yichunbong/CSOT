using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace CSOT.UserInterface.Utils
{
    public class StringHelper
    {
        public static char KeySeparator = '@';
        public static char ListSeparator = ',';
        public static char FieldSeparator = ';';
        public static char RecordSeparator = '\t';

        public static string NULL_ID = "-";
        public static bool UseAscii85 = true;


        public static bool Equals(string a, string b, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            return string.Equals(a, b, comparisonType);
        }                

        public static bool Like(string text, string pattern)
        {
            return Mozart.Text.LikeUtility.Like(text, pattern);
        }

        public static EnumT Parse<EnumT>(string value, EnumT defaultValue)
        {
            return Parse<EnumT>(value, defaultValue, true);
        }


        public static EnumT Parse<EnumT>(string value, EnumT defaultValue, bool ignoreCase)
        {
            try
            {
                return (EnumT)Enum.Parse(typeof(EnumT), value, ignoreCase);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ConcatKey(object o1, object o2)
        {
            return o1.ToString() + KeySeparator + o2;
        }

        public static string ConcatKey(object o1, object o2, object o3)
        {
            return o1.ToString() + KeySeparator + o2 + KeySeparator + o3;
        }

        public static string ConcatKey(object o1, object o2, object o3, object o4)
        {
            return o1.ToString() + KeySeparator + o2 + KeySeparator + o3 + KeySeparator + o4;
        }
        
        public static string ConcatKey(params object[] args)
        {
            return Concat(KeySeparator, args);
        }

        private static string Concat<TCh>(TCh separator, params object[] args)
        {
            if (args == null || args.Length == 0)
                return string.Empty;

            string first = (args[0] != null) ? args[0].ToString() : "";
            if (args.Length == 1)
                return first;
            if (args.Length == 2)
                return first + separator + args[1];
            if (args.Length == 3)
                return first + separator + args[1] + separator + args[2];
            if (args.Length == 4)
                return first + separator + args[1] + separator + args[2] + separator + args[3];
            
            StringBuilder sb = new StringBuilder(first);
            for (int i = 1; i < args.Length; i++)
            {
                sb.Append(separator);
                sb.Append(args[i]);
            }

            return sb.ToString();
        }

        //static readonly string[] EmptyStringArray = new string[0];
        //public static string[] SplitSorted(string value)
        //{
        //    return SplitSorted(value, ListSeparator);
        //}

        //public static string[] SplitSorted(string value, char separator)
        //{
        //    if (string.IsNullOrEmpty(value))
        //        return EmptyStringArray;

        //    string[] items = value.Split(separator);
        //    ISet<string> st = new SortedListSet<string>();
        //    for (int i = 0; i < items.Length; i++)
        //        st.Add(items[i].Trim());
        //    return st.ToArray();
        //}

        //public static string[] Split(string value)
        //{
        //    return Split(value, ListSeparator);
        //}

        //public static string[] Split(string value, char separator)
        //{
        //    if (string.IsNullOrEmpty(value))
        //        return EmptyStringArray;

        //    string[] items = value.Split(separator);
        //    string[] results = new string[items.Length];
        //    for (int i = 0; i < items.Length; i++)
        //        results[i] = items[i].Trim();
        //    return results;
        //}

        private static string Merge<TCh>(string[] args, TCh separator)
        {
            if (args == null || args.Length == 0)
                return string.Empty;

            if (args.Length == 1)
                return args[0];
            if (args.Length == 2)
                return args[0] + separator + args[1];
            if (args.Length == 3)
                return args[0] + separator + args[1] + separator + args[2];

            StringBuilder sb = new StringBuilder(args[0]);
            for (int i = 1; i < args.Length; i++)
            {
                sb.Append(separator);
                sb.Append(args[i]);
            }

            return sb.ToString();
        }

        public static string Merge(string[] args, char separator)
        {
            return Merge(args, separator);
        }

        public static string Merge(string[] args, string separator)
        {
            return Merge(args, separator);
        }

        public static string MakeListString(params object[] args)
        {
            return Concat(ListSeparator, args);
        }

        public static string MakeListString(ICollection items)
        {
            bool first = true;
            StringBuilder sb = new StringBuilder();
            foreach (object item in items)
            {
                if (first) first = false;
                else sb.Append(ListSeparator);
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }

        public static List<string> ExtractListString(string listStr)
        {
            if (string.IsNullOrEmpty(listStr))
                return new List<string>();

            string[] ary = listStr.Split(ListSeparator);
            List<string> list = new List<string>(ary.Length);
            foreach (string a in ary)
                list.Add(a.Trim());
            return list;
        }

        public static bool IsEmptyID(string text)
        {
            return Mozart.SeePlan.StringUtility.IsEmptyID(text);
        }

        public static string ToSafeString(string p, string defaultVale = null)
        {
            if (IsEmptyID(p))
            {
                if (defaultVale == null)
                    defaultVale = string.Empty;

                return defaultVale;
            }

            return p;
        }
           

        //public static Dictionary<string, string> ParseParamKeyValue(string paramValue)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();

        //    try
        //    {
        //        List<string> list = StringHelper.ExtractListString(paramValue);
        //        foreach (string str in list)
        //        {
        //            if (str.Contains("=") == false)
        //                continue;

        //            string[] arr = StringHelper.Split(str, '=');
        //            if (arr.Length != 2)
        //                continue;

        //            string key = arr[0].Trim();
        //            string value = arr[1].Trim();

        //            if (dic.ContainsKey(key) == true)
        //                continue;

        //            dic.Add(key, value);
        //        }
        //    }
        //    catch { }

        //    return dic;
        //}


        public static string Trim(string _str)
        {
            int start = 0;
            int num = 0;//중간 띄어쓰기 위치
            string tmp = _str;
            while (tmp.IndexOf(" ") > 0)
            {
                num = tmp.IndexOf(" ");
                string tmp1 = tmp.Substring(0, num);
                start = num + 1;
                tmp1 += tmp.Substring(num + 1);
                tmp = tmp1;
            }
            return tmp;
        }

        public static bool CheckWildString(string wildString, string targetString)
        {
            if (wildString == "ALL")
                return true;

            if (string.IsNullOrEmpty(targetString) || targetString == "-")
                targetString = "";

            if (Regex.IsMatch(targetString, WildCardToRegex(wildString.ToUpper()), RegexOptions.IgnoreCase))
                return true;
            else
                return false;
        }

        public static string WildCardToRegex(string pattern)
        {
            string s = "^" + Regex.Escape(pattern).Replace("%", ".*").Replace("\\?", ".") + "$";
            return s;
        }

        public static string ToSafeUpper(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            return text.ToUpper();
        }

        #region string compress part

        public static string Compress(string data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Compress(stream, data);
                if (UseAscii85)
                    return Ascii85Encode(stream.GetBuffer(), 0, (int)stream.Length);
                else return Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public static void Compress(Stream outputStream, string data)
        {
            Compress(outputStream, data, Encoding.ASCII);
        }

        public static void Compress(Stream outputStream, string data, Encoding encoding)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                DeflateStream gzip = new DeflateStream(outputStream, CompressionMode.Compress, true);

                byte[] ascii = encoding.GetBytes(data);
                gzip.Write(ascii, 0, ascii.Length);

                gzip.Close();
            }
        }

        public static string Decompress(string data)
        {
            byte[] buf = (UseAscii85) ? Ascii85Decode(data) : Convert.FromBase64String(data);
            using (MemoryStream stream = new MemoryStream(buf))
            {
                return Decompress(stream);
            }
        }

        public static string Decompress(Stream inputStream)
        {
            return Decompress(inputStream, Encoding.ASCII);
        }

        public static string Decompress(Stream inputStream, Encoding encoding)
        {
            return Decompress(inputStream, encoding, 4096);
        }

        public static string Decompress(Stream inputStream, Encoding encoding, int bufferSize)
        {
            using (MemoryStream decompressedStream = new MemoryStream((int)inputStream.Length * 4))
            {
                DeflateStream gzip = new DeflateStream(inputStream, CompressionMode.Decompress, true);

                byte[] buffer = new byte[bufferSize];
                while (true)
                {
                    int readBytes = gzip.Read(buffer, 0, bufferSize);
                    if (readBytes <= 0) break;
                    //decompressed.Append(encoding.GetString(buffer, 0, readBytes));
                    decompressedStream.Write(buffer, 0, readBytes);
                }
                gzip.Close();

                decompressedStream.Position = 0;
                return encoding.GetString(decompressedStream.GetBuffer(), 0, (int)decompressedStream.Length);
            }
        }

        #endregion

        #region Ascii85
        private static uint[] pow85 = { 85 * 85 * 85 * 85, 85 * 85 * 85, 85 * 85, 85, 1 };
        private const int ascii85Offset = 33;
        private const int ascii85encLength = 5;
        private const int ascii85decLength = 4;

        /// <summary>
        /// Encodes binary data into a plaintext ASCII85 format string
        /// </summary>
        /// <param name="ba">binary data to encode</param>
        /// <returns>ASCII85 encoded string</returns>
        public static string Ascii85Encode(byte[] ba, int baoffset, int bacount)
        {
            byte[] encodedBlock = new byte[ascii85encLength];

            StringBuilder sb = new StringBuilder(bacount * ascii85encLength / ascii85decLength);
            uint tuple = 0;
            int count = 0;
            byte b;
            for (int i = baoffset; i < bacount; i++)
            {
                b = ba[i];
                if (count >= ascii85decLength - 1)
                {
                    tuple |= b;
                    if (tuple == 0)
                    {
                        sb.Append('z');
                    }
                    else
                    {
                        encodedBlock[4] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                        encodedBlock[3] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                        encodedBlock[2] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                        encodedBlock[1] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                        encodedBlock[0] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;

                        sb.Append((char)encodedBlock[0]);
                        sb.Append((char)encodedBlock[1]);
                        sb.Append((char)encodedBlock[2]);
                        sb.Append((char)encodedBlock[3]);
                        sb.Append((char)encodedBlock[4]);
                    }

                    tuple = 0;
                    count = 0;
                }
                else
                {
                    tuple |= (uint)(b << (24 - (count * 8)));
                    count++;
                }
            }

            // if we have some bytes left over at the end..
            if (count > 0)
            {
                encodedBlock[4] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                encodedBlock[3] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                encodedBlock[2] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                encodedBlock[1] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;
                encodedBlock[0] = (byte)((tuple % 85) + ascii85Offset); tuple /= 85;

                for (int i = 0; i < count + 1; i++)
                    sb.Append((char)encodedBlock[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes an ASCII85 encoded string into the original binary data
        /// </summary>
        /// <param name="s">ASCII85 encoded string</param>
        /// <returns>byte array of decoded binary data</returns>
        public static byte[] Ascii85Decode(string s)
        {
            byte[] decodedBlock = new byte[ascii85decLength];

            MemoryStream ms = new MemoryStream(s.Length * ascii85decLength / ascii85encLength);
            int count = 0;
            bool processChar = false;
            uint tuple = 0;

            char c;
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                switch (c)
                {
                    case 'z':
                        if (count != 0)
                            throw new Exception("The character 'z' is invalid inside an ASCII85 block.");

                        decodedBlock[0] = 0;
                        decodedBlock[1] = 0;
                        decodedBlock[2] = 0;
                        decodedBlock[3] = 0;
                        ms.Write(decodedBlock, 0, decodedBlock.Length);
                        processChar = false;
                        break;
                    case '\n':
                    case '\r':
                    case '\t':
                    case '\0':
                    case '\f':
                    case '\b':
                        processChar = false;
                        break;
                    default:
                        if (c < '!' || c > 'u')
                            throw new Exception("Bad character '" + c + "' found. ASCII85 only allows characters '!' to 'u'.");

                        processChar = true;
                        break;
                }

                if (processChar)
                {
                    tuple += ((uint)(c - ascii85Offset) * pow85[count]);
                    count++;
                    if (count == ascii85encLength)
                    {
                        decodedBlock[0] = (byte)(tuple >> 24 - (0 * 8));
                        decodedBlock[1] = (byte)(tuple >> 24 - (1 * 8));
                        decodedBlock[2] = (byte)(tuple >> 24 - (2 * 8));
                        decodedBlock[3] = (byte)(tuple >> 24 - (3 * 8));

                        ms.Write(decodedBlock, 0, decodedBlock.Length);
                        tuple = 0;
                        count = 0;
                    }
                }
            }

            // if we have some bytes left over at the end..
            if (count != 0)
            {
                if (count == 1)
                    throw new Exception("The last block of ASCII85 data cannot be a single byte.");

                count--;
                tuple += pow85[count];

                for (int k = 0; k < count; k++)
                    decodedBlock[k] = (byte)(tuple >> 24 - (k * 8));

                for (int i = 0; i < count; i++)
                    ms.WriteByte(decodedBlock[i]);
            }

            return ms.ToArray();
        }
        #endregion Ascii85
    
    }
}
