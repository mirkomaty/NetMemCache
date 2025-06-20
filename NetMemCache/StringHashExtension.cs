using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMemCache
{
    public static class StringHashExtension
    {
        /// <summary>
        /// This method provides a HashCode, which remains the same
        /// after the new start of an application. Note, that String.GetHashCode()
        /// gets a different HashCode for each instance of the application process.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetConstantHashCode( this string s )
        {
            if (s == null)
                throw new ArgumentNullException( "s" );

            if (s == String.Empty)
                s = " ";
            // Make sure that the hash code is random throughout
            // it's whole length, if s is very short
            while (s.Length < 8)
                s += s;
            Byte[] barr = Encoding.UTF8.GetBytes(s);
            int hash = -1;
            int l = barr.Length;
            for (int i = 0; i < l; i++)
            {
                byte b = barr[i];
                hash = ( hash << 5 ) - hash + b;
            }
            return hash;
        }
    }
}
