using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowBot.Model {
	public class Download {
		public int Id { get; set; }
		public string TorrentFile { get; set; }
		public DownloadStatus Status { get; set; }
		public double Progress { get; set; }
		public string Path { get; set; }
		public IEnumerable<DownloadFile> Files { get; private set; }

		public Download() {
			Files = new DownloadFile[0];
			Status = DownloadStatus.NotStarted;
		}

		public Download(IList<DownloadFile> files) {
			Files = files;
			Status = DownloadStatus.NotStarted;
		}
	}

	public enum DownloadStatus { NotStarted = 0, InProgress, Finished };

	public class DownloadFile {
		public int Id { get; set; }
		public string Name { get; set; }
		public int Lenght { get; set; }
	}
}
