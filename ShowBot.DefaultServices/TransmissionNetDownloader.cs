using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using TransmissionNet;
using System.IO;
using System.Net;

namespace ShowBot.DefaultServices {
	public class TransmissionNetDownloader : IDownloader {

		private readonly ITransmission transmission;
		private readonly string baseDownloadDir;


		public TransmissionNetDownloader(IConfig config) {
			var settings = config.GetConfigurationSettings();
			string url = settings.TransmissionUrl;
			transmission = new TransmissionRPC(new Uri(url));
			baseDownloadDir = settings.BaseDownloadDir;
		}

		public Download AddDownload(Show showToDownload) {
			string torrentFile = DownloadTorrentFile(showToDownload.TorrentFile);
			try {
				Console.WriteLine("Adding torrent {0}", new Uri(torrentFile));
				var addedTorrent = transmission.AddTorrent(new Uri(torrentFile));

				var torrent = (from torrentStatus in transmission.CheckStatus()
							   where torrentStatus.id == addedTorrent.id
							   select torrentStatus).Single();

				return BuildDownloadFromTorrent(torrent);
			} finally {
				File.Delete(torrentFile);
			}
		}

		public IEnumerable<Download> GetStatus() {
			var torrents = transmission.CheckStatus();
			var statusTorrents = from torrent in torrents
								 select BuildDownloadFromTorrent(torrent);
			return statusTorrents;
		}

		public void PauseDownload(Download downloadToPause) {
			transmission.StopTorrent(downloadToPause.Id);
		}

		public void RemoveDownload(Download downloadToRemove) {
			transmission.RemoveTorrent(downloadToRemove.Id);
		}

		public void AddTracker(Download download, string tracker) {
			transmission.AddTracker(download.Id, tracker);
		}

		private string HandleDownloadDir(string torrentDownloadDir) {
			if (string.IsNullOrEmpty(baseDownloadDir)) {
				return torrentDownloadDir;
			}
			return Path.Combine(baseDownloadDir, Path.IsPathRooted(torrentDownloadDir) ? torrentDownloadDir.Substring(1) : torrentDownloadDir);
		}

		private static string DownloadTorrentFile(string torrentUrl) {
			string torrentFile = Path.GetTempFileName();
			try {
				var client = new WebClient();
				client.DownloadFile(torrentUrl, torrentFile);
			} catch {
				if (File.Exists(torrentFile)) {
					File.Delete(torrentFile);
				}
				throw;
			}
			return torrentFile;
		}

		private Download BuildDownloadFromTorrent(TorrentStatus torrent) {
			return new Download(
				(from file in torrent.files
				 select new DownloadFile { Name = file.name, Length = file.length }).ToList(),
				(from tracker in torrent.trackers
				 select tracker.announce).ToList()
				) {
					Name = torrent.name,
					Id = torrent.id,
					Path = HandleDownloadDir(torrent.downloadDir),
					Progress = torrent.percentDone,
					Status = Math.Abs(torrent.percentDone - 1.0) < 0.0001 ? DownloadStatus.Finished : DownloadStatus.InProgress,
					TorrentFile = torrent.torrentfile
				};
		}
	}
}
