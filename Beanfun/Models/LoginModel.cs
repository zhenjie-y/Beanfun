using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beanfun.Models
{
    public class LoginResult
    {
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    public class AccountLoginModel
    {
        private string username = string.Empty;

        public string Username
        {
            get => username;
            set => username = value;
        }

        private string password = string.Empty;

        public string Password
        { 
            get => password;
            set => password = value;
        }
    }
}
