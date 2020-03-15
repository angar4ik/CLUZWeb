using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class GameRoom
    {
        [Parameter]
        public Guid Guid { get; set; }
        IEnumerable<Player> players;

        protected override void OnInitialized()
        {
            try
            {
                players = _gamePool.Games[Guid].Players.Values;

                _gamePool.Games[Guid].GamePropertyChangedEvent += async (o, e) => await InvokeAsync(() => StateHasChanged());
            }
            catch (KeyNotFoundException)
            {
                players = new List<Player>();
            }
            
            
        }

        
    }
}
