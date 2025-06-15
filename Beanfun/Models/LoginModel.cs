using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace Beanfun.Models
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? WebToken { get; set; }
        public BitmapImage? QrCodeImage { get; set; }
    }

    public class AccountLoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
