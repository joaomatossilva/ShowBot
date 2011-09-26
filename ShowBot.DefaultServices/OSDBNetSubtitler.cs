using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;

namespace ShowBot.DefaultServices {
	public class OSDBNetSubtitler : ISubtitler {
		public bool GetSubtitleForFile(string movieFilePath) {
			return false;
		}
	}
}
