using System;
using System.Text;

namespace FileSystemIDandChk
{
    public static class StringHandlers
    {
        public static string CToString(byte[] CString)
        {
            StringBuilder sb = new StringBuilder();
			
            for (int i = 0; i < CString.Length; i++)
            {
                if (CString[i] == 0)
                    break;

                sb.Append(Encoding.ASCII.GetString(CString, i, 1));
            }
			
            return sb.ToString();
        }

        public static string PascalToString(byte[] PascalString)
        {
            StringBuilder sb = new StringBuilder();

            byte length = PascalString[0];

            for (int i = 1; i < length + 1; i++)
            {
                sb.Append(Encoding.ASCII.GetString(PascalString, i, 1));
            }

            return sb.ToString();
        }
    }
}

