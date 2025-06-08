using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Beanfun.Interfaces;
using Microsoft.UI.Xaml;
using Beanfun.Models;
using Windows.Media.Protection;
using Beanfun.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Beanfun.ViewModels
{
    public partial class LoginViewModel(IDialogService dialogService, 
                                        AccountLoginService accountLoginService,
                                        QRCodeLoginService qrcodeLoginService) : ObservableObject
    {
        private readonly IDialogService dialogService = dialogService;
        private readonly AccountLoginService accountLoginService = accountLoginService;
        private readonly QRCodeLoginService qrcodeLoginService = qrcodeLoginService;

        private static readonly BitmapImage defaultQRCodeImage = new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"));

        private string? username;
        private string? password;
        private BitmapImage? qrCodeImage;

        public string Username
        {
            get => username ?? string.Empty;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password ?? string.Empty;
            set => SetProperty(ref password, value);
        }

        public BitmapImage QRCodeImage
        {
            get => qrCodeImage ?? defaultQRCodeImage;
            set => SetProperty(ref qrCodeImage, value);
        }

        [RelayCommand]
        public async Task LoginAsync(XamlRoot xamlRoot)
        {
            var account = new AccountLoginRequest
            {
                Username = username,
                Password = password
            };

            LoginResult loginResult = await accountLoginService.LoginAsync(account);

            if (loginResult.IsSuccess)
            {
                await dialogService.ShowMessageAsync("登入", "登入成功", xamlRoot);
            }
            else
            {
                await dialogService.ShowMessageAsync("登入", loginResult.ErrorMessage ?? string.Empty, xamlRoot);
            }
        }

        public async void OnNavigatedToAsync()
        {
            LoginResult loginResult = await qrcodeLoginService.GetQRCodeAsync();

            if (!loginResult.IsSuccess)
            {
                return;
            }

            QRCodeImage = loginResult.QrCodeImage ?? defaultQRCodeImage;
        }
    }
}
