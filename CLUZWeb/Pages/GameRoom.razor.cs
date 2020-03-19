using System;
using System.Collections.Generic;
using CLUZWeb.Events;
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
            if (GamePool.Games.TryGetValue(Guid, out _game) &&
                _game.Players.TryGetValue(GetCurrentUserGuid(), out _player))
            {
                _players = _game.Players.Values;

                _game.GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
                _game.PlayerPropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
                _game.GameEndedEvent += (o, e) =>
                {
                    GameEndedEventArgs winner = e as GameEndedEventArgs;
                    GamePool.Games.Remove(_game.Guid);
                    NavigationManager.NavigateTo($"/winner/{winner.Winner}");
                };
            }
            else
            {
                _players = new List<Player>();
            }
        }
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            
        }

    private void Ready(Player p)
        {
            p.State = PlayerState.Ready;
        }

        private async void Action(Player p)
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
                _player.State = PlayerState.Ready;
                if (p.Role == PlayerRole.Mafia)
                    await AlertMessage("alert-info", $"{p.Name} is mafia!");
                else
                    await AlertMessage("alert-info", $"{p.Name} NOT a mafia!");
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
                && _player.Role != PlayerRole.Police
                && _game.IsGameVoting == false)
                return false;
            else
                return true;
        }

        private bool IsDisabledActionBtn(Player p)
        {
            if (p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked)
                return true;
            else if (_game.TimeOfDay == TimeOfDay.Night
                && (_player.Role == PlayerRole.Mafia || _player.Role == PlayerRole.Police)
                && _player.State == PlayerState.Idle)
                return false;
            else if (_game.TimeOfDay == TimeOfDay.Day
                    && _player.AllowedToVote == true
                    && _player.HasVoted == false)
                return false;
            else
                return true;

        }

        private GameAction GetActionBtnType()
        {
            if (_game.IsGameVoting == true)
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
