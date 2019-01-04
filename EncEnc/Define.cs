using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncEnc
{
    public class Define
    {
        #region AES

        // AESの初期設定
        // BlockSize = 128;
        // KeySize = 256;
        // Mode = CipherMode.CBC
        // Padding = PaddingMode.PKCS7;

        public const int SALT_SIZE = 16;
        public const int KEY_SIZE = 16;

        #endregion
    }
}
