using CLUZWeb.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Events
{
    public class GameEventArgs : EventArgs
    {
        public string EventHeader {get; set; }
        public string EventBody { get; set; }
        public InfoType InfoType { get; set; }
        public int TimeSpan { get; set; }

        public GameEventArgs(string header, string body, InfoType type)
        {
            EventHeader = header;
            EventBody = body;
            InfoType = type;
        }

    }
}
