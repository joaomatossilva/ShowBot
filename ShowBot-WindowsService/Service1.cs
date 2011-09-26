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

namespace ShowBot_WindowsService {
	public partial class Service1 : ServiceBase {
		private readonly IKernel kernel;
		private readonly IEngine engine;
		private readonly Timer timer;
		private DateTime lastTimeChecked;

		public Service1() {
			InitializeComponent();
			kernel = new StandardKernel(new DefaultNinjectModule());
			engine = kernel.Get<IEngine>();
			timer = new Timer(60000);
			timer.Enabled = false;
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			this.lastTimeChecked = DateTime.Now;
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e) {
			var dateNow = DateTime.Now;
			engine.CheckForNewShows(lastTimeChecked);
			lastTimeChecked = dateNow;
			engine.CheckStatus();
		}

		protected override void OnStart(string[] args) {
			timer.Enabled = true;
		}

		protected override void OnStop() {
			timer.Enabled = false;
		}
	}
}
