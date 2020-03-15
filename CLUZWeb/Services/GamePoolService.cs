using CLUZWeb.Models;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public IDictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();

        //internal IList<Game> GetGames()
        //{
        //    return Games.Values.ToList();
        //}

        public void ClearAfterLogOut(IIdentity identity)
        {
            foreach(Game g in Games.Values)
            {
                if (g.PlayerInGame(identity))
                    g.RemovePlayer(identity);

                
            }
        }

        public bool GameExists(string name)
        {
            return Games.Values.ToList().Exists(g => g.Name == name);
        }

        public Guid AddGame(string name, string gamePing, IIdentity identity)
        {


            Game newGame = new Game(name, gamePing, identity);

            //newGame.OnAllReady += new EventHandler(AllPlayersReady.Handler);

            Games.Add(newGame.Guid, newGame);

            //Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            return newGame.Guid;
        }


    }
}