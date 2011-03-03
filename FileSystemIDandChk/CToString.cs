using System;
using System.Text;

namespace FileSystemIDandChk
{
	public static class StringHandlers
	{
		public static string CToString (byte[] CString)
		{
			StringBuilder sb = new StringBuilder();
			
			for(int i = 0; i<CString.Length; i++)
			{
				if(CString[i]==0)
					break;

				sb.Append(Encoding.ASCII.GetString(CString, i, 1));
			}
			
			return sb.ToString();
		}
	}
}

