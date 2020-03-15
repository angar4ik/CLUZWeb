using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

namespace CLUZWeb.Models
{
    public enum GameState
    {
        Unfilled,
        Filled,
        Locked,
        Finished
    }

    public enum TimeOfDay
    {
        Day,
        Night
    }

    public class Game
    {
        //public event EventHandler OnAllReady;
        public event EventHandler AllPlayersReady;
        public event PropertyChangedEventHandler GamePropertyChangedEvent;
        public event PropertyChangedEventHandler PlayerPropertyChangedEvent;

        [JsonIgnore]
        public DateTime ChangeTimeSpamp { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public bool PropChanged { get; set; } = false;
        [JsonIgnore]
        public bool ListChanged { get; set; } = false;
        //[JsonIgnore]
        //public bool AllPlayersReady { get; set; } = false;
        [JsonIgnore]
        public bool GameHasEnded { get; set; } = false;

        private GameState _status = GameState.Unfilled;
        public GameState Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (value != _status)
                {
                    _status = value;
                    GamePropertyChanged("GameState");
                }
            }
        }

        private int _minimumPlayersCount = 4;
        public int MinimumPlayerCount
        {
            get
            {
                return _minimumPlayersCount;
            }

            set
            {
                if (value != _minimumPlayersCount)
                {
                    _minimumPlayersCount = value;
                }
            }
        }

        [JsonIgnore]
        public IDictionary<Guid, Player> Players { get; set; } = new Dictionary<Guid, Player>();

        public string Name { get; }

        public string GamePin { get; set; }

        public Guid Guid { get; }

        public Guid AdminGuid { get; }

        public TimeOfDay TimeOfDay { get; set; }

        private int _timeFrame = 0;
        public int TimeFrame
        {
            get
            {
                return _timeFrame;
            }

            set
            {
                if (value != _timeFrame)
                {
                    if (_timeFrame % 2 == 0)
                        TimeOfDay = TimeOfDay.Day;
                    else { TimeOfDay = TimeOfDay.Night; }
                    
                    _timeFrame = value;
                    GamePropertyChanged("TimeFrame");
                }
            }
        }

        public Game(string name, string gamePin, Guid adminGuid)
        {
            Guid = Guid.NewGuid();
            Name = name;
            GamePin = ComputeSha256Hash(gamePin);
            AdminGuid = adminGuid;
        }
        private void PlayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            int howManyPlayersReady = Players.Values.ToList().FindAll(p => p.State == PlayerState.Ready).Count;

            if (howManyPlayersReady >= this.MinimumPlayerCount)
            {
                //game.Players.Count >= game.MinimumPlayerCount
                //set a flag all players ready
                //AllPlayersReady = true;
                //Log.Information("All players ready in '{game}'", Name);
                AllPlayersReady?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));
            }

            PlayerPropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Player)));
        }

        private void GamePropertyChanged(string propName)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            GamePropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));

            PropChanged = true;
        }

        /// <summary>
        /// Will add player object to game players dict and will check game fullfilment
        /// </summary>
        /// <param name="player">Player object</param>
        public void AddPlayer(Player player)
        {
            if(Status != GameState.Locked)
            {
                //GamePropertyChanged("Players");

                player.PropertyChanged += PlayerPropertyChanged;

                Players.Add(player.Guid, player);

                CheckGameFulfillment();

                GamePropertyChanged("PlayerList");

                //Log.Information("Game: Player '{0}' added to the game '{1}'", player.Name, this.Name);
            }
            else
            {
                //Log.Information("Can't allow add player {0} to game {1}. Game is locked", player.Name, this.Name);
            }
        }

        public bool PlayerInGame(Guid guid)
        {
            return Players.Values.ToList().Exists(p => p.Guid == guid);
        }

        /// <summary>
        /// Will remove player from players dict by guid key and will check game fullfillment
        /// </summary>
        /// <param name="playerGuid"></param>
        public void RemovePlayer(Guid guid)
        {
            if (Players.Remove(guid))
            {
                //resetting particular player
                //Players[playerGuid] = new Player(Players[playerGuid].ConnId, Players[playerGuid].Name, playerGuid);

                CheckGameFulfillment();

                GamePropertyChanged("PlayerList");
            }
            else
            {
                //Log.Warning("UNSUCCESSFUL attempt to remove player '{0}' from game '{1}'", Players[playerGuid].Name, this.Name);
            }
        }

        private void CheckGameFulfillment()
        {
            if (Players.Count >= _minimumPlayersCount)
            {
                Status = GameState.Filled;
            }
        }

        public void Raffle()
        {
            int count = Players.Count; //4

            Random random = new Random();

            int mafia = random.Next(0, count); //3

            HashSet<int> exclude = new HashSet<int>() { mafia };

            int police = GiveMeANumber(0, count, exclude);

            for (int i = 0; i < Players.Count; i++)
            {
                if(i == mafia)
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Mafia;
                }
                else if(i == police)
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Police;
                }
                else
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Citizen;
                }
            }
        }

        public void ResetPlayers()
        {
            //reset player states
            foreach (Player p in this.Players.Values.ToList())
            {
                p.AllowedToVote = false;
                p.KillRequest = false;
                p.Role = PlayerRole.None;
                p.State = PlayerState.Idle;
                p.VoteCount = 0;
            }

            this.Players.Clear();
        }

        public void ResetVotes()
        {
            foreach (Player p in this.Players.Values.ToList())
            {
                p.VoteCount = 0;
                p.HasVoted = false;
            }
        }

        public void ResetPlayersReadyState()
        {
            foreach (Player p in this.Players.Values.ToList())
            {
                if(!(p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked))
                {
                    p.State = PlayerState.Idle;
                }
                    
            }
        }

        private int GiveMeANumber(int min, int max, HashSet<int> exclude)
        {
            //var exclude = new HashSet<int>() { 5, 7, 17, 23 };

            var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

            var rand = new Random();
            int index = rand.Next(min, max - exclude.Count);
            return range.ElementAt(index);
        }

        static string ComputeSha256Hash(string rawData)
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
