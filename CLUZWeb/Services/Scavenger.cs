using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CLUZWeb.Services
{
    public class Scavenger : BackgroundService
    {
        private GamePoolService _gamePool;
        Guid gameToRemove = Guid.Empty;

        public Scavenger(GamePoolService gamePoolService)
        {
            _gamePool = gamePoolService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_gamePool.Games.Values.Count() > 0)
                {
                    foreach (KeyValuePair<Guid, Game> entry in _gamePool.Games)
                    {
                        if ((DateTime.UtcNow - entry.Value.ChangeTimeSpamp).TotalMinutes > 30 || entry.Value.IsEnded == true)
                        {
                            gameToRemove = entry.Value.Guid;
                        }
                    }

                    if(gameToRemove != Guid.Empty)
                    {
                        _gamePool.Remove(gameToRemove);
                        gameToRemove = Guid.Empty;
                    }                    
                }

                await Task.Delay(60000);
            }
        }
    }
}
