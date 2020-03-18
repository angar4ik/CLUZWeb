using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.Json.Serialization;

namespace CLUZWeb.Models
{
    public enum PlayerRole
    {
        None,
        Citizen,
        Mafia,
        Police,
        Ghost,
        Kicked
    }
    public enum PlayerState
    {
        Idle,
        Ready
    }

    public class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fileds
        private PlayerState _state = PlayerState.Idle;
        private PlayerRole _role = PlayerRole.None;
        private int _voteCount = 0;
        private bool allowedtovote = false;
        private bool hasVoted = false;
        #endregion

        #region Properties
        public Guid Guid { get; }

        public string Name { get; }

        [JsonIgnore]
        public bool KillRequest { get; set; }

        public int VoteCount
        {
            get
            {
                return _voteCount;
            }

            set
            {
                if (value != _voteCount)
                {
                    _voteCount = value;
                    NotifyPropertyChanged(nameof(VoteCount));
                }
            }
        }

        public PlayerState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (value != _state)
                {
                    _state = value;
                    NotifyPropertyChanged(nameof(State));
                }
            }
        }

        public PlayerRole Role
        {

            get
            {
                return _role;
            }

            set
            {
                if (value != _role)
                {
                    _role = value;
                    NotifyPropertyChanged(nameof(Role));
                }
            }
        }

        public bool AllowedToVote
        {
            get
            {
                return allowedtovote;
            }

            set
            {
                if (value != allowedtovote)
                {
                    allowedtovote = value;
                    NotifyPropertyChanged(nameof(AllowedToVote));
                }
            }
        }

        public bool HasVoted
        {
            get
            {
                return hasVoted;
            }

            set
            {
                if (value != hasVoted)
                {
                    hasVoted = value;
                    NotifyPropertyChanged(nameof(HasVoted));
                }
            }
        }
        #endregion
        public Player(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public void ExcludeMySelfFromAnyGame(GamePool gamePool)
        //{

        //    foreach (Game g in gamePool.Games.Values.ToList())
        //    {
        //        if(g.Players.TryGetValue(this.Guid, out Player p))
        //        {
        //            g.Players.Remove(p.Guid);
        //            Log.Information("Player {name} removed his self from game {name}", p.Name, g.Name);
        //        }
        //    }
        //}
    }
}
