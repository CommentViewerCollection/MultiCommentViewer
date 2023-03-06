using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SitePluginCommon.AutoReconnection
{
    public class ReconnectionCounter
    {
        Queue<DateTime> _list = new Queue<DateTime>();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>一定時間内のAdd回数が一定回数以下だったらtrue</returns>
        public bool Add(DateTime now)
        {
            const int n = 5;
            const int range = 60 * 1000;
            _list.Enqueue(now);
            if (_list.Count <= n)
            {
                return true;
            }
            //直近のn個より前のデータを全て消す
            for (int i = _list.Count - n; i > 0; i--)
            {
                _list.Dequeue();
            }
            //今あるn個のデータが全て基準時刻移行のものだったらfalseを返す
            var baseTime = now.AddMilliseconds(-range);
            var b = _list.All(h => h > baseTime);
            return !b;
        }
        public void Reset()
        {
            _list.Clear();
        }
    }
    /// <summary>
    /// 複数のIProviderを管理
    /// </summary>
    public class ConnectionManager
    {
        private readonly MessageUntara _messageSender;
        private IEnumerable<IProvider> _group;
        private readonly ILogger _logger;

        private bool IsValidGroup(IEnumerable<IProvider> group) => GetRoot(group) != null;
        private IProvider GetRoot(IEnumerable<IProvider> group)
        {
            var roots = group.Where(x => x.Master == null).ToList();
            if (roots.Count == 1)
            {
                return roots[0];
            }
            else
            {
                return null;
            }
        }
        private IProvider FindProvider(IEnumerable<IProvider> providers, Task work) => providers.Where(x => x.Work == work).First();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>RootのFinishReason</returns>
        public async Task<ProviderFinishReason> ConnectAsync(IEnumerable<IProvider> group)
        {
            _group = group;
            if (!IsValidGroup(group))
            {
                throw new ArgumentException();
            }
            _reconnectionCounterDict.Clear();
            foreach (var provider in group)
            {
                _reconnectionCounterDict.Add(provider, new ReconnectionCounter());
                provider.Start();
            }
            var workingProviders = new List<IProvider>(group);
            while (true)
            {
                var t = await Task.WhenAny(workingProviders.Select(x => x.Work));
                var provider = FindProvider(workingProviders, t);
                if (IsRoot(provider))
                {
                    //rootが終了したら全て終了する。
                    foreach (var a in group)
                    {
                        await FinishProviderAsync(workingProviders, a);
                    }
                    return provider.FinishReason;//ここ以外からはwhileから抜け出さない
                }
                else
                {
                    await FinishProviderAsync(workingProviders, provider);
                    //Masterが終了していなかったら再起動する
                    if (!provider.Master.IsFinished)
                    {
                        //エラーによる終了が一定時間に一定回数発生したら継続しないようにしたい。
                        if (_reconnectionCounterDict[provider].Add(DateTime.Now))
                        {
                            provider.Start();
                            workingProviders.Add(provider);
                        }
                    }
                }
            }
        }
        public void Disconnect()
        {
            if (_group == null) return;
            var root = GetRoot(_group);
            root.Stop();
        }

        private async Task FinishProviderAsync(List<IProvider> providers, IProvider provider)
        {
            try
            {
                provider.Stop();
                await provider.Work;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            providers.Remove(provider);
        }

        bool IsRoot(IProvider provider) => provider.Master == null;
        public ConnectionManager(ILogger logger)
        {
            _logger = logger;
        }
        Dictionary<IProvider, ReconnectionCounter> _reconnectionCounterDict = new Dictionary<IProvider, ReconnectionCounter>();
    }
}
