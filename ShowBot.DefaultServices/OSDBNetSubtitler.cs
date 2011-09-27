using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using OSDBnet;
using System.IO;

namespace ShowBot.DefaultServices {
	public class OSDBNetSubtitler : ISubtitler {

		private readonly string userAgent;

		public OSDBNetSubtitler(IConfig config) {
			var settings = config.GetConfigurationSettings();
			userAgent = settings.UserAgent;
		}

		public bool GetSubtitleForFile(string movieFilePath) {
			using (IAnonymousClient client = Osdb.Login("", userAgent)) {
				var subtitlesFound = client.SearchSubtitles(movieFilePath);
				if (subtitlesFound.Count == 0) {
					return false;
				}
				client.DownloadSubtitleToPath(Path.GetDirectoryName(movieFilePath), subtitlesFound.First());
			}
			return true;
		}
	}
}
