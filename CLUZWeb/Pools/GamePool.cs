﻿using CLUZWeb.Models;
using System;
using System.Collections.Generic;

namespace CLUZWeb.Pools
{
    public class GamePool
    {
        //PlayerPool _playerPool;
        //public GamePool(PlayerPool playerPool)
        //{
        //    _playerPool = playerPool;
        //}

        public Dictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();
        public Guid AddGame(string name, string gamePing, double minimum)
        {
            Game newGame = new Game(name, gamePing, minimum);

            //newGame.OnAllReady += new EventHandler(AllPlayersReady.Handler);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            return newGame.Guid;
        }


    }
}