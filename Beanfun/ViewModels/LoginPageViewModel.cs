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

namespace Beanfun.ViewModels
{
    public partial class LoginPageViewModel(IDialogService dialogService, ILoginService loginService) : ObservableObject
    {
        public AccountLoginModel Account { get; set; } = new();

        private readonly IDialogService dialogService = dialogService;
        private readonly ILoginService loginService = loginService;

        [RelayCommand]
        public async Task LoginAsync(XamlRoot xamlRoot)
        {
            LoginResult loginResult = await loginService.LoginAsync(Account);

            if (loginResult.Success)
            {
                await dialogService.ShowMessageAsync("登入", "登入成功", xamlRoot);

            }
            else
            {
                if (loginResult.ErrorMessage != null)
                {
                    await dialogService.ShowMessageAsync("登入", loginResult.ErrorMessage, xamlRoot);
                }
            }
        }
    }
}
