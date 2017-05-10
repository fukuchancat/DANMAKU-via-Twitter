using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DANMAKU_via_Twitter
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
		private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "DANMAKU via Twitter");

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (!mutex.WaitOne(0, false))
			{
				MessageBox.Show("Application is already running!","DANMAKU via Twitter");
				mutex.Close();
				mutex = null;
				this.Shutdown();
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
			}
		}
	}
}
