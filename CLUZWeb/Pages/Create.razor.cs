using CLUZWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Pages
{
    public partial class Create
    {
        //[Inject] ToastService ToastService { get; set; }

        private CreateGame _createGameModel = new CreateGame();

        private string StatusMessage;
        private string StatusClass;

        private void HandleValidSubmit()
        {
            if (_gamePool.GameExists(_createGameModel.Name))
            {
                StatusClass = "alert-danger";
                StatusMessage = "Game exists";
            }
            else
            {
                _gamePool.AddGame(_createGameModel.Name, _createGameModel.Password, _auth.GetAuthenticationStateAsync().Result.User.Identity);
                NavigationManager.NavigateTo("/");
            }


        }
    }
}
