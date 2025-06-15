using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Beanfun.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Beanfun.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page, IDisposable
    {
        public LoginViewModel? ViewModel { get; } = App.Services?.GetRequiredService<LoginViewModel>();

        public LoginPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel is not LoginViewModel viewModel)
            {
                return;
            }

            viewModel.Password = ((PasswordBox)sender).Password.ToString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel is not LoginViewModel loginViewModel)
            {
                return;
            }

            loginViewModel.OnNavigatedToAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (this.Content is not IDisposable disposable)
            {
                return;
            }

            disposable.Dispose();
        }
    }
}
