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
            await Task.Yield();
        }

        protected async override Task<List<Cookie>> ReadCookiesFromDiskAsync()
        {
            await Task.Yield();
            return null;
        }

        protected override async Task WriteCookiesToDiskAsync(List<Cookie> cookieJar)
        {
            await Task.Yield();
        }
    }
}
