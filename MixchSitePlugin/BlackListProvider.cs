﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Common;

namespace OpenrecSitePlugin
{
    class BlackListProvider : IBlackListProvider
    {
        private CancellationTokenSource _cts;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;

        public event EventHandler<List<string>> Received;
        public async Task ReceiveAsync(string movieId, Context context)
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var list = await API.GetBanList(_dataSource, context);
                    Received?.Invoke(this, list);
                }
                catch(System.Net.Http.HttpRequestException ex)
                {
                    _logger.LogException(ex);
                    if (ex.Message.Contains("401"))
                    {
                        //401が出てたら回復の見込み無し
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                try
                {
                    await Task.Delay(60 * 1000, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            _cts = null;
        }
        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        public BlackListProvider(IDataSource dataSource, ILogger logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }
    }
}
