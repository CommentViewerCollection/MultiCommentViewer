using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using Org.BouncyCastle.X509;
using System.Runtime.InteropServices;

namespace ryu_s.BrowserCookie
{
    class ChromeAesGcm
    {
        public string LocalStatePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\" + "Local State";
        private void LoadKey()
        {
            var encryptedKey = GetEncryptedKey(LocalStatePath);
            var key = DecryptEncryptedKey(encryptedKey);
            Key = key;
        }
        byte[] Key { get; set; }
        public string Decrypt(byte[] encrypted_value)
        {
            if (!IsAesGcmData(encrypted_value)) return null;
            var nonce = GetNonce(encrypted_value);
            if (Key == null)
            {
                LoadKey();
            }
            var data = encrypted_value.Skip(15).ToArray();
            var bytes = Decrypt(data, Key, nonce);
            return Encoding.UTF8.GetString(bytes);
        }
        private byte[] GetNonce(byte[] encrypted_value)
        {
            return encrypted_value.Skip(3).Take(12).ToArray();
        }
        private bool IsAesGcmData(byte[] encryptedData)
        {
            var data = encryptedData;
            return (data[0] == 'v' && data[1] == '1' && data[2] == '0');
        }
        public static byte[] DecryptEncryptedKey(string encryptedKey)
        {
            if (encryptedKey is null)
            {
                throw new ArgumentNullException(nameof(encryptedKey));
            }

            var base64Decoded = Base64.Decode(encryptedKey);
            var dpapiedKey = base64Decoded.Skip(5).ToArray();
            return NativeMethods.CryptUnprotectData(dpapiedKey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="statePath"></param>
        /// <returns></returns>
        private static string GetEncryptedKey(string statePath)
        {
            string content;
            using (var sr = new StreamReader(statePath))
            {
                content = sr.ReadToEnd();
            }

            var match = Regex.Match(content, "\"encrypted_key\":\"([^\"]+)\"");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        public static byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] nonce)
        {
            if (encryptedData is null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (nonce is null)
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            const int macSize = 128;

            using (var cipherStream = new MemoryStream(encryptedData))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), macSize, nonce);
                cipher.Init(false, parameters);

                var cipherText = cipherReader.ReadBytes(encryptedData.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                var outSize = cipher.GetOutputSize(cipherText.Length);

                var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                cipher.DoFinal(plainText, len);

                return plainText;
            }
        }
    }
}
