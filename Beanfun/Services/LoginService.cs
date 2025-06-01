using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beanfun.Interfaces;
using Beanfun.Models;

namespace Beanfun.Services
{
    class LoginService : ILoginService
    {
        public async Task<LoginResult> LoginAsync(AccountLoginModel loginModel)
        {
            await Task.Delay(500);

            if (string.IsNullOrEmpty(loginModel.Username))
            {
                return new LoginResult { Success = false, ErrorMessage = "帳號或密碼錯誤" };
            }

            if (string.IsNullOrEmpty(loginModel.Password))
            {
                return new LoginResult { Success = false, ErrorMessage = "帳號或密碼錯誤" };
            }

            return new LoginResult { Success = true };
        }
    }
}
