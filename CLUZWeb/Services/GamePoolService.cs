using CLUZWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CLUZWeb.Services
{
    public class GamePoolService
    {
        public event PropertyChangedEventHandler GameAddedEvent;

        public IDictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();
       
        public bool GameExists(string name)
        {
            return Games.Values.ToList().Exists(g => g.Name == name);
        }

        public Guid AddGame(string name, string gamePing, Guid adminGuid)
        {
            Game newGame = new Game(name, gamePing, adminGuid);

            //newGame.AllPlayersReadyEvent += (o, e) => AllPlayersReadyService.Act(newGame);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            GameAddedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            return newGame.Guid;
        }
    }
}