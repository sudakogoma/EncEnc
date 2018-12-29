using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace EncEnc
{
    public class MainFormViewModel : BindableBase
    {
        #region CanClickRunButton 変更通知プロパティ

        public bool CanClickRunButton
        {
            get
            {
                return !String.IsNullOrEmpty(this.FilePath)
                    && !String.IsNullOrEmpty(this.Password);
            }
        }

        #endregion

        #region FilePath 変更通知プロパティ

        private string _filePath = String.Empty;

        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                if (SetProperty(ref this._filePath, value, nameof(FilePath)))
                {
                    OnPropertyChanged(nameof(CanClickRunButton));
                }
            }
        }

        #endregion

        #region Password 変更通知プロパティ

        private string _password = String.Empty;

        public string Password
        {
            get { return this._password; }
            set
            {
                if (SetProperty(ref this._password, value, nameof(Password)))
                {
                    OnPropertyChanged(nameof(CanClickRunButton));
                }
            }
        }

        #endregion

        public void SelectFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.FilePath = dialog.FileName;
                }
            }
        }

        public void Encrypt()
        {
            const int SALT_SIZE = 16;
            const int KEY_SIZE = 16;

            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(this.FilePath),
                $"{Path.GetFileNameWithoutExtension(this.FilePath)}.dat");

            using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var aes = new AesManaged())
            {
                aes.BlockSize = 128;
                aes.KeySize = KEY_SIZE * 8;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var deriveBytes = new Rfc2898DeriveBytes(this.Password, SALT_SIZE);
                aes.Key = deriveBytes.GetBytes(KEY_SIZE);
                aes.GenerateIV();

                outputStream.Write(deriveBytes.Salt, 0, SALT_SIZE);
                outputStream.Write(aes.IV, 0, 16);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                using (var deflateStream = new DeflateStream(cryptoStream, CompressionMode.Compress))
                using (var fs = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
                {
                    int len;
                    var buffer = new byte[4096];
                    while ((len = fs.Read(buffer, 0, 4096)) > 0)
                    {
                        deflateStream.Write(buffer, 0, len);
                    }
                }
            }
        }

        public void Decrypt()
        {
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(this.FilePath),
                Path.GetFileNameWithoutExtension(this.FilePath));

            using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var fs = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
            using (var aes = new AesManaged())
            {
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var salt = new byte[16];
                fs.Read(salt, 0, 16);

                // Initilization Vector
                byte[] iv = new byte[16];
                fs.Read(iv, 0, 16);
                aes.IV = iv;

                var deriveBytes = new Rfc2898DeriveBytes(this.Password, salt);
                byte[] bufferKey = deriveBytes.GetBytes(16);
                aes.Key = bufferKey;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var cryptoStream = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                {
                    using (var defaultStream = new DeflateStream(cryptoStream, CompressionMode.Decompress))
                    {
                        int len;
                        var buffer = new byte[4096];
                        while ((len = defaultStream.Read(buffer, 0, 4096)) > 0)
                        {
                            outputStream.Write(buffer, 0, len);
                        }
                    }
                }
            }
        }
    }
}
