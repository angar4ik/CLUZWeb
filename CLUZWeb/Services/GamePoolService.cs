using CLUZWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public IDictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();

        //internal IList<Game> GetGames()
        //{
        //    return Games.Values.ToList();
        //}

        public Guid AddGame(string name, string gamePing)
        {


            Game newGame = new Game(name, gamePing);

            //newGame.OnAllReady += new EventHandler(AllPlayersReady.Handler);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            return newGame.Guid;
        }


    }
}