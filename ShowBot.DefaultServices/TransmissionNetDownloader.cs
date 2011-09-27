using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using TransmissionNet;

namespace ShowBot.DefaultServices {
	public class TransmissionNetDownloader : IDownloader {

		private readonly ITransmission transmission;


		public TransmissionNetDownloader(IConfig config) {
			var settings = config.GetConfigurationSettings();
			string url = settings.TransmissionUrl;
			transmission = new TransmissionRPC(new Uri(url));
		}

		public Download AddDownload(Show showToDownload) {
			var torrent = transmission.AddTorrent(new Uri(showToDownload.TorrentFile));
			return new Download {
				Id = torrent.id,
				Status = DownloadStatus.NotStarted,
				TorrentFile = showToDownload.TorrentFile,
				Path = string.Empty
			};
		}

		public IEnumerable<Download> GetStatus() {
			var torrents = transmission.CheckStatus();
			var statusTorrents = from torrent in torrents
								 select new Download(
										(from file in torrent.files
										select file.name).ToList()
									 ) {
										 Id = torrent.id,
										 Path = torrent.downloadDir,
										 Progress = torrent.percentDone,
										 Status = torrent.isFinished ? DownloadStatus.Finished : DownloadStatus.InProgress,
										 TorrentFile = torrent.torrentfile
								 };
			return new List<Download>();
		}

		public void RemoveDownload(Download downloadToRemove) {
			transmission.RemoveTorrent(downloadToRemove.Id);
		}
	}
}
