using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        public event PropertyChangedEventHandler GamePropertyChangedEvent;
        public event PropertyChangedEventHandler PlayerPropertyChangedEvent;

        #region Fields
        private GameState _status = GameState.Unfilled;
        private int _timeFrame = 0;
        #endregion

        #region Props
        public DateTime ChangeTimeSpamp { get; set; } = DateTime.UtcNow;
        public bool GameHasEnded { get; set; } = false;
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
                    GamePropertyChanged(nameof(GameState));
                }
            }
        }
        public int MinimumPlayerCount { get; set; } = 2;
        public IDictionary<Guid, Player> Players { get; set; } = new Dictionary<Guid, Player>();
        public string Name { get; }
        public string GamePin { get; }
        public Guid Guid { get; }
        public Guid AdminGuid { get; }
        public TimeOfDay TimeOfDay { get; set; }
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
                    if (value % 2 == 0)
                        TimeOfDay = TimeOfDay.Day;
                    else { TimeOfDay = TimeOfDay.Night; }

                    _timeFrame = value;
                    GamePropertyChanged(nameof(TimeFrame));
                }
            }
        }
        #endregion

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
                //Log.Information("All players ready in '{game}'", Name);
                AllPlayersReady();
            }

            PlayerPropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Player)));
        }
        private void GamePropertyChanged(string propName)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            GamePropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(Game)));
        }
        public void AddPlayer(Player player)
        {
            if(Status != GameState.Locked)
            {
                player.PropertyChanged += PlayerPropertyChanged;

                Players.Add(player.Guid, player);

                CheckGameFulfillment();

                GamePropertyChanged(nameof(Players));

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
            if (Players.Count >= MinimumPlayerCount)
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
        public void AllPlayersReady()
        {
            #region Kill Results
            foreach (Player p in Players.Values.ToList())
            {
                if (p.KillRequest == true)
                    p.Role = PlayerRole.Ghost;
            }
            #endregion

            //Votes should only by days
            if (TimeOfDay == TimeOfDay.Day && TimeFrame >= 2 && Status == GameState.Locked)
            {
                #region Votes Results
                List<Player> playersSortedList = new List<Player>();

                if (Players.Count >= 2)
                {
                    //sort Players by Vote var
                    playersSortedList = Players.Values.ToList().OrderByDescending(o => o.VoteCount).ToList();
                    //assign kicked to first guid in sorted list
                    Players[playersSortedList[0].Guid].Role = PlayerRole.Kicked;
                }

                ResetVotes();

                //Log.Information("Votes over. Kicked name {0}", Players[playersSortedList[0].Guid].Name);
                #endregion
            }

            if (TimeFrame == 0 && Status == GameState.Locked)
            {
                #region First Day to Night (Raffle)
                //Status = GameState.Locked;
                ResetPlayersReadyState();
                Raffle();
                TimeFrame += 1;
                //LoInformation("GamePool: Iterating Timeframe with Raffle in game '{0}' now is '{1}'", Name, TimeFrame);
                #endregion
            }

            else if (TimeFrame >= 1 && Status == GameState.Locked)
            {
                #region Regular Iteration
                ResetPlayersReadyState();
                TimeFrame += 1;
                //Log.Information("GamePool: Iterating timeframe for '{game}'. Now is '{time}'", Name, g.TimeFrame);

                // allowing random player to vote
                //Voting.AllowRandomPlayerToVote(g, _hubContext);
                #endregion
            }

        }
    }
}
