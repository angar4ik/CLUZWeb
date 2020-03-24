using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Events
{
    public class GameEndedEventArgs : EventArgs
    {
        public string Winner { get; set; }
        public Guid Guid { get; set; }
        public GameEndedEventArgs(string winner, Guid guid)
        {
            Winner = winner;
            Guid = guid;
        }
    }
}
