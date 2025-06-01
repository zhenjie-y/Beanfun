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

namespace Beanfun.ViewModels
{
    public partial class LoginPageViewModel(IDialogService dialogService) : ObservableObject
    {
        private string account = string.Empty;

        public string Account
        {
            get => account;
            set => SetProperty(ref account, value);
        }

        private string password = string.Empty;

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private readonly IDialogService dialogService = dialogService;

        [RelayCommand]
        public async Task LoginAsync(XamlRoot xamlRoot)
        {
            bool isLoginSuccess = false;

            if (!isLoginSuccess)
            {
                await dialogService.ShowMessageAsync("登入失敗", "帳號或密碼錯誤", xamlRoot);
            }
        }
    }
}
