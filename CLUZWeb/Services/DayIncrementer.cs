//using CLUZWeb.Helpers;
//using CLUZWeb.Models;
//using Microsoft.Extensions.Hosting;
//using System.Threading;
//using System.Threading.Tasks;

//namespace CLUZWeb.Services
//{
//    public class DayIncrementer : BackgroundService
//    {
//        private GamePoolService _gamePool;
//        private AllPlayersReady _allPlayersReady;

//        public DayIncrementer(GamePoolService gamePool, AllPlayersReady allPlayersReady)
//        {
//            _gamePool = gamePool;
//            _allPlayersReady = allPlayersReady;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                foreach (Game game in _gamePool.Games.Values)
//                {
//                    if (game.AllPlayersReady)
//                    {
//                        _allPlayersReady.Act(game);

//                        game.AllPlayersReady = false;
//                    }
//                }

//                await Task.Delay(1250);
//            }
//        }
//    }
//}
