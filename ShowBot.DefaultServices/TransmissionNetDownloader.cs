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
				var torrent = transmission.AddTorrent(new Uri(torrentFile));
				return new Download {
					Id = torrent.id,
					Status = DownloadStatus.NotStarted,
					TorrentFile = showToDownload.TorrentFile,
					Path = string.Empty
				};
			} finally {
				File.Delete(torrentFile);
			}
		}

		public IEnumerable<Download> GetStatus() {
			var torrents = transmission.CheckStatus();
			var statusTorrents = from torrent in torrents
								 select new Download(
										(from file in torrent.files
										 select new DownloadFile { Name = file.name, Lenght = file.lenght }).ToList()
									 ) {
										 Id = torrent.id,
										 Path = HandleDownloadDir(torrent.downloadDir),
										 Progress = torrent.percentDone,
										 Status = torrent.percentDone == 1.0 ? DownloadStatus.Finished : DownloadStatus.InProgress,
										 TorrentFile = torrent.torrentfile
									 };
			return statusTorrents;
		}

		public void RemoveDownload(Download downloadToRemove) {
			transmission.RemoveTorrent(downloadToRemove.Id);
		}

		private string HandleDownloadDir(string torrentDownloadDir) {
			if(string.IsNullOrEmpty(baseDownloadDir)){
				return torrentDownloadDir;
			}
			return Path.Combine(baseDownloadDir, Path.IsPathRooted(torrentDownloadDir) ? torrentDownloadDir.Substring(1) : torrentDownloadDir);
		}

		private string DownloadTorrentFile(string torrentUrl) {
			string torrentFile = Path.GetTempFileName();
			try {
				WebClient client = new WebClient();
				client.DownloadFile(torrentUrl, torrentFile);
			} catch {
				if (File.Exists(torrentFile)) {
					File.Delete(torrentFile);
				}
				throw;
			}
			return torrentFile;
		}
	}
}
