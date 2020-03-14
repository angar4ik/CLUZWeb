using CLUZWeb.Data;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class Index
    {
        private IList<Game> games;

        protected override async Task OnInitializedAsync()
        {
            games = _gamePool.GetGames();
        }

        private void Create()
        {
            NavigationManager.NavigateTo("/create");
        }
    }
}
