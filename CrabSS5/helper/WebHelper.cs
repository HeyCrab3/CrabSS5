using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Net.Http.Handlers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrabSS5.helper
{
    internal class WebHelper
    {
        public static Dispatcher dispatcher= Dispatcher.CurrentDispatcher;
        public static string? GetRequest(string url) {
            HttpClient client = new();
            var result = client.GetAsync(url);
            return result.ToString();
        }
        public static async Task<bool> DownloadFile(string url, string fileName, ProgressBar DownloadProgress, Button btn)
        {
            DownloadProgress.Visibility = Visibility.Visible;
            DownloadProgress.IsIndeterminate = false;
            try
            {
                var progressMessageHandler = new ProgressMessageHandler(new HttpClientHandler());
                progressMessageHandler.HttpReceiveProgress += (_, e) =>
                {
                    dispatcher.BeginInvoke(new Action(delegate
                    {
                        DownloadProgress.Value = e.ProgressPercentage;//下载进度百分比
                        string downloadSpeed = (e.TotalBytes - e.BytesTransferred) / 1048576 + "MB/s";
                        btn.Content = $"正在下载 {e.ProgressPercentage}% ({downloadSpeed})"; // 操作栏展示实时进度
                    }));
                };
                using (var client = new HttpClient(progressMessageHandler))
                using (var filestream = new FileStream(fileName, FileMode.Create))
                {
                    var netstream = await client.GetStreamAsync(url);
                    await netstream.CopyToAsync(filestream);//写入文件
                }
                return true;
            }
            catch (Exception ex)
            {
                btn.Content = "未能下载文件，点击重试";
                DownloadProgress.Visibility = Visibility.Hidden;
                HandyControl.Controls.MessageBox.Show(ex.ToString(), "未能下载文件，以下是错误信息", MessageBoxButton.OK, MessageBoxImage.Error);
                btn.IsEnabled = true;
                return false;
            }
        }
    }
}
