using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using System.Threading.Tasks;
using ShowBot.Infrastructure;
using System.IO;
using log4net;

namespace ShowBot {
	public class Engine : IEngine {
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IDownloader downloader;
		private readonly INewShowsProvider newShowsProvider;
		private readonly ISubtitler subtitler;
		private readonly INotifier notifier;

		private readonly string[] publicTrackers;
		private readonly string[] privateTrackers;

		public Engine(IDownloader downloader, INewShowsProvider newShowsProvider, ISubtitler subtitler, INotifier notifier, IConfig config) {
			this.downloader = downloader;
			this.newShowsProvider = newShowsProvider;
			this.subtitler = subtitler;
			this.notifier = notifier;

			var settings = config.GetConfigurationSettings();
			publicTrackers = settings.PublicTrackers.Split(',');
			privateTrackers = settings.PrivateTrackers.Split(',');
		}

		public void CheckForNewShows(DateTime lastExecutionDate) {
			Log.Info("Checking for new shows...");
			var newShows = newShowsProvider.GetNewShowsSince(lastExecutionDate);
			Log.InfoFormat("{0} new shows found.", newShows.Count());

			foreach (var newShow in newShows) {
				Log.InfoFormat("Downloading show {0} : {1}", newShow.Title, newShow.TorrentFile);
				try {
					var download = downloader.AddDownload(newShow);
					Log.DebugFormat("Show is beeing downloaded with id {0}", download.Id);
					notifier.Notify(string.Format("Started downloading the show {0}", newShow.Title));
				} catch (Exception ex) {
					Log.ErrorFormat("Error adding show {0} :{1} - {2}", newShow.Title, ex);
				}
			}
		}

		private void EnsurePublicTrackers(Download download) {
			foreach (var publicTracker in publicTrackers.Where(publicTracker => !download.Trackers.Contains(publicTracker))) {
				downloader.AddTracker(download, publicTracker);
			}
		}

		private bool IsPrivateTorrent(Download download) {
			return privateTrackers.Any(privateTracker => download.Trackers.Any(tracker => tracker.Contains(privateTracker)));
		}

		public void CheckStatus() {
			Log.InfoFormat("Checking for download status");
			var currentDownloads = downloader.GetStatus();
			Log.DebugFormat("{0} currently active downloads", currentDownloads.Count());

			foreach (var download in currentDownloads) {
				var privateTorrent = IsPrivateTorrent(download);
				Console.WriteLine("{3}:{0} is at {1} - {2}", download.Id, download.Progress, download.Status.ToString(), privateTorrent ? "private" : "public");
				if (!privateTorrent)
					EnsurePublicTrackers(download);
				if (!privateTorrent && download.Status == Model.DownloadStatus.Finished) {
					HandleFinishedDownload(download);
				}
			}
		}

		private void HandleFinishedDownload(Download finishedDownload) {
			try {
				Log.InfoFormat("Handling finished download id {0}...", finishedDownload.Id);
				downloader.PauseDownload(finishedDownload);
				string movieFile = GuessMovieFile(finishedDownload.Path, finishedDownload.Files);
				string movieFullPath = Path.Combine(finishedDownload.Path, movieFile);
				Log.DebugFormat("Getting subtitles for movie {0}", movieFullPath);
				bool couldFindSubtitle = subtitler.GetSubtitleForFile(movieFullPath);
				Log.DebugFormat("Subtitles found? {0}", couldFindSubtitle);
				if (!couldFindSubtitle) {
					Log.DebugFormat("Getting subtitles for torrent name {0}", finishedDownload.Name);
					couldFindSubtitle = subtitler.GetSubtitleForName(finishedDownload.Name, movieFullPath);
					Log.DebugFormat("Subtitles found? {0}", couldFindSubtitle);
				}
				if (couldFindSubtitle) {
					Log.DebugFormat("Removing finished download...");
					downloader.RemoveDownload(finishedDownload);
					Log.InfoFormat("The movie {0} is completed", movieFile);
					notifier.Notify(String.Format("The movie {0} is completed", movieFile));
				}
			} catch (Exception ex) {
				Log.Error("Error handling finished download", ex);				
				throw;
			}
		}

		private string GuessMovieFile(string path, IEnumerable<DownloadFile> files) {
			var biggestFile = files.OrderByDescending(file => file.Length).First();
			return biggestFile.Name;
		}
	}
}
