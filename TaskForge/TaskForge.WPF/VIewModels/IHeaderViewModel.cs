using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TaskForge.WPF.ViewModels
{
    public interface IHeaderViewModel
    {
        BitmapImage UserAvatar { get; }
        ICommand ProfileCommand { get; }
        ICommand LogoutCommand { get; }
        bool IsUserInfoVisible { get; }
    }
}