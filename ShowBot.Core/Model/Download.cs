using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowBot.Model {
	public class Download {
		public int Id { get; set; }
		public string TorrentFile { get; set; }
		public DownloadStatus Status { get; set; }
		public int Progress { get; set; }
		public string Path { get; set; }
		public IEnumerable<string> Files { get; private set; }

		public Download() {
			Files = new List<string>();
			Status = DownloadStatus.NotStarted;
		}
	}

	public enum DownloadStatus { NotStarted = 0, InProgress, Finished };
}
