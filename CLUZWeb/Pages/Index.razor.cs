using CLUZWeb.Data;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class Index
    {
        private IEnumerable<Game> games;

        protected override void OnInitialized()
        {
            try
            {
                games = _gamePool.Games.Values;

                _gamePool.PropertyChanged += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch (KeyNotFoundException)
            {
                new List<Game>();
            }

            
        }

        private void Create()
        {
            NavigationManager.NavigateTo("/create");
        }

        private void Join(Game game)
        {
            if (!game.PlayerInGame(_auth.GetAuthenticationStateAsync().Result.User.Identity))
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
