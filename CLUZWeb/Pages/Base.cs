﻿using System;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

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

        //public binary[] GetProfilePicture()
        //{
        //    if (user.HasClaim(c => c.Type == "urn:google:picture"))
        //    {
        //        string googlePicUrl = user.FindFirst("urn:google:picture").Value;
        //    }
        //}

        //urn:facebook:id ->
        //https://graph.facebook.com/3177970262245867/picture?type=normal

        public string GetCurrentUserName()
        {
            var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;

            if (user.HasClaim(c => c.Type == "urn:facebook:name"))
            {
                //Console.WriteLine(user.FindFirst("urn:facebook:email").Value);
                return user.FindFirst("urn:facebook:name").Value;
            }
            else if (user.HasClaim(c => c.Type == "urn:google:name"))
            {
                //Console.WriteLine(user.FindFirst("urn:google:email").Value);
                return user.FindFirst("urn:google:name").Value;
            }
            else
            {
                return AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User.Identity.Name;
            }

            //string? pic = ((ClaimsIdentity)user.Identity).FindFirst("urn:google:picture").Value;

            //Claim? name = ((ClaimsIdentity)user.Identity).FindFirst("Name"); 
        }
        public void ShowInfo(string title, string body, InfoType type)
        {
            switch (type)
            {
                case InfoType.Success:
                    Toaster.Success(body, title);
                    break;
                case InfoType.Info:
                    Toaster.Info(body, title);
                    break;
                case InfoType.Warn:
                    Toaster.Warning(body, title);
                    break;
                case InfoType.Error:
                    Toaster.Error(body, title);
                    break;
            }
                
        }
    }
}
