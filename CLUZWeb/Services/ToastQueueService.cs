using CLUZWeb.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CLUZWeb.Services
{
    public class ToastQueueService
    {
        public event EventHandler ToastServiceEvent;
        public ICollection<Toast> Toasts { get; set; } = new List<Toast>();
        protected virtual void OnToastServiceEvent(EventArgs e)
        {
            EventHandler handler = ToastServiceEvent;
            handler?.Invoke(this, e);
        }
        public async Task AddToast(string header, string body, int life)
        {
            Toast toast = new Toast(header, body);
            Toasts.Add(toast);
            OnToastServiceEvent(new EventArgs());
            Task.Run(() => WaitAndRemove(toast, life));
        }

        private async Task WaitAndRemove(Toast toast, int life)
        {
            await Task.Delay(life * 1000);
            Toasts.Remove(toast);
            OnToastServiceEvent(new EventArgs());
        }
    }
}
