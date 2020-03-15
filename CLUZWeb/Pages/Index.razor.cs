using CLUZWeb.Data;
using CLUZWeb.Models;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class Index : ComponentBase
    {
        private IEnumerable<Game> games;
        [Inject] GamePoolService GamePool { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] UserManager<IdentityUser> UserManager { get; set; }

        protected override void OnInitialized()
        {
            try
            {
                games = GamePool.Games.Values;

                GamePool.PropertyChanged += async (o, e) => await InvokeAsync(() => StateHasChanged());
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
            if (!game.PlayerInGame(Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User))))
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
