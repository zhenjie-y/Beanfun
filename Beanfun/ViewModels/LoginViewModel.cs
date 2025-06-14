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
using WinRT.Interop;

namespace Beanfun.ViewModels
{
    public partial class LoginViewModel : ObservableObject, IDisposable
    {
        private readonly IDialogService dialogService;
        private readonly AccountLoginService accountLoginService;
        private readonly QRCodeLoginService qrcodeLoginService;

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

        public LoginViewModel(IDialogService dialogService,
                              AccountLoginService accountLoginService,
                              QRCodeLoginService qrcodeLoginService)
        {
            this.dialogService = dialogService;
            this.accountLoginService = accountLoginService;
            this.qrcodeLoginService = qrcodeLoginService;

            qrcodeLoginService.LoginSuccessEvent += OnLoginSuccess;
            qrcodeLoginService.TokenExpired += OnTokenExpiredAsync;
        }

        public void Dispose()
        {
            qrcodeLoginService.EndPolling();

            GC.SuppressFinalize(this);
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
            await InitializeQRCodeAsync();
        }

        private async Task InitializeQRCodeAsync()
        {
            LoginResult loginResult = await qrcodeLoginService.GetQRCodeAsync();

            if (!loginResult.IsSuccess)
            {
                return;
            }

            QRCodeImage = loginResult.QrCodeImage ?? defaultQRCodeImage;

            if (QRCodeImage == defaultQRCodeImage)
            {
                return;
            }

            qrcodeLoginService.StartPolling();
        }

        private void OnLoginSuccess(object? sender, EventArgs e)
        {

        }

        private async void OnTokenExpiredAsync(object? sender, EventArgs e)
        {
            await InitializeQRCodeAsync();
        }
    }
}
