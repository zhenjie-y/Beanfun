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

namespace Beanfun.ViewModels
{
    public partial class LoginViewModel(IDialogService dialogService, IEnumerable<ILoginService> loginServices) : ObservableObject
    {
        private readonly IDialogService dialogService = dialogService;
        private readonly IDictionary<string, ILoginService> services = loginServices.ToDictionary(service => service.GetType().Name);

        private string? username;
        private string? password;

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

        [RelayCommand]
        public async Task LoginAsync(XamlRoot xamlRoot)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            if (!services.TryGetValue("AccountLoginService", out var service))
            {
                return;
            }

            LoginResult loginResult = await service.LoginAsync(loginRequest);

            if (loginResult.IsSuccess)
            {
                await dialogService.ShowMessageAsync("登入", "登入成功", xamlRoot);

            }
            else
            {
                await dialogService.ShowMessageAsync("登入", loginResult.ErrorMessage ?? string.Empty, xamlRoot);
            }
        }
    }
}
