using CLUZWeb.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CLUZWeb.Pages
{
    public partial class Join
    {
        [Parameter]
        public Guid Guid { get; set; }
        private JoinGame _joinGameModel = new JoinGame();

        private void HandleValidSubmit()
        {
            Game g = _gamePool.Games[Guid];
            //check password and join the game
            if (g.GamePin == ComputeSha256Hash(_joinGameModel.Password)
                && g.Status != GameState.Locked)
            {
                Player player = new Player(_auth.GetAuthenticationStateAsync().Result.User.Identity.Name);

                g.AddPlayer(player);

                NavigationManager.NavigateTo($"/gameroom/{g.Guid}");
            }
            else
            {
                //display snackbar error message
            }

            string ComputeSha256Hash(string rawData)
            {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
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
}
