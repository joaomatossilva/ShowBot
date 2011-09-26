using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowBot.Model {
	public class Show {
		public string Title { get; set; }
		public int Season { get; set; }
		public int Episode { get; set; }
		public string TorrentFile { get; set; }
	}
}
