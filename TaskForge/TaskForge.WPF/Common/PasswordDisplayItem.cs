using System;
using System.ComponentModel;
using TaskForge.Domain.Entities;

namespace TaskForge.WPF.Common
{
    public class PasswordDisplayItem : INotifyPropertyChanged
    {
        private bool _isRevealed;

        public Password? Password { get; set; }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                if (_isRevealed != value)
                {
                    _isRevealed = value;
                    OnPropertyChanged(nameof(IsRevealed));
                    OnPropertyChanged(nameof(DisplayedPassword));
                }
            }
        }

        public string DisplayTitle => !string.IsNullOrEmpty(Password?.Url) ?
            GetDomainFromUrl(Password.Url) :
            (!string.IsNullOrEmpty(Password?.Login) ? Password.Login : "No Title");

        public string DecryptedPassword => DecryptPassword(Password?.PasswordEncrypted ?? "");

        public string DisplayedPassword => IsRevealed ? DecryptedPassword : new string('●', DecryptedPassword.Length);

        private string GetDomainFromUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                return url;
            }
        }

        private string DecryptPassword(string encryptedText)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                return System.Text.Encoding.UTF8.GetString(encryptedBytes);
            }
            catch (FormatException)
            {
                return encryptedText;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
