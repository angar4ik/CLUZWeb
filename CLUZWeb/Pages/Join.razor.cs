using CLUZWeb.Models;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;

namespace CLUZWeb.Pages
{
    public partial class Join
    {
        [Inject] GamePoolService GamePool { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] UserManager<IdentityUser> UserManager { get; set; }

        [Parameter]
        public Guid Guid { get; set; }
        private JoinGame _joinGameModel = new JoinGame();
        private string StatusMessage;
        private string StatusClass;
        private void HandleValidSubmit()
        {
            Game g = GamePool.Games[Guid];
            //check password and join the game
            if (g.GamePin == ComputeSha256Hash(_joinGameModel.Password)
                && g.Status != GameState.Locked)
            {
                //check if player already in the game
                if (!g.PlayerInGame(Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User))))
                {
                    Player player = new Player(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User.Identity.Name, Guid.Parse(UserManager.GetUserId(AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User)));
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
                StatusClass = "alert-danger";
                StatusMessage = "Password wrong or game locked";
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
