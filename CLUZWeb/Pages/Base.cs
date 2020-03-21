using System;
using CLUZWeb.Services;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public enum InfoType
    {
        Success,
        Info,
        Warn,
        Error
    }
    public class Base : ComponentBase
    {
        [Inject] public GamePoolService GamePool { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<IdentityUser> UserManager { get; set; }
        [Inject] protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }
        //[Inject] public ILogger Logger { get; set; }

        public Guid GetCurrentUserGuid()
        {
            return Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User));
        }
        public string GetCurrentUserName()
        {
            return AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User.Identity.Name;
        }
        public void ShowInfo(string header, string body, InfoType type)
        {
            switch (type)
            {
                case InfoType.Success:
                    Toaster.Success(body, header);
                    break;
                case InfoType.Info:
                    Toaster.Info(body, header);
                    break;
                case InfoType.Warn:
                    Toaster.Warning(body, header);
                    break;
                case InfoType.Error:
                    Toaster.Error(body, header);
                    break;
            }
                
        }
    }
}
