using System;
using System.Runtime.InteropServices;

namespace console_app_rest_client
{
    public class NativeMethods
    {
        private const string LIBCURL = "libcurl";

        #region curl_slist_append

        [DllImport(LIBCURL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr curl_slist_append(IntPtr slist, string data);

        #endregion

        #region curl_slist_free_all

        [DllImport(LIBCURL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void curl_slist_free_all(IntPtr pList);

        #endregion
    }
}