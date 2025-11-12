using TaskForge.Domain.Entities;

namespace TaskForge.WPF.Common
{
    public class PasswordDisplayItem
    {
        public TaskForge.Domain.Entities.Password? Password { get; set; }

        public bool IsRevealed { get; set; }

        public string DisplayedPassword => this.IsRevealed && this.Password != null
            ? this.Password.PasswordEncrypted
            : new string('•', this.Password?.PasswordEncrypted?.Length ?? 0);
    }
}
