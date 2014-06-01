using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Task_Logger_Pro.Utils
{
    class SettingsQueue : IDisposable
    {
        private Timer _timer;
        private ConcurrentQueue<SettingsProxy> _queue;

        public event EventHandler<SettingsProxy> SaveSettings;
        private bool _disposed;

        public SettingsQueue()
        {
            _timer = new Timer();
            _timer.Interval = 100;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _queue = new ConcurrentQueue<SettingsProxy>();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_queue.Count == 1)
            {
                _timer.Enabled = false;
                SettingsProxy uzerSetting;
                _queue.TryDequeue(out uzerSetting);
                var handler = SaveSettings;
                if (handler != null)
                    handler(this, uzerSetting);
            }
            else
            {
                SettingsProxy uzerSetting;
                _queue.TryDequeue(out uzerSetting);
            }
        }

        public void Add(SettingsProxy setting)
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
            foreach (EventHandler<SettingsProxy> del in delegateBuffer)
                handler -= del;
        }
    }
}
