using CLUZWeb.Models;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace CLUZWeb.Pages
{
    public partial class GameRoom
    {
        [Inject] GamePoolService GamePool { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] UserManager<IdentityUser> UserManager { get; set; }

        [Parameter]
        public Guid Guid { get; set; }
        IEnumerable<Player> players;
        Game g;
        private string StatusMessage;
        private string StatusClass;

        protected override void OnInitialized()
        {
            try
            {
                g = GamePool.Games[Guid];
                players = g.Players.Values;

                GamePool.Games[Guid].GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch (KeyNotFoundException)
            {
                players = new List<Player>();
            }
        }
        private void Start()
        {
            if(g.Status == GameState.Filled)
            {
                g.Status = GameState.Locked;
            }
            else if(g.Status == GameState.Unfilled)
            {
                StatusClass = "alert-danger";
                StatusMessage = "Minimum 4 players needed";
            }
        }

        private void Leave()
        {
            g.RemovePlayer(Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User)));
            NavigationManager.NavigateTo("/");
        }
    }
}
