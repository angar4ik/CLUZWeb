using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class GameRoom
    {
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
                g = _gamePool.Games[Guid];
                players = g.Players.Values;

                _gamePool.Games[Guid].GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
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
            g.RemovePlayer(_auth.GetAuthenticationStateAsync().Result.User.Identity);
            NavigationManager.NavigateTo("/");
        }
    }
}
