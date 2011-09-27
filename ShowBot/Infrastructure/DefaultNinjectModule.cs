using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using ShowBot.Services;
using ShowBot.DefaultServices;

namespace ShowBot.Infrastructure {
	public class DefaultNinjectModule : NinjectModule {
		public override void Load() {
			BindDownloader();
			BindNewShowsProvider();
			BindNotifier();
			BindSubtitler();
			Kernel.Bind<IConfig>().ToProvider<ConfigProvider>();
			Kernel.Bind<IEngine>().To<Engine>();			
		}

		public virtual void BindNotifier(){
			Kernel.Bind<INotifier>().To<EMailNotifier>();
		}

		public virtual void BindSubtitler() {
			Kernel.Bind<ISubtitler>().To<OSDBNetSubtitler>();
		}

		public virtual void BindDownloader() {
			Kernel.Bind<IDownloader>().To<TransmissionNetDownloader>();
		}

		public virtual void BindNewShowsProvider(){
			Kernel.Bind<INewShowsProvider>().To<ShowRssNewShowProvider>();
		}
	}
}
