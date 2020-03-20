using CLUZWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CLUZWeb.Services
{
    public class GamePoolService
    {
        public event PropertyChangedEventHandler GamePoolEvent;

        public IDictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();
       
        public bool GameExists(string name)
        {
            return Games.Values.ToList().Exists(g => g.Name == name);
        }

        public Guid Add(string name, string gamePing, Guid adminGuid)
        {
            Game newGame = new Game(name, gamePing, adminGuid);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            GamePoolEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            return newGame.Guid;
        }

        public void Remove(Guid guid)
        {
            Games.Remove(guid);

            GamePoolEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));
        }
    }
}