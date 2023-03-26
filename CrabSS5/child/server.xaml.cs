using CrabSS5.helper;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = HandyControl.Controls.MessageBox;
using Timer = System.Timers.Timer;

namespace CrabSS5.child
{
    /// <summary>
    /// server.xaml 的交互逻辑
    /// </summary>
    public partial class server : HandyControl.Controls.Window
    {
        public static Timer timer = new(1000);
        public static Process p = new();
        public string time = "";
        public static PerformanceCounter cpuPer_ = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        //public static PerformanceCounter ramAvailable_ = new("Memory", "Available MBytes");
        public static PerformanceCounter ramPer_ = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        public server()
        {
            InitializeComponent();
            OperationInProgress.Visibility = Visibility.Visible;
            OperationInProgress.IsIndeterminate = true;
            corepath.Text = Properties.Settings.Default.core;
            javapath.Text = Properties.Settings.Default.java_path;
            m1.Value = Properties.Settings.Default.min_ram;
            m2.Value = Properties.Settings.Default.max_ram;
            extra.Text = Properties.Settings.Default.extra;
            stopcmd.Text = Properties.Settings.Default.stop;
            OperationInProgress.Visibility = Visibility.Hidden;
            timer.Elapsed += new ElapsedEventHandler(GetAllUsage);
            timer.AutoReset = true;
            CPURing.IsIndeterminate = false;
            RamRing.IsIndeterminate = false;
            timer.Enabled = true;
        }
        private void GetAllUsage(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                time = DateTime.Now.ToString("G");
                CPURing.Value = Math.Round(cpuPer_.NextValue(),2);
                RamRing.Value = Math.Round(ramPer_.NextValue(), 2);
                CPU.Text = CPURing.Value + "%";
                RAM.Text = RamRing.Value + "%";
                CrabSS_Ram.Text = GetMemory();
                try
                {
                    if (p.HasExited == true)
                    {
                        TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                        tr.Text = $"\n[{time}] 服务器已退出，进程退出码为 {p.ExitCode}\n";
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                        ConsoleOutput.ScrollToEnd();
                        PID.Text = "未知";
                        start.IsEnabled = true;
                        stop.IsEnabled = false;
                        restart.IsEnabled = false;
                        p.CancelErrorRead();
                        p.CancelOutputRead();
                    }
                }
                catch
                {
                    return;
                }
            }));
        }
        public static string GetMemory()
        {
            Process proc = Process.GetCurrentProcess();
            long b = proc.PrivateMemorySize64;
            for (int i = 0; i < 2; i++)
            {
                b /= 1024;
            }
            return b + "MB";
        }

        private async void Down1_Click(object sender, RoutedEventArgs e)
        {
            Down1.IsEnabled = false;
            var result = await WebHelper.DownloadFile("https://gh.flyinbug.top/gh/https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.6%2B10/OpenJDK17U-jdk_x64_windows_hotspot_17.0.6_10.zip", "OpenJDK17U-jdk_x64_windows_hotspot_17.0.6_10.zip", OperationInProgress, Down1);
            if (result == true)
            {
                OperationInProgress.IsIndeterminate = true;
                try
                {   
                    Down1.Content = "下载完成！";
                    OperationInProgress.Visibility = Visibility.Hidden;
                    Down2.IsEnabled = true;
                    System.IO.Compression.ZipFile.ExtractToDirectory($"{Environment.CurrentDirectory}\\OpenJDK17U-jdk_x64_windows_hotspot_17.0.6_10.zip", Environment.CurrentDirectory);
                }
                catch (Exception ex)
                {
                    Down1.Content = "发生错误，点击重试";
                    Down1.IsEnabled = true;
                    HandyControl.Controls.MessageBox.Error("发生错误，未能完成解压。\n" + ex, "发生错误，未能完成解压。");
                }
            }
            else
            {
                Down1.Content = "发生错误，点击重试";
                Down1.IsEnabled = true;
            }
        }
        private async void Down2_Click(object sender, RoutedEventArgs e)
        {
            Down2.IsEnabled = false;
            var result = await WebHelper.DownloadFile("https://gh.flyinbug.top/gh/https://github.com/yushijinhun/authlib-injector/releases/download/v1.2.2/authlib-injector-1.2.2.jar", "authlib-injector-1.2.2.jar", OperationInProgress, Down2);
            if (result == true)
            {
                OperationInProgress.IsIndeterminate = true;
                Down2.Content = "正在解压";
                try
                {
                    Down2.Content = "下载完成！";
                    OperationInProgress.Visibility = Visibility.Hidden;
                    Down2.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Error(ex.ToString(), "=(");
                    Down2.Content = "发生错误，点击重试";
                    Down2.IsEnabled = true;
                }
            }
            else
            {
                Down2.Content = "发生错误，点击重试";
                Down2.IsEnabled = true;
            }
        }
        private async void BrowseCore(object sender, RoutedEventArgs e)
        {
            ServerHelper.CopyCore(corepath);
        }
        private async void BrowseJava(object sender, RoutedEventArgs e)
        {
            ServerHelper.BrowseJava(javapath);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if(corepath.Text == "" || javapath.Text == "" || m1.Value == 0 || m2.Value == 0 || stopcmd.Text == "")
            {
                HandyControl.Controls.MessageBox.Warning("一个或多个必要参数为空", "警告");
            }
            else
            {
                Properties.Settings.Default.core = corepath.Text;
                Properties.Settings.Default.java_path = javapath.Text;
                Properties.Settings.Default.min_ram = (int)m1.Value;
                Properties.Settings.Default.max_ram  = (int)m2.Value;
                Properties.Settings.Default.extra = extra.Text;
                Properties.Settings.Default.stop = stopcmd.Text;
                Properties.Settings.Default.Save();
                File.WriteAllTextAsync($"{Environment.CurrentDirectory}\\eula.txt", "eula=true");
                File.WriteAllTextAsync($"{Environment.CurrentDirectory}\\start.cmd", '"' + javapath.Text + '"' + $" -Xms{m1.Value}M -Xmx{m2.Value}M -jar {corepath.Text} {extra.Text}");
                p.StartInfo.FileName = "start.cmd";
                p.StartInfo.Arguments = "start.cmd";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;
                start.IsEnabled = false;
                Start.IsEnabled = false;
                restart.IsEnabled = true;
                stop.IsEnabled = true;
                p.OutputDataReceived += ColorfulOutput;
                p.ErrorDataReceived += Error;
                p.Exited += Exited;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                PID.Text = p.Id.ToString();
            }
        }
        private void Exited(object sender, EventArgs e)
        {
            TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
            tr.Text = $"\n[{time}] 服务器已退出，进程退出码为 {p.ExitCode}\n";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            ConsoleOutput.ScrollToEnd();
            PID.Text = "未知";
            start.IsEnabled = true;
            stop.IsEnabled = false;
            restart.IsEnabled = false;
            p.CancelErrorRead();
            p.CancelOutputRead();
        }
        private void Error(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                tr.Text = $"\n[{time}] 错误！{outLine.Data} \n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                tr.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
            }));
        }
        private void ColorfulOutput(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                Action methodDelegate = delegate ()
                {
                    if (outLine.Data != null)
                    {
                        TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                        tr.Text = $"\n[{time}] {outLine.Data}\r";
                        Brush brush = Brushes.White;
                        if (outLine.Data.IndexOf("INFO") != -1)
                            brush = Brushes.White;
                        else if (outLine.Data.IndexOf("WARN") != -1)
                        {
                            System.Drawing.Color drawColor = System.Drawing.Color.FromArgb(255, 245, 228, 0);
                            brush = new SolidColorBrush(Color.FromArgb(drawColor.A, drawColor.R, drawColor.G, drawColor.B));
                        }
                        else if (outLine.Data.IndexOf("ERROR") != -1)
                        {
                            System.Drawing.Color drawColor = System.Drawing.Color.FromArgb(255, 214, 0, 21);
                            brush = new SolidColorBrush(Color.FromArgb(drawColor.A, drawColor.R, drawColor.G, drawColor.B));
                        }
                        else
                            brush = Brushes.White;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                        ConsoleOutput.ScrollToEnd();
                    }
                    else
                    {
                        return;
                    }
                };
                Dispatcher.BeginInvoke(methodDelegate);
            }
            catch (Exception ex)
            {
                MessageBox.Error($"抛出异常：{ex.Message}", $"发生了未知错误。\n{ex}");
            }
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                p.StandardInput.WriteLine("stop");
                p.StandardInput.AutoFlush = true;
                p.WaitForExit();
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                tr.Text = $"[{time}] 未能重启服务器。\n{ex} \n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                tr.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                ConsoleOutput.ScrollToEnd();
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                p.StandardInput.WriteLine("stop");
                p.StandardInput.AutoFlush = true;
            }
            catch (Exception ex)
            {
                TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                tr.Text = $"[{time}] 未能关闭服务器。\n{ex}\n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                tr.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                ConsoleOutput.ScrollToEnd();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                p.StandardInput.WriteLine(command.Text);
                p.StandardInput.AutoFlush = true;
            }
            catch (Exception ex)
            {
                TextRange tr = new(ConsoleOutput.Document.ContentEnd, ConsoleOutput.Document.ContentEnd);
                tr.Text = $"[{time}] 无法发送指令，请先启动服务器。如果服务器已启动，请尝试重启进程！\n{ex}\n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                tr.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                ConsoleOutput.ScrollToEnd();
            }
        }
    }
}
