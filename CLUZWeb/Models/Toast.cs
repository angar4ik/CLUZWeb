using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Models
{
    public class Toast
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public Toast(string header, string body)
        {
            Header = header;
            Body = body;
        }
    }
}
