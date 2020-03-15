﻿using CLUZWeb.Helpers;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CLUZWeb.Services
{
    public class GamePoolService
    {
        //PlayerPool _playerPool;
        //public GamePool(PlayerPool playerPool)
        //{
        //    _playerPool = playerPool;
        //}

        [Inject] AllPlayersReadyService AllPlayersReadyService { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public IDictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();

        //internal IList<Game> GetGames()
        //{
        //    return Games.Values.ToList();
        //}

        public void ClearAfterLogOut(Guid guid)
        {
            foreach(Game g in Games.Values)
            {
                if (g.PlayerInGame(guid))
                    g.RemovePlayer(guid);

                
            }
        }

        public bool GameExists(string name)
        {
            return Games.Values.ToList().Exists(g => g.Name == name);
        }

        public Guid AddGame(string name, string gamePing, Guid adminGuid)
        {
            Game newGame = new Game(name, gamePing, adminGuid);

            newGame.AllPlayersReady += (o, e) => AllPlayersReadyService.Act(newGame);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            return newGame.Guid;
        }
    }
}