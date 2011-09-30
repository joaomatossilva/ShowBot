using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Configuration;
using Ninject;
using ShowBot.Infrastructure;
using ShowBot.DefaultServices;
using ShowBot.Services;
using ShowBot;

namespace ShowBot_MonoDaemon {
	public class Service {
		private const string INTERVAL_KEY = "Interval";
		private const string LAST_CHECK_DATE_KEY = "LastCheckDate";

		private readonly IKernel kernel;
		private readonly IEngine engine;
		private readonly Timer timer;
		private readonly double interval;
		private DateTime lastTimeChecked;

		public Service() {

			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			interval = double.Parse(config.AppSettings.Settings[INTERVAL_KEY].Value);
			var lastTimeChecketString = config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value;
			if (string.IsNullOrEmpty(lastTimeChecketString)) {
				lastTimeChecked = DateTime.Now.AddMilliseconds(-1 * interval);
				config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value = lastTimeChecked.ToString();
				config.Save();
				Console.WriteLine("last check date saved");
			} else {
				lastTimeChecked = DateTime.Parse(lastTimeChecketString);
			}			
			/*
			kernel = new StandardKernel(new DefaultNinjectModule());
			engine = kernel.Get<IEngine>();*/
			
			///************** Temp modification since there is a bug on Ninject
			kernel = new StandardKernel(new DefaultNinjectModule());

			INotifier notifier = new EMailNotifier(new DynamicConfigReader("EMailNotifier"));
			ISubtitler subtitler = new OSDBNetSubtitler(new DynamicConfigReader("OSDBNetSubtitler"));
			IDownloader downloader = new TransmissionNetDownloader(new DynamicConfigReader("TransmissionNetDownloader"));
			INewShowsProvider showsProvider = new ShowRssNewShowProvider(new DynamicConfigReader("ShowRssNewShowProvider"));
			engine = new Engine(downloader, showsProvider, subtitler, notifier);
			///************** End Temp modification 


			timer = new Timer(interval);
			timer.Enabled = false;
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e) {
			ExecuteCheck();
		}

		private void ExecuteCheck() {
			Console.WriteLine("timer tick");
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
			Console.WriteLine("last check date saved");
		}

		public void Start(string[] args) {
			Console.WriteLine("starting service");
			timer.Enabled = true;
			ExecuteCheck();
		}

		public void Stop() {
			Console.WriteLine("stopping service");
			timer.Enabled = false;
		}
	}
}
