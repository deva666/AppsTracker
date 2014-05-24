using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Task_Logger_Pro.Models;

namespace Task_Logger_Pro.Utils
{
    class SettingsQueue : IDisposable
    {
        private Timer _timer;
        private ConcurrentQueue<UzerSetting> _queue;

        public event EventHandler<UzerSetting> SaveSettings;
        private bool _disposed;

        public SettingsQueue()
        {
            _timer = new Timer();
            _timer.Interval = 100;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _queue = new ConcurrentQueue<UzerSetting>();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_queue.Count == 1)
            {
                _timer.Enabled = false;
                UzerSetting uzerSetting;
                _queue.TryDequeue(out uzerSetting);
                var handler = SaveSettings;
                if (handler != null)
                    handler(this, uzerSetting);
            }
            else
            {
                UzerSetting uzerSetting;
                _queue.TryDequeue(out uzerSetting);
            }
        }

        public void Add(UzerSetting setting)
        {
            if (_queue.Count == 0)
                _timer.Start();
            _queue.Enqueue(setting);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;
            _timer.Enabled = false;
            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();
            var handler = SaveSettings;
            if (handler == null)
                return;
            Delegate[] delegateBuffer = handler.GetInvocationList();
            foreach (EventHandler<UzerSetting> del in delegateBuffer)
                handler -= del;
        }
    }
}
