using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ABI.Windows.ApplicationModel.Activation;
using Beanfun.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;
//using Windows.Web.Http;

namespace Beanfun.Services
{
    public partial class QRCodeLoginService
    {
        private readonly HttpClientHandler? handler;
        private readonly HttpClient? httpClient;
        private readonly CookieContainer cookieContainer = new();

        private string SessionKey { get; set; } = string.Empty;
        private string LoginHtml { get; set; } = string.Empty;
        private string EncryptData { get; set; } = string.Empty;
        private string ViewState { get; set; } = string.Empty;
        private string EventValidation { get; set; } = string.Empty;
        private string QRCodeImageUrl { get; set; } = string.Empty;

        [GeneratedRegex("skey=(.*)&display")]
        private static partial Regex SessionKeyRegex();

        [GeneratedRegex("id=\"__VIEWSTATE\" value=\"(.*)\" />")]
        private static partial Regex ViewStateRegex();

        [GeneratedRegex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />")]
        private static partial Regex EventValidationRegex();

        [GeneratedRegex("\\$\\(\"#theQrCodeImg\"\\).attr\\(\"src\", \"../(.*)\" \\+ obj.strEncryptData\\);")]
        private static partial Regex QRCodeImageUrlRegex();

        public QRCodeLoginService()
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

        public async Task<LoginResult> GetQRCodeAsync()
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

            loginResult = await GetQRCodeValueAsync();

            if (!loginResult.IsSuccess)
            {
                return loginResult;
            }

            loginResult = await GetQRCodeImage();

            return loginResult;
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

                var requestUri = $"https://tw.newlogin.beanfun.com/login/qr_form.aspx?skey={SessionKey}";

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
                return new LoginResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<LoginResult> GetQRCodeValueAsync()
        {
            if (!ViewStateRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Get view state error!" };
            }

            if (!EventValidationRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Get event validation error!" };
            }

            if (!QRCodeImageUrlRegex().IsMatch(LoginHtml))
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = "Get QRCode image url error!" };
            }

            ViewState = ViewStateRegex().Match(LoginHtml).Groups[1].Value;
            EventValidation = EventValidationRegex().Match(LoginHtml).Groups[1].Value;
            QRCodeImageUrl = "https://tw.newlogin.beanfun.com/" + QRCodeImageUrlRegex().Match(LoginHtml).Groups[1].Value;

            return await GetEncryptDataAsync();
        }

        private async Task<LoginResult> GetEncryptDataAsync()
        {
            try
            {
                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                var url = $"https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey={SessionKey}";

                var response = await httpClient.GetStringAsync(url);

                if (string.IsNullOrEmpty(response))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "Get QRCode data response error!" };
                }

                JObject jsonData = JObject.Parse(response);

                if (int.TryParse(jsonData["intResult"]?.ToString(), out int result) is false || result != 1)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "QRCode login result error!" };
                }

                var encryptData = jsonData["strEncryptData"]?.ToString();

                if (string.IsNullOrEmpty(encryptData))
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "EncryptData error!" };
                }

                EncryptData = encryptData;

                return new LoginResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<LoginResult> GetQRCodeImage()
        {
            try
            {
                if (httpClient is null)
                {
                    return new LoginResult { IsSuccess = false, ErrorMessage = "HttpClient Error!" };
                }

                byte[] buffer = await httpClient.GetByteArrayAsync(QRCodeImageUrl);

                var stream = new InMemoryRandomAccessStream();

                await using var writer = stream.AsStreamForWrite();

                //await writer.WriteAsync(buffer, 0, buffer.Length);
                await writer.WriteAsync(buffer);
                await writer.FlushAsync();

                stream.Seek(0);

                var qrcodeImage = new BitmapImage();

                await qrcodeImage.SetSourceAsync(stream);

                return new LoginResult { IsSuccess = true, QrCodeImage = qrcodeImage };

            }
            catch (Exception ex)
            {
                return new LoginResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}
