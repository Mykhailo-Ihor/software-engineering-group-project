using System.ComponentModel;

namespace TaskForge.WPF.Common
{
    public class PasswordDisplayItem : INotifyPropertyChanged
    {
        private bool _isRevealed;

        public TaskForge.Domain.Entities.Password? Password { get; set; }

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

        public string DisplayedPassword => this.IsRevealed && this.Password != null
            ? this.Password.PasswordEncrypted
            : new string('•', this.Password?.PasswordEncrypted?.Length ?? 0);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
