using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using System.Threading.Tasks;
using ShowBot.Infrastructure;

namespace ShowBot {
	public class Engine : IEngine {
		private readonly IDownloader downloader;
		private readonly INewShowsProvider newShowsProvider;
		private readonly ISubtitler subtitler;
		private readonly INotifier notifier;

		public Engine(IDownloader downloader, INewShowsProvider newShowsProvider, ISubtitler subtitler, INotifier notifier) {
			this.downloader = downloader;
			this.newShowsProvider = newShowsProvider;
			this.subtitler = subtitler;
			this.notifier = notifier;
		}

		public void CheckForNewShows(DateTime lastExecutionDate) {
			var newShows = newShowsProvider.GetNewShowsSince(lastExecutionDate);

			foreach (var newShow in newShows) {
				downloader.AddDownload(newShow);
			}
		}

		public void CheckStatus() {
			var currentDownloads = downloader.GetStatus();

			foreach (var download in currentDownloads) {
				if (download.Status == Model.DownloadStatus.Finished) {
					Task.Factory.StartNew(() => HandleFinishedDownload(download));
				}
			}
		}

		private void HandleFinishedDownload(Download finishedDownload) {
			string movieFile = GuessMovieFile(finishedDownload.Path, finishedDownload.Files);
			bool couldFindSubtitle = subtitler.GetSubtitleForFile(movieFile);
			if (couldFindSubtitle) {
				downloader.RemoveDownload(finishedDownload);
			}
		}

		private string GuessMovieFile(string path, IEnumerable<string> files) {
			return files.First();
		}
	}
}
