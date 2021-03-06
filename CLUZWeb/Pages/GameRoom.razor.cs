﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using CLUZWeb.Events;
using CLUZWeb.Models;
using CLUZWeb.Shared.ModalResources;
using Microsoft.AspNetCore.Components;
using Serilog;

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
        private bool _someoneVoting = false;

        private EventHandler _gameEventHandler;
        private EventHandler _gameEndHandler;
        private PropertyChangedEventHandler _gamePropertyChangedEventHandler;
        private PropertyChangedEventHandler _playerPropertyChangedEventHandler;

        protected override void OnInitialized()
        {
            #region Event Handlers
            _gameEventHandler = (o, e) =>
            {
                GameEventArgs message = e as GameEventArgs;
                ShowInfo(message.EventHeader, message.EventBody, message.InfoType);
            };

            _gameEndHandler = (o, e) =>
            {
                GameEndedEventArgs winner = e as GameEndedEventArgs;
                //ShowInfo("Game", $"Game has ended. Winner is/are {winner.Winner}", InfoType.Info);
                NavigationManager.NavigateTo($"/winner/{winner.Winner}/{winner.Guid}");
                //NavigationManager.NavigateTo("/"); 
            };

            _gamePropertyChangedEventHandler = async (o, e) =>
            {
                async Task ShowModalAsync(string modalTitle, string modalText, string modalButton)
                {
                    ModalParameters parameters = new ModalParameters();
                    parameters.Add("ModalText", modalText);
                    parameters.Add("ModalButton", modalButton);
                    await Modal.Show<CustomModal>(modalTitle, parameters).Result;
                }

                _someoneVoting = false;

                if (_game.Status == GameState.Locked && _game.TimeFrame == 0)
                {
                    await ShowModalAsync("Game", "Game is about to start. Are you ready?", "Ready");

                    _player.State = PlayerState.Ready;
                }

                if (_game.Status == GameState.Locked && _game.TimeFrame > 0 && _game.TimeOfDay == TimeOfDay.Night)
                {
                    switch (_player.Role)
                    {
                        case PlayerRole.Citizen:
                            await ShowModalAsync("Citizen", "It night time and you are Citizen. Hold tight!", "Dismiss");
                            _player.State = PlayerState.Ready;
                            break;
                        case PlayerRole.Mafia:
                            await ShowModalAsync("Mafia", "It night time and you are Mafia. Select player to kill from the list", "Continue");
                            break;
                        case PlayerRole.Police:
                            await ShowModalAsync("Police", "It night time and you are Police. Guess who is Mafia from the list", "Continue");
                            break;
                        default:
                            ShowModalAsync("Chill", "Just chill. They kicked you out", "Dismiss");
                            _player.State = PlayerState.Ready;
                            break;
                    }
                }

                await InvokeAsync(() => StateHasChanged());
            };

            _playerPropertyChangedEventHandler = async (o, e) =>
            {
                bool isModalOpen = false;

                async Task ShowModalAsync(string modalTitle, string modalText, string modalButton)
                {
                    ModalParameters parameters = new ModalParameters();
                    parameters.Add("ModalText", modalText);
                    parameters.Add("ModalButton", modalButton);
                    await Modal.Show<CustomModal>(modalTitle, parameters).Result;
                }

                if (_game.IsGameVoting == true && isModalOpen == false && e.PropertyName == "AllowedToVote")
                {
                    if (_player.AllowedToVote == true && _player.HasVoted == false)
                    {
                        isModalOpen = true;
                        _someoneVoting = false;
                        await ShowModalAsync("Vote", "It's your turn to vote. Select player to kick out from the list", "Continue");
                        isModalOpen = false;
                    }
                    else
                        _someoneVoting = true;
                }

                await InvokeAsync(() => StateHasChanged());
            };
            #endregion

            if (GamePool.Games.TryGetValue(Guid, out _game) &&
                _game.Players.TryGetValue(GetCurrentUserGuid(), out _player))
            {
                _players = _game.Players.Values;

                _game.GamePropertyChangedEvent += _gamePropertyChangedEventHandler;
                _game.PlayerPropertyChangedEvent += _playerPropertyChangedEventHandler;
                _game.GameEvent += _gameEventHandler;
                _game.GameEndedEvent += _gameEndHandler;
                //Log.Information("Player {player} subscribed to events at {time}.", GetCurrentUserGuid(), DateTime.Now);
            }
            else
            {
                _players = new List<Player>();
            }
        }

        private void Ready(Player p)
        {
            p.State = PlayerState.Ready;
        }
        private void Action(Player p)
        {
            //_player doing something with p
            if (GetActionBtnType() == GameAction.Vote)
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
                    ShowInfo("Game", $"{p.Name} is mafia!", InfoType.Success);
                else
                    ShowInfo("Game", $"{p.Name} NOT a mafia!", InfoType.Warn);
            }
        }
        private void Start(Game g)
        {
            if (g.Status == GameState.Filled)
            {
                g.Status = GameState.Locked;
            }
            else if (g.Status == GameState.Unfilled)
            {
                ShowInfo("Game", "Minimum 4 players needed", InfoType.Error);
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
            if (p.Role == PlayerRole.Ghost
                || p.Role == PlayerRole.Kicked)
                return true;
            else if (_player.Guid == p.Guid)
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
        private string IsGhost(Player p)
        {
            if(p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked)
            {
                return "ghost";
            }
            return "";
        }
        public void Dispose()
        {
            if (GamePool.Games.TryGetValue(Guid, out _game))
            {
                _game.GameEvent -= _gameEventHandler;
                _game.GameEndedEvent -= _gameEndHandler;
                _game.GamePropertyChangedEvent -= _gamePropertyChangedEventHandler;
                _game.PlayerPropertyChangedEvent -= _playerPropertyChangedEventHandler;
            }
        }
    }
}
