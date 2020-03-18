using System;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CLUZWeb.Pages
{
    public partial class Create
    {
        private CreateGame _createGameModel = new CreateGame();
        private async void HandleValidSubmit()
        {
            if (GamePool.GameExists(_createGameModel.Name))
            {
                await AlertMessage("alert-danger", "Game exists");
            }
            else
            {
                GamePool.AddGame(_createGameModel.Name, _createGameModel.Password, GetCurrentUserGuid());
                NavigationManager.NavigateTo("/");
            }
        }
    }
}
