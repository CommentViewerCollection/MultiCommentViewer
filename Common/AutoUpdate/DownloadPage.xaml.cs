﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Common.AutoUpdate
{
    /// <summary>
    /// Interaction logic for DownloadPage.xaml
    /// </summary>
    public partial class DownloadPage : Page
    {
        private readonly LatestVersionInfo _latest;
        private readonly ILogger _logger;
        private readonly string _userAgent;
        Dispatcher _dispatcher;
        public DownloadPage(NavigationService service, Version currentVersion, LatestVersionInfo latestVersionInfo, ILogger logger, string userAgent)
        {
            InitializeComponent();
            Loaded += DownloadPage_Loaded;
            _latest = latestVersionInfo;
            _logger = logger;
            _userAgent = userAgent;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private async void DownloadPage_Loaded(object sender, RoutedEventArgs e)
        {
            var exeFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var baseDir = System.IO.Path.GetDirectoryName(exeFile);
            var zipFilename = "latest.zip";
            var zipFilePath = System.IO.Path.Combine(baseDir, zipFilename);
            System.IO.File.Delete(zipFilePath);
            var url = _latest.Url;
            try
            {
                using (var client = new HttpClientDownloadWithProgress(url, zipFilePath))
                {
                    client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                    {
                        _dispatcher.Invoke(() =>
                        {
                            if (progressPercentage.HasValue)
                            {
                                progressBar.Value = progressPercentage.Value;
                            }
                            txtTotalBytesDownloaded.Text = totalBytesDownloaded.ToString();
                            txtFileSize.Text = totalFileSize.ToString();
                        });
                        Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                    };

                    await client.StartDownload();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogException(ex, "", $"baseDir={baseDir}, zipFilePath={zipFilePath}");
            }
            //var wc = new WebClient();
            //wc.Headers.Add("User-Agent", _userAgent);
            //wc.DownloadProgressChanged += Wc_DownloadProgressChanged;

            //TODO:ここでWebExceptionが発生した場合の対応
            //await wc.DownloadFileTaskAsync(url, zipFilePath);

            //list.txtに記載されているファイル全てに.oldを付加            
            try
            {
                var list = new List<string>();
                using (var sr = new System.IO.StreamReader(System.IO.Path.Combine(baseDir, "list.txt")))
                {
                    while (!sr.EndOfStream)
                    {
                        var filename = sr.ReadLine();
                        if (!string.IsNullOrEmpty(filename))
                            list.Add(filename);
                    }
                }
                foreach (var filename in list)
                {
                    var srcPath = System.IO.Path.Combine(baseDir, filename);
                    var dstPath = System.IO.Path.Combine(baseDir, filename + ".old");
                    try
                    {
                        if (System.IO.File.Exists(srcPath))
                        {
                            System.IO.File.Delete(dstPath);//If the file to be deleted does not exist, no exception is thrown.
                            System.IO.File.Move(srcPath, dstPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "", $"src={srcPath}, dst={dstPath}");
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            try
            {
                using (var archive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var entryPath = System.IO.Path.Combine(baseDir, entry.FullName);
                        var entryDir = System.IO.Path.GetDirectoryName(entryPath);
                        if (!System.IO.Directory.Exists(entryDir))
                        {
                            System.IO.Directory.CreateDirectory(entryDir);
                        }

                        var entryFn = System.IO.Path.GetFileName(entryPath);
                        if (!string.IsNullOrEmpty(entryFn))
                        {
                            entry.ExtractToFile(entryPath, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            try
            {
                var pid = Process.GetCurrentProcess().Id;
                System.Diagnostics.Process.Start(exeFile, "/updated " + pid);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return;
            }
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
    }
}
