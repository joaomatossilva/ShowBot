using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;

namespace ShowBot.DefaultServices {
	public class TransmissionNetDownloader : IDownloader {
		public Download AddDownload(Show showToDownload) {
			return new Download { 
				Id = 1, 
				Status = DownloadStatus.NotStarted, 
				TorrentFile = showToDownload.TorrentFile, 
				Path = string.Empty 
			};
		}

		public IEnumerable<Download> GetStatus() {
			return new List<Download>();
		}

		public void RemoveDownload(Download downloadToRemove) {			
		}
	}
}
