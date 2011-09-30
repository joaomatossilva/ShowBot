using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using System.Threading.Tasks;
using ShowBot.Infrastructure;
using System.IO;

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
			Console.WriteLine("Checking for new shows...");
			var newShows = newShowsProvider.GetNewShowsSince(lastExecutionDate);
			Console.WriteLine("{0} new shows found.", newShows.Count());

			foreach (var newShow in newShows) {
				Console.WriteLine("Downloading show {0} : {1}", newShow.Title, newShow.TorrentFile);
				var download = downloader.AddDownload(newShow);
				Console.WriteLine("Show is beeing downloaded with id {0}", download.Id);
			}
		}

		public void CheckStatus() {
			Console.WriteLine("Checking for download status");
			var currentDownloads = downloader.GetStatus();
			Console.WriteLine("{0} currently active downloads", currentDownloads.Count());

			foreach (var download in currentDownloads) {
				Console.WriteLine("{0} is at {1} - {2}", download.Id, download.Progress, download.Status.ToString());
				if (download.Status == Model.DownloadStatus.Finished) {
					HandleFinishedDownload(download);
				}
			}
		}

		private void HandleFinishedDownload(Download finishedDownload) {
			try {
				Console.WriteLine("Handling finished download id {0}...", finishedDownload.Id);
				downloader.PauseDownload(finishedDownload);
				string movieFile = GuessMovieFile(finishedDownload.Path, finishedDownload.Files);
				string movieFullPath = Path.Combine(finishedDownload.Path, movieFile);
				Console.WriteLine("Getting subtitles for movie {0}", movieFullPath);
				bool couldFindSubtitle = subtitler.GetSubtitleForFile(movieFullPath);
				Console.WriteLine("Subtitles found? {0}", couldFindSubtitle);
				if (couldFindSubtitle) {
					Console.WriteLine("Removing finished download...");
					downloader.RemoveDownload(finishedDownload);
					Console.WriteLine("finished download removed.");
					notifier.Notify(String.Format("The movie {0} is completed", movieFile));
				}
			} catch (Exception ex) {
				throw;
			}
		}

		private string GuessMovieFile(string path, IEnumerable<DownloadFile> files) {
			var biggestFile = files.OrderByDescending(file => file.Lenght).First();
			return biggestFile.Name;
		}
	}
}
