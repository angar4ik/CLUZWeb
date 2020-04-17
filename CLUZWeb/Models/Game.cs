using CLUZWeb.Events;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using CLUZWeb.Pages;
using System.Threading.Tasks;

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

    public enum GameAction
    {
        Kill,
        Guess,
        Vote,
        None
    }

    public class Game
    {
        public event PropertyChangedEventHandler GamePropertyChangedEvent;
        public event PropertyChangedEventHandler PlayerPropertyChangedEvent;
        public event EventHandler GameEndedEvent;
        public event EventHandler GameEvent;

        protected virtual void OnGameEndedEvent(GameEndedEventArgs e)
        {
            EventHandler handler = GameEndedEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnGameEvent(GameEventArgs e)
        {
            EventHandler handler = GameEvent;
            handler?.Invoke(this, e);
        }

        #region Fields
        private GameState _status = GameState.Unfilled;
        private int _timeFrame = 0;
        #endregion

        #region Props
        public DateTime ChangeTimeSpamp { get; set; } = DateTime.UtcNow;
        public bool IsEnded { get; set; }
        public string Winner { get; set; }
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
                    GamePropertyChanged(nameof(Status));
                }
            }
        }
        public int MinimumPlayerCount { get; set; } = 4;
        public IDictionary<Guid, Player> Players { get; set; } = new Dictionary<Guid, Player>();
        public string Name { get; set; }
        public string GamePin { get; set; }
        public Guid Guid { get; set; }
        public Guid AdminGuid { get; set; }
        public TimeOfDay TimeOfDay { get; set; }
        public bool IsGameVoting { get; set; }
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
                    if (!IsEnded)
                        IsGameEnd();

                    if (value % 2 == 0)
                    {
                        TimeOfDay = TimeOfDay.Day;
                        OnGameEvent(new GameEventArgs("Game", $"Day", InfoType.Info));
                    }
                        
                    else
                    {
                        TimeOfDay = TimeOfDay.Night;
                        OnGameEvent(new GameEventArgs("Game", $"Night", InfoType.Info));
                    }

                    _timeFrame = value;

                    GamePropertyChanged(nameof(TimeFrame));
                }
            }
        }
        #endregion

        public Game(string name, string gamePin, Guid guid, Guid adminGuid)
        {
            Guid = guid;
            Name = name;
            GamePin = ComputeSha256Hash(gamePin);
            AdminGuid = adminGuid;
        }

        private void PlayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            // ResetReadyStatus calls PlayerPropertyChanged again, and Ghost

            int howManyPlayersReady = Players.Values.ToList().FindAll(p => p.State == PlayerState.Ready).Count;

            //Console.WriteLine($"Ready: {howManyPlayersReady}");

            if (howManyPlayersReady >= this.MinimumPlayerCount && howManyPlayersReady == Players.Count())
            {
                ResetPlayersReadyState();
                //Log.Information("All players ready in '{game}'", Name);
                IncrementTimeFrame();

                MakeReadyInactivePlayers();
            }

            PlayerPropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(e.PropertyName));
        }
        private void GamePropertyChanged(string propName)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            if(propName == "Status" && Status == GameState.Locked)
            {
                OnGameEvent(new GameEventArgs("Game", "Game has started", InfoType.Info));
            }

            GamePropertyChangedEvent?.Invoke(this,
                new PropertyChangedEventArgs(nameof(propName)));
        }
        public void AddPlayer(Player player)
        {
            if(Status != GameState.Locked)
            {
                player.PropertyChanged += PlayerPropertyChanged;

                Players.Add(player.Guid, player);

                CheckGameFulfillment();

                GamePropertyChanged(nameof(Players));

                OnGameEvent(new GameEventArgs("Player", $"Player '{player.Name}' added", InfoType.Info));

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
                    p.State = PlayerState.Idle;
            }
        }
        public void MakeReadyInactivePlayers()
        {
            foreach (Player p in this.Players.Values.ToList())
            {
                if ((p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked))
                {
                    p.State = PlayerState.Ready;
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
        public async void IncrementTimeFrame()
        {
            #region Kill Results
            foreach (Player p in Players.Values.ToList())
            {
                if (p.KillRequest == true)
                    p.Role = PlayerRole.Ghost;
            }
            #endregion

            #region Voting
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
            #endregion

            if (TimeFrame == 0 && Status == GameState.Locked && !IsEnded)
            {
                #region First Day to Night (Raffle)
                //Status = GameState.Locked;
                //ResetPlayersReadyState();
                Raffle();
                TimeFrame += 1;
                //LoInformation("GamePool: Iterating Timeframe with Raffle in game '{0}' now is '{1}'", Name, TimeFrame);
                #endregion
            }

            else if (TimeFrame >= 1 && Status == GameState.Locked && !IsEnded)
            {
                #region Regular Iteration
                await Task.Delay(2000);
                TimeFrame += 1;
                //Log.Information("GamePool: Iterating timeframe for '{game}'. Now is '{time}'", Name, g.TimeFrame);
                //set IsGameVoting flag
                if (TimeOfDay == TimeOfDay.Day)
                    IsGameVoting = true;
                else
                    IsGameVoting = false;
                // allowing random player to vote
                AllowRandomPlayerToVote();
                #endregion
            }

        }
        public void AllowRandomPlayerToVote()
        {
            if (TimeOfDay == TimeOfDay.Day)
            {
                Random rand = new Random();

                //this shit should return true if any of active player still have AllowedToVote = false
                while (Players.Values.ToList()
                    .FindAll(p => p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked).ToList()
                    .Exists(p => p.AllowedToVote == false))
                {
                    Player p = Players.ElementAt(rand.Next(0, Players.Count)).Value;

                    if (p.AllowedToVote == false
                        && IsPlayerActive(p) == true)
                    {
                        p.AllowedToVote = true;
                        //OnGameEvent(new GameEventArgs("Vote", $"{p.Name} is voting", InfoType.Info));
                        //await hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{p.Name}' is voting", 3, Guid);

                        break;
                    }
                }
            }
            else
            {
                Players.ToList().ForEach(p => p.Value.AllowedToVote = false);
            }
        }
        private bool IsPlayerActive(Player p)
        {
            if (p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void VoteRequest(Player sourcePlayer, Player targetPlayer)
        {
            //Log.Information("Request from '{0}' to kick '{1}'", _playerPool.Players[fromGuid].Name, _playerPool.Players[kickGuid].Name);

            if (targetPlayer.Role != PlayerRole.Kicked || targetPlayer.Role != PlayerRole.Ghost)
            {
                targetPlayer.VoteCount += 1;
                OnGameEvent(new GameEventArgs("Vote", $"{sourcePlayer.Name} voted to kick {targetPlayer.Name}", InfoType.Info));
            }

            else
                Console.WriteLine("!!! Trying to vote for inactive player");

            sourcePlayer.HasVoted = true;

            AllowRandomPlayerToVote();

            //await _hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{_playerPool.Players[fromGuid].Name}' voted to kick '{_playerPool.Players[kickGuid].Name}'", 5, g.Guid);
        }
        public void KillRequest(Player sourcePlayer, Player targetPlayer)
        {
            if (sourcePlayer.Role == PlayerRole.Mafia)
            {
                //Log.Information("Request from '{0}' to kill '{1}'", _playerPool.Players[fromGuid].Name, _playerPool.Players[killGuid].Name);
                targetPlayer.KillRequest = true;
            }
            else
                Console.WriteLine("!!! Despite this player not a mafia, trying to kill");
        }
        public void IsGameEnd()
        {
            if (IsAnyMafiaLeftInGame() != true
                && Status == GameState.Locked
                && TimeFrame >= 2)
            {
                IsEnded = true;
                OnGameEndedEvent(new GameEndedEventArgs("Citizens", this.Guid));

            }

            else if (IsAnyMafiaLeftInGame() == true
                && HowManyActiveInGame() < MinimumPlayerCount - 1
                && Status == GameState.Locked)
            {
                IsEnded = true;
                OnGameEndedEvent(new GameEndedEventArgs("Mafia", this.Guid));
            }

            //Log.Information("Game {0} has {1} active players", g.Name, Helpers.Results.HowManyActiveInGame(g));


            bool IsAnyMafiaLeftInGame()
            {
                int result = Players.Values.ToList().Count(p => p.Role == PlayerRole.Mafia);  //TrueForAll(p => p.State == PlayerState.Ready)

                //Log.Information("{count} mafia(s) in game", result);

                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        int HowManyActiveInGame()
        {
            return Players.Values.ToList().Count(p => p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked);
        }

        int HowManyVotedInGame()
        {
            return Players.Values.ToList().Count(p => p.HasVoted == true);
        }
    }
}
