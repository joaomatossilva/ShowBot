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
using log4net;

namespace ShowBot_MonoDaemon {
	public class Service {
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
				Log.InfoFormat("last check date saved");
			} else {
				lastTimeChecked = DateTime.Parse(lastTimeChecketString);
			}			
			/*
			kernel = new StandardKernel(new DefaultNinjectModule());
			engine = kernel.Get<IEngine>();*/
			
			///************** Temp modification since there is a bug on Ninject
			kernel = new StandardKernel(new DefaultNinjectModule());

			INotifier notifier = new EMailNotifier(new DynamicConfigReader("EMailNotifier"));
			ISubtitler subtitler = new LegendasTvSubtitler(new DynamicConfigReader("LegendasTvSubtitler"));
			IDownloader downloader = new TransmissionNetDownloader(new DynamicConfigReader("TransmissionNetDownloader"));
			INewShowsProvider showsProvider = new ShowRssNewShowProvider(new DynamicConfigReader("ShowRssNewShowProvider"));
			IConfig engineConfig = new DynamicConfigReader("Engine");
			engine = new Engine(downloader, showsProvider, subtitler, notifier, engineConfig);
			///************** End Temp modification 


			timer = new Timer(interval);
			timer.Enabled = false;
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e) {
			try {
				ExecuteCheck();
			} catch (Exception ex) {
				Log.ErrorFormat("General Error {0}: {1} - {2}", DateTime.Now, ex);
			}
		}

		private void ExecuteCheck() {
			var dateNow = DateTime.Now;
			Log.DebugFormat("timer tick at {0}", dateNow);
			Log.DebugFormat("last check: {0}", lastTimeChecked);
			try {
				engine.CheckForNewShows(lastTimeChecked);
				lastTimeChecked = dateNow;
				PersistLastTimeCheck(lastTimeChecked);
			} catch (Exception ex) {
				Log.ErrorFormat("General Error checking for new shows from {0} to {1}: {2} - {3}", lastTimeChecked, dateNow, ex);
			}
			try {
				engine.CheckStatus();
			} catch (Exception ex) {
				Log.ErrorFormat("General Error checking status {0}: {1} - {2}", dateNow, ex);
			}
		}

		private void PersistLastTimeCheck(DateTime lastTimeChecked) {
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			config.AppSettings.Settings[LAST_CHECK_DATE_KEY].Value = lastTimeChecked.ToString();
			config.Save();
			Log.InfoFormat("last check date saved");
		}

		public void Start(string[] args) {
			Log.InfoFormat("starting service");
			timer.Enabled = true;
			ExecuteCheck();
		}

		public void Stop() {
			Log.InfoFormat("stopping service");
			timer.Enabled = false;
		}
	}
}
