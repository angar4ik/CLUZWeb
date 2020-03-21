using System;
using System.Collections.Generic;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace CLUZWeb.Pages
{
    public partial class Index
    {
        private IEnumerable<Game> games;

        protected override void OnInitialized()
        {
            try
            {
                games = GamePool.Games.Values;

                GamePool.GamePoolEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch
            {
                games = new List<Game>();
            }
        }

        private async void Create()
        {
            NavigationManager.NavigateTo("/create");
        }

        private void Join(Game game)
        {
            if (!game.PlayerInGame(GetCurrentUserGuid()))
            {
                NavigationManager.NavigateTo($"/join/{game.Guid}");
            }
            else
            {
                NavigationManager.NavigateTo($"/gameroom/{game.Guid}");
            }
        }
    }
}
