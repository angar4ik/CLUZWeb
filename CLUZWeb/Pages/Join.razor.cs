using System;
using System.Security.Cryptography;
using System.Text;
using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;

namespace CLUZWeb.Pages
{
    public partial class Join
    {
        //cames from page parameter
        [Parameter]
        public Guid Guid { get; set; }

        private JoinGame _joinGameModel = new JoinGame();

        private async void HandleValidSubmit()
        {
            Game g = GamePool.Games[Guid];
            //check password and join the game
            if (g.GamePin == ComputeSha256Hash(_joinGameModel.Password)
                && g.Status != GameState.Locked)
            {
                //check if player already in the game
                if (!g.PlayerInGame(GetCurrentUserGuid()))
                {
                    Player player = new Player(GetCurrentUserName(), GetCurrentUserGuid());
                    g.AddPlayer(player);
                    NavigationManager.NavigateTo($"/gameroom/{g.Guid}");
                }
                else
                {
                    NavigationManager.NavigateTo($"/gameroom/{g.Guid}");
                }
            }
            else
            {
                Toaster.Error("Password wrong or game is locked");
            }

            static string ComputeSha256Hash(string rawData)
            {
                // Create a SHA256   
                using SHA256 sha256Hash = SHA256.Create();
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

    }
}
