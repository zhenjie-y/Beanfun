using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beanfun.Models;

namespace Beanfun.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResult> LoginAsync(AccountLoginModel loginModel);
    }
}
