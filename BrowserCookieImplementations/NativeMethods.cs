using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace ryu_s.BrowserCookie
{
    class NativeMethods
    { 
        /// <summary>
             /// 
             /// </summary>
             /// <param name="pDataIn"></param>
             /// <param name="ppszDataDescr"></param>
             /// <param name="pOptionalEntropy"></param>
             /// <param name="pvReserved"></param>
             /// <param name="pPromptStruct"></param>
             /// <param name="dwFlags"></param>
             /// <param name="pDataOut"></param>
             /// <returns></returns>
        [DllImport("Crypt32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, string ppszDataDescr,
            ref DATA_BLOB pOptionalEntropy, IntPtr pvReserved, IntPtr pPromptStruct, uint dwFlags, out DATA_BLOB pDataOut);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct DATA_BLOB : IDisposable
        {
            public int cbData;
            public IntPtr pbData;
            void IDisposable.Dispose()
            {
                if (pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pbData);//これは内部でLocalFree()を呼び出しているらしい。
                    pbData = IntPtr.Zero;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string CryptUnprotectData(byte[] data, Encoding enc)
        {
            string value = null;
            DATA_BLOB pDataIn = default(DATA_BLOB);
            DATA_BLOB pDataOut = default(DATA_BLOB);
            try
            {
                pDataIn = new DATA_BLOB { pbData = Marshal.AllocHGlobal(data.Length), cbData = data.Length };
                DATA_BLOB pOptionalEntropy = default(DATA_BLOB);
                Marshal.Copy(data, 0, pDataIn.pbData, data.Length);

                if (CryptUnprotectData(ref pDataIn, null, ref pOptionalEntropy, IntPtr.Zero, IntPtr.Zero, 0, out pDataOut))
                {
                    var numArray = new byte[pDataOut.cbData];
                    Marshal.Copy(pDataOut.pbData, numArray, 0, pDataOut.cbData);
                    value = enc.GetString(numArray);
                }
            }
            finally
            {
                ((IDisposable)pDataIn).Dispose();
                ((IDisposable)pDataOut).Dispose();
            }

            return value;
        }
    }
}
