using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EncEnc
{
    public abstract class BindableBase : INotifyPropertyChanged, IDataErrorInfo
    {
        #region プロパティの変更通知

        /// <summary>
        /// プロパティ変更時に発生するイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更イベントを発生させます
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 指定フィールドに値を設定し、プロパティの変更を通知します。
        /// プロパティが既に設定したい値と一致している場合は、新たに設定されません。
        /// </summary>
        /// <typeparam name="T">フィールドの型</typeparam>
        /// <param name="field">値を設定するフィールド</param>
        /// <param name="value">設定したい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>値が変更された場合はtrue、既存の値が設定したい値と一致している場合はfalseを返します</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        #endregion

        #region エラー情報の通知

        /// <summary>
        /// プロパティ毎のエラーメッセージを保持します。
        /// </summary>
        private Dictionary<string, string> _errorMessages = new Dictionary<string, string>();

        /// <summary>
        /// オブジェクトに関する間違いを示すエラー メッセージを取得します。
        /// </summary>
        public string Error
        {
            get { return HasErrors ? "Has Error" : null; }
        }

        /// <summary>
        /// 指定した名前のプロパティに関するエラー メッセージを取得します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                string errorMessage;
                if (_errorMessages.TryGetValue(propertyName, out errorMessage))
                {
                    return errorMessage;
                }

                return null;
            }
        }

        /// <summary>
        /// エラーが存在するかどうかを取得します。
        /// </summary>
        protected bool HasErrors
        {
            get { return this._errorMessages.Any(); }
        }

        /// <summary>
        /// 指定した名前のプロパティに関するエラー メッセージを設定します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="errorMessage"></param>
        protected void SetError(string propertyName, string errorMessage)
        {
            this._errorMessages[propertyName] = errorMessage;
        }

        /// <summary>
        /// 指定した名前のプロパティに関するエラー メッセージをクリアします。
        /// </summary>
        /// <param name="propertyName"></param>
        protected void ClearError(string propertyName)
        {
            if (_errorMessages.ContainsKey(propertyName))
            {
                _errorMessages.Remove(propertyName);
            }
        }

        #endregion
    }
}
