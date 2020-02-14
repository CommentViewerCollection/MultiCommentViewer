using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ryu_s.BrowserCookie
{
    class ChromeValueDecryptor
    {
        public string LocalStatePath { get; set; }
        private string UnProtect(byte[] source)
        {
            return NativeMethods.CryptUnprotectData(source, Encoding.UTF8);
        }
        public (bool, string) Decrypt(byte[] encrypted_value)
        {
            var data = encrypted_value;
            if (data[0] == 1 && data[1] == 0 && data[2] == 0 && data[3] == 0)
            {
                var value = UnProtect(encrypted_value);
                return (value != null, value);
            }
            else if (data[0] == 'v' && data[1] == '1' && data[2] == '0')
            {
                var size = Marshal.SizeOf(data[0]) * data.Length;
                var gh = Marshal.AllocHGlobal(size);
                Marshal.Copy(data, 0, gh, data.Length);
                var sh = NativeMethods.Decrypt(gh, data.Length, LocalStatePath);
                var s = sh.AsString();
                var isSuccess = s[0] != '0';
                var decryptedValue = s.Substring(1);
                return (isSuccess, decryptedValue);
            }
            else
            {
                return (false, null);
            }
        }
        internal class StringHandle : SafeHandle
        {
            public StringHandle() : base(IntPtr.Zero, true) { }

            public override bool IsInvalid => false;

            public string AsString()
            {
                var len = 0;
                while (Marshal.ReadByte(handle, len) != 0) { ++len; }
                var buffer = new byte[len];
                Marshal.Copy(handle, buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer);
            }

            protected override bool ReleaseHandle()
            {
                NativeMethods.FreeString(handle);
                return true;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        struct DecryptResult
        {
            //[MarshalAs(UnmanagedType.I4)]
            //public int is_success;
            [MarshalAs(UnmanagedType.LPWStr)]
            public StringHandle data;
        }
        static class NativeMethods
        {
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
                var pDataIn = default(DATA_BLOB);
                var pDataOut = default(DATA_BLOB);
                try
                {
                    pDataIn = new DATA_BLOB { pbData = Marshal.AllocHGlobal(data.Length), cbData = data.Length };
                    var pOptionalEntropy = default(DATA_BLOB);
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
            const string DirPath = @"encrypted_value_decryptor.dll";
            [DllImport(DirPath, EntryPoint = "free_string", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void FreeString(IntPtr str);
            [DllImport(DirPath, EntryPoint = "covid_19", CallingConvention = CallingConvention.Cdecl)]
            public static extern StringHandle Decrypt(IntPtr data_ptr, int data_size, string localStatePath);
        }
    }
}
