using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beanfun.Interfaces;
using Beanfun.Models;
using Windows.Security.Authentication.Web.Core;

namespace Beanfun.Services
{
    partial class AccountLoginService : ILoginService
    {
        private readonly HttpClientHandler? handler;
        private readonly HttpClient? httpClient;
        private readonly CookieContainer cookieContainer = new ();

        private string SessionKey { get; set; } = string.Empty;
        private string LoginHtml { get; set; } = string.Empty;
        private string AuthKey { get; set; } = string.Empty;
        private string WebToken { get; set; } = string.Empty;

        [GeneratedRegex("skey=(.*)&display")]
        private static partial Regex SessionKeyRegex();

        [GeneratedRegex("id=\"__VIEWSTATE\" value=\"(.*)\" />")]
        private static partial Regex ViewStateRegex();

        [GeneratedRegex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />")]
        private static partial Regex EventValidationRegex();

        [GeneratedRegex("id=\"__VIEWSTATEGENERATOR\" value=\"(.*)\" />")]
        private static partial Regex ViewStateGeneratorRegex();

        [GeneratedRegex("akey=(.*)")]
        private static partial Regex AuthKeyRegex();

        [GeneratedRegex("<script type=\"text/javascript\">\\$\\(function\\(\\){MsgBox.Show\\('(.*)'\\);}\\);</script>")]
        private static partial Regex MessageBoxShowRegex();

        [GeneratedRegex("pollRequest\\(\"([^\"]*)\",\"(\\w+)\",\"([^\"]+)\"\\);")]
        private static partial Regex PollRequestRegex();

        public AccountLoginService()
        {
            handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true
            };

            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
        }

        public async Task<LoginResult> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                LoginResult loginResult = await GetSessionKeyAsync();

                if (!loginResult.IsSuccess)
                {
                    return loginResult;
                }

                loginResult = await GetLoginHtmlAsync();

                if (!loginResult.IsSuccess)
                {
                    return loginResult;
                }

                loginResult = await GetAuthKeyAsync(loginRequest);

                if (!loginResult.IsSuccess)
                {
                    return loginResult;
                }

                loginResult = await GetWebTokenAsync();

                if (!loginResult.IsSuccess)
                {
                    return loginResult;
                }

                loginResult.WebToken = WebToken;

                return loginResult;

            }
            catch (Exception ex)
            {
                return new LoginResult() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<LoginResult> GetSessionKeyAsync()
        {
            try
            {
                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                var requestUri = "https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0";

                var response = await httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get redirectUri response failed!" };
                }

                var redirectUri = response.RequestMessage?.RequestUri?.ToString();

                if (string.IsNullOrEmpty(redirectUri))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get redirectUri failed!" };
                }

                if (!SessionKeyRegex().IsMatch(redirectUri))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get sessionKey failed!" };
                }

                SessionKey = SessionKeyRegex().Match(redirectUri).Groups[1].Value;

                return new LoginResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<LoginResult> GetLoginHtmlAsync()
        {
            try
            { 
                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                var requestUri = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={SessionKey}";

                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get LoginHtml Response Failed!" };
                }

                LoginHtml = await response.Content.ReadAsStringAsync();

                return new LoginResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = false, ErrorMessage= ex.Message };
            }
        }

        private async Task<LoginResult> GetAuthKeyAsync(LoginRequest loginRequest)
        {
            if (!ViewStateRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "ViewState not Match!" };
            }

            if (!EventValidationRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "EventValidation not Match!" };
            }

            if (!ViewStateGeneratorRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "ViewStateGenerator not Match!" };
            }

            string viewState = ViewStateRegex().Match(LoginHtml).Groups[1].Value;
            string eventValidation = EventValidationRegex().Match(LoginHtml).Groups[1].Value;
            string viewStateGenerator = ViewStateGeneratorRegex().Match(LoginHtml).Groups[1].Value;

            var payload = new Dictionary<string, string>
            {
                { "__EVENTTARGET", "" },
                { "__EVENTARGUMENT", "" },
                { "__VIEWSTATE", viewState },
                { "__VIEWSTATEGENERATOR", viewStateGenerator },
                { "__EVENTVALIDATION", eventValidation },
                { "t_AccountID", loginRequest.Username ?? string.Empty },
                { "t_Password", loginRequest.Password ?? string.Empty },
                { "btn_login", "登入" }
            };

            var content = new FormUrlEncodedContent(payload);

            try
            {
                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                var loginUrl = $"https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey={SessionKey}";

                var response = await httpClient.PostAsync(loginUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get login response failed!" };
                }

                var requestUri = response.RequestMessage?.RequestUri?.ToString();

                if (string.IsNullOrEmpty(requestUri))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get login redirect uri failed!" };
                }

                if (!AuthKeyRegex().IsMatch(requestUri))
                {
                    string errorMessage = "Unknown Error!";

                    var redirectHtml = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(redirectHtml))
                    {
                        return new LoginResult { IsSuccess = false, ErrorMessage = "Get redirect html failed!" };
                    }

                    if (MessageBoxShowRegex().IsMatch(redirectHtml))
                    {
                        return new LoginResult
                        {
                            IsSuccess = false,
                            ErrorMessage = MessageBoxShowRegex().Match(redirectHtml).Groups[1].Value.Replace("[br /]", Environment.NewLine)
                        };
                    }
                    else if (PollRequestRegex().IsMatch(redirectHtml))
                    {
                        errorMessage = $"{PollRequestRegex().Match(redirectHtml).Groups[1].Value}\",\"{PollRequestRegex().Match(redirectHtml).Groups[3].Value}";
                        var loginToken = PollRequestRegex().Match(redirectHtml).Groups[2].Value;
                    }

                    return new LoginResult { IsSuccess = false, ErrorMessage = errorMessage };
                }

                AuthKey = AuthKeyRegex().Match(requestUri).Groups[1].Value;

                return new LoginResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = true, ErrorMessage = ex.Message };
            }
        }

        private async Task<LoginResult> GetWebTokenAsync()
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    { "SessionKey", SessionKey },
                    { "AuthKey", AuthKey },
                    { "ServiceCode", "" },
                    { "ServiceRegion", "" },
                    { "ServiceAccountSN", "0" }
                };

                var content = new FormUrlEncodedContent(payload);

                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                var response = await httpClient.PostAsync("https://tw.beanfun.com/beanfun_block/bflogin/return.aspx", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get login response failed!" };
                }

                var loginHtml = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(loginHtml))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get login html failed!" };
                }

                var location = response.Headers.Location?.ToString();

                var redirectUrl = "https://tw.beanfun.com/" + location;

                var redirectResponse = await httpClient.GetAsync(redirectUrl);

                if (!redirectResponse.IsSuccessStatusCode)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get redirect response failed!" };
                }

                var redirectHtml = await redirectResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(redirectHtml))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get redirect html failed!" };
                }

                var cookieCollection = cookieContainer.GetCookies(new Uri("https://tw.beanfun.com"));

                var cookie = cookieCollection.Cast<Cookie>().FirstOrDefault(cookie => cookie.Name == "bfWebToken");

                if (cookie is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get web token failed!" };
                }

                WebToken = cookie.Value;

                return new LoginResult { IsSuccess = true };
            }
            catch(Exception ex)
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}
