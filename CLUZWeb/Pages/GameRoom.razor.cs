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

        private IEnumerable<Player> _players;
        private Game _game;
        private Player _player;

        protected override void OnInitialized()
        {
            try
            {
                _game = GamePool.Games[Guid];
                _players = _game.Players.Values;
                _player = _game.Players[GetCurrentUserGuid()];
                GamePool.Games[Guid].GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
                GamePool.Games[Guid].PlayerPropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch (KeyNotFoundException)
            {
                _players = new List<Player>();
                _game = new Game("", "", Guid.NewGuid());
            }
        }

        private void Ready(Player p)
        {
            p.State = PlayerState.Ready;
        }

        private void Action(Player p)
        {
            //_player doing something with p
            if(GetActionBtnType() == GameAction.Vote)
            {
                _game.VoteRequest(_player, p);
                _player.State = PlayerState.Ready;
            }
            else if (GetActionBtnType() == GameAction.Kill)
            {
                _game.KillRequest(_player, p);
                _player.State = PlayerState.Ready;
            }
            else if (GetActionBtnType() == GameAction.Guess)
            {
                if (p.Role == PlayerRole.Mafia)
                    Console.WriteLine($"{p.Name} is mafia");
                _player.State = PlayerState.Ready;
            }
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

        private bool IsDisabledReadyBtn()
        {
            if (_player.State == PlayerState.Idle
                && _player.Role != PlayerRole.Mafia
                && _player.Role != PlayerRole.Police)
                return false;
            else
                return true;
        }

        private bool IsDisabledActionBtn()
        {
            if ((_player.Role == PlayerRole.Mafia || _player.Role == PlayerRole.Police)
                && _player.State != PlayerState.Ready)
                return false;
            else
                return true;
        }

        private GameAction GetActionBtnType()
        {
            if (_player.AllowedToVote == true)
                return GameAction.Vote;
            else if (_player.Role == PlayerRole.Mafia)
                return GameAction.Kill;
            else if (_player.Role == PlayerRole.Police)
                return GameAction.Guess;

            else
                return GameAction.None;
        }
    }
}
