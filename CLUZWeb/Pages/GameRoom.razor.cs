using System;
using System.Collections.Generic;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;

namespace CLUZWeb.Pages
{
    public partial class GameRoom
    {
        //cames from page parameter
        [Parameter]
        public Guid Guid { get; set; }

        IEnumerable<Player> _players;
        Game _game;
        Player _player;
        protected override void OnInitialized()
        {
            try
            {
                _game = GamePool.Games[Guid];
                _players = _game.Players.Values;
                _player = _game.Players[GetCurrentUserGuid()];
                GamePool.Games[Guid].GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch (KeyNotFoundException)
            {
                _players = new List<Player>();
                _game = new Game("", "", Guid.NewGuid());
            }
        }

        private void Ready(Player player)
        {
            player.State = PlayerState.Ready;
        }

        private void Action(Player player)
        {

        }

        private async void Start(Game g)
        {
            if(g.Status == GameState.Filled)
            {
                g.Status = GameState.Locked;
                await AlertMessage("alert-info", "Game has started");
            }
            else if(g.Status == GameState.Unfilled)
            {
                await AlertMessage("alert-danger", "Minimum 4 players needed");
            }
        }

        private void Leave(Game g)
        {
            g.RemovePlayer(_player.Guid);
            NavigationManager.NavigateTo("/");
        }
    }
}
