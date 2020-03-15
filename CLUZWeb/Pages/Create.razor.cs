using CLUZWeb.Models;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class Create
    {
        [Inject] GamePoolService GamePool { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] UserManager<IdentityUser> UserManager { get; set; }

        private CreateGame _createGameModel = new CreateGame();
        
        private string StatusMessage;
        private string StatusClass;

        private void HandleValidSubmit()
        {
            if (GamePool.GameExists(_createGameModel.Name))
            {
                StatusClass = "alert-danger";
                StatusMessage = "Game exists";
            }
            else
            {
                GamePool.AddGame(_createGameModel.Name, _createGameModel.Password, Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User)));
                NavigationManager.NavigateTo("/");
            }


        }
    }
}
