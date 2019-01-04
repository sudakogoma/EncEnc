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

        /// <summary>
        /// ファイル選択ダイアログを用いてファイルを選択します。
        /// </summary>
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

        /// <summary>
        /// 指定されたファイルを指定パスワードを用いて暗号化します。}
        /// </summary>
        public void Encrypt()
        {
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(this.FilePath),
                $"{Path.GetFileNameWithoutExtension(this.FilePath)}.enc");

            using (var memoryStream = new MemoryStream())
            using (var aes = new AesManaged())
            {
                var deriveBytes = new Rfc2898DeriveBytes(this.Password, Define.SALT_SIZE);
                aes.Key = deriveBytes.GetBytes(Define.KEY_SIZE);
                aes.GenerateIV();

                memoryStream.Write(deriveBytes.Salt, 0, deriveBytes.Salt.Length);
                memoryStream.Write(aes.IV, 0, aes.IV.Length);

                ICryptoTransform encryptor = aes.CreateEncryptor();

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (var deflateStream = new DeflateStream(cryptoStream, CompressionMode.Compress))
                using (var fileStream = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
                {
                    int len;
                    var buffer = new byte[4096];
                    while ((len = fileStream.Read(buffer, 0, 4096)) > 0)
                    {
                        deflateStream.Write(buffer, 0, len);
                    }
                }

                using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    streamWriter.Write(Convert.ToBase64String(memoryStream.ToArray()));
                }
            }
        }

        /// <summary>
        /// 指定されたファイルを指定パスワードを用いて復号化します。
        /// </summary>
        public void Decrypt()
        {
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(this.FilePath),
                Path.GetFileNameWithoutExtension(this.FilePath));

            using (var memoryStream = new MemoryStream())
            using (var aes = new AesManaged())
            {
                using (var fs = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                {
                    var x = Convert.FromBase64String(sr.ReadToEnd());
                    memoryStream.Write(x, 0, x.Length);
                    memoryStream.Position = 0;
                }

                var salt = new byte[Define.SALT_SIZE];
                memoryStream.Read(salt, 0, salt.Length);

                var deriveBytes = new Rfc2898DeriveBytes(this.Password, salt);
                aes.Key = deriveBytes.GetBytes(Define.KEY_SIZE);

                var iv = new byte[aes.IV.Length];
                memoryStream.Read(iv, 0, iv.Length);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor();

                using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var deflateStream = new DeflateStream(cryptoStream, CompressionMode.Decompress))
                {
                    int len;
                    var buffer = new byte[4096];
                    while ((len = deflateStream.Read(buffer, 0, 4096)) > 0)
                    {
                        outputStream.Write(buffer, 0, len);
                    }
                }
            }

        }
    }
}
