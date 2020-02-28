using NUnit.Framework;
using ryu_s.BrowserCookie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserCookieImplementationsTests
{
    [TestFixture]
    class ChromeAesGcmTests
    {
        [Test]
        public void DecryptEncryptedKeyTest()
        {
            var encryptedKey = "RFBBUEkBAAAA0Iyd3wEV0RGMegDAT8KX6wEAAADrTIpDJDARRLAtM249pl4eAAAAAAIAAAAAABBmAAAAAQAAIAAAAEuJ9ZY1v9SIB4q1WHM/4Sjj4Tise7Bg1Fr1iEVMRP78AAAAAA6AAAAAAgAAIAAAABQqd2SqFtW25iItjpVKj8IgXE4kyud86ra+EULyhk6DMAAAABKh7D8Opd1PT/wh2XguFb/cV2559HmwEuEuIS+wA49S3mSw90hnie6zxoWLz678G0AAAABozRsDCHuI/+dSVGKT+Ft2bUczJozczEL7r+FOkdL5OPXZwODGOOymlWthJN4UYhRp4DdydMBjKDSpNT1Pr2/w";
            var key = ChromeAesGcm.DecryptEncryptedKey(encryptedKey);
            Assert.AreEqual(new byte[] { 209, 50, 133, 14, 3, 140, 15, 207, 240, 31, 130, 120, 153, 200, 240, 51, 220, 160, 201, 61, 65, 89, 128, 75, 142, 37, 64, 29, 53, 59, 53, 85 }, key);
        }
        [Test]
        public void Test()
        {
            var data = new byte[]{
                0xf6, 0xed, 0x46, 0x55, 0xf0, 0xad, 0x6a, 0x89, 0x75, 0x98, 0xf2,
                0xbd, 0xee, 0x62, 0x2c, 0x87, 0x76, 0xa8, 0xdb, 0x70, 0xe3, 0x67,
                0x8a, 0xb0, 0x10, 0x4f, 0x21, 0x1b, 0xfd, 0xe1, 0xaf, 0xb5,
            };
            var key = new byte[] { 209, 50, 133, 14, 3, 140, 15, 207, 240, 31, 130, 120, 153, 200, 240, 51, 220, 160, 201, 61, 65, 89, 128, 75, 142, 37, 64, 29, 53, 59, 53, 85 };
            var nonce = new byte[] { 0xA8, 0xCF, 0xDD, 0xE9, 0x34, 0x97, 0xAD, 0xC3, 0x7A, 0x0D, 0xDF, 0x19 };
            var bytes = ChromeAesGcm.Decrypt(data, key, nonce);
            var decrypted = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual("2OGPPK7S6DE42BIA", decrypted);
        }
    }
}
