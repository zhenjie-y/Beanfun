using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace Beanfun.Interfaces
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string title, string content, XamlRoot xamlRoot);
    }
}
