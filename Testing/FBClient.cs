using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Testing
{
    public class FBClient : MessengerClient
    {
        protected async override Task DeleteCookiesAsync()
        {
            System.IO.File.Delete(Helpers.CookiesFile);
        }

        protected async override Task<List<Cookie>> ReadCookiesFromDiskAsync()
        {
            return Helpers.DeSerializeObject<List<Cookie>>(Helpers.CookiesFile);
        }

        protected override async Task WriteCookiesToDiskAsync(List<Cookie> cookieJar)
        {
            Helpers.SerializeObject<List<Cookie>>(cookieJar, Helpers.CookiesFile);
        }
    }
}
