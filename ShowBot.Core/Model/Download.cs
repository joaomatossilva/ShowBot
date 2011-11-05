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
		public IEnumerable<string> Trackers { get; private set; } 

		public Download() {
			Files = new DownloadFile[0];
			Trackers = new String[0];
			Status = DownloadStatus.NotStarted;
		}

		public Download(IEnumerable<DownloadFile> files, IEnumerable<string> trackers) {
			Files = files;
			Trackers = trackers;
			Status = DownloadStatus.NotStarted;
		}
	}

	public enum DownloadStatus { NotStarted = 0, InProgress, Finished };

	public class DownloadFile {
		public int Id { get; set; }
		public string Name { get; set; }
		public long Lenght { get; set; }
	}
}
