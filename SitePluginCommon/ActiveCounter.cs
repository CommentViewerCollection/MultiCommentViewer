using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
namespace SitePluginCommon
{
    public class ActiveCounter<T> : IActiveCounter<T>
    {
        class Item
        {
            public T Id { get; }
            public DateTime Time { get; }
            public override bool Equals(object obj)
            {
                return this.Id.Equals(obj);
            }
            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
            public Item(T id, DateTime time)
            {
                Id = id;
                Time = time;
            }
        }
        System.Threading.CancellationTokenSource _cts;
        #region IActiveCounter
        public event EventHandler<int> Updated;

        #region CountIntervalSec
        private int _countIntervalSec;
        public int CountIntervalSec
        {
            get => _countIntervalSec;
            set
            {
                if (value > 0)
                {
                    _countIntervalSec = value;
                }
            }
        }
        #endregion//CountIntervalSec
        public int MeasureSpanMin { get; set; } = 10;
        public async Task Start()
        {
            if (_cts != null) throw new InvalidOperationException("Start済み");
            _cts = new System.Threading.CancellationTokenSource();
            while (true)
            {
                var list = new List<Item>();
                for(int i = 0; i < _stack.Count; )
                {
                    var item = _stack[i];
                    var expired = DateTime.Now.AddMinutes(-MeasureSpanMin);
                    if(item.Time < expired)
                    {
                        lock (_lockObj)
                        {
                            _stack.RemoveAt(i++);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                lock (_lockObj)
                {
                    _stack = _stack.Distinct().ToList();
                }
                Updated?.Invoke(this, _stack.Count);
                try
                {
                    await WaitAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { break; }
            }
        }
        private readonly object _lockObj = new object();
        public void Add(T id)
        {
            lock (_lockObj)
            {
                _stack.Add(new Item(id, DateTime.Now));
            }
        }
        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }
        #endregion
        List<Item> _stack = new List<Item>();
        /// <summary>
        /// 計測する範囲より古いデータか
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected virtual bool IsOldItem(DateTime time)
        {
            return time < DateTime.Now.AddMinutes(-MeasureSpanMin);
        }
        /// <summary>
        /// 次の更新タイミングまで待つ
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected virtual Task WaitAsync(System.Threading.CancellationToken ct)
        {
            return Task.Delay(CountIntervalSec * 1000, ct);
        }
    }
}
