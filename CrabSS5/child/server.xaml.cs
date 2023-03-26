using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Timer = System.Timers.Timer;

namespace CrabSS5.child
{
    /// <summary>
    /// server.xaml 的交互逻辑
    /// </summary>
    public partial class server : HandyControl.Controls.Window
    {
        public static Timer timer = new(1000);
        public static PerformanceCounter cpuPer_ = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        //public static PerformanceCounter ramAvailable_ = new("Memory", "Available MBytes");
        public static PerformanceCounter ramPer_ = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        public server()
        {
            InitializeComponent();
            Thread.Sleep(2000);
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
                CPURing.Value = Math.Round(cpuPer_.NextValue(),2);
                RamRing.Value = Math.Round(ramPer_.NextValue(), 2);
                CPU.Text = CPURing.Value + "%";
                RAM.Text = RamRing.Value + "%";
                CrabSS_Ram.Text = GetMemory();
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
    }
}
