using System;
using CLUZWeb.Services;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public class Base : ComponentBase
    {
        [Inject] public GamePoolService GamePool { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<IdentityUser> UserManager { get; set; }
        [Inject] public ToastQueueService ToastService { get; set; }
        [Inject] protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }
        //[Inject] public ILogger Logger { get; set; }

        public string StatusMessage;
        public string StatusClass;
        public async Task AlertMessage(string alertClass, string alertMesage)
        {
            StatusClass = alertClass;
            StatusMessage = alertMesage;
            await Task.Delay(3000);
            StatusClass = null;
            StatusMessage = null;
            StateHasChanged();
        }

        public Guid GetCurrentUserGuid()
        {
            return Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User));
        }

        public string GetCurrentUserName()
        {
            return AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User.Identity.Name;
        }
    }
}
