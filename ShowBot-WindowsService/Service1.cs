using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Ninject;
using ShowBot.Infrastructure;
using System.Timers;
using System.Configuration;

namespace ShowBot_WindowsService {
	public partial class Service1 : ServiceBase {

		private const string INTERVAL_KEY = "Interval";
		private const string LAST_CHECK_DATE_KEY = "LastCheckDate";

		private readonly IKernel kernel;
		private readonly IEngine engine;
		private readonly Timer timer;
		private readonly double interval;
		private DateTime lastTimeChecked;

		public Service1() {
			InitializeComponent();

			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			interval = double.Parse(config.AppSettings.Settings[INTERVAL_KEY].Value);
			var lastTimeChecketString = config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value;
			if (string.IsNullOrEmpty(lastTimeChecketString)) {
				lastTimeChecked = DateTime.Now.AddMilliseconds(-1 * interval);
				config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value = lastTimeChecked.ToString();
				config.Save();
			} else {
				lastTimeChecked = DateTime.Parse(lastTimeChecketString);
			}			

			kernel = new StandardKernel(new DefaultNinjectModule());
			engine = kernel.Get<IEngine>();
			timer = new Timer(interval);
			timer.Enabled = false;
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e) {
			var dateNow = DateTime.Now;
			engine.CheckForNewShows(lastTimeChecked);
			lastTimeChecked = dateNow;
			PersistLastTimeCheck(lastTimeChecked);
			engine.CheckStatus();
		}

		private void PersistLastTimeCheck(DateTime lastTimeChecked) {
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value = lastTimeChecked.ToString();
			config.Save();
		}

		protected override void OnStart(string[] args) {
			timer.Enabled = true;
		}

		protected override void OnStop() {
			timer.Enabled = false;
		}
	}
}
