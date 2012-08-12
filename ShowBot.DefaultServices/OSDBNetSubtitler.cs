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
		private readonly string[] languages;

		public OSDBNetSubtitler(IConfig config) {
			var settings = config.GetConfigurationSettings();
			userAgent = settings.UserAgent;
			languages = ((string)settings.Languages).Split(',');
		}

		public bool GetSubtitleForFile(string movieFilePath) {
			using (IAnonymousClient client = Osdb.Login(userAgent)) {
				var subtitlesFound = client.SearchSubtitlesFromFile(languages.Aggregate((n,m) => n + "," + m), movieFilePath);
				var enforcedLanguageSubtitles = from subtitle in subtitlesFound
												where languages.Any( (lang) => lang.Equals(subtitle.LanguageId, StringComparison.InvariantCultureIgnoreCase))
												select subtitle;
				if (enforcedLanguageSubtitles.Count() == 0) {
					return false;
				}
				string downloadedSubtitleFile = client.DownloadSubtitleToPath(Path.GetDirectoryName(movieFilePath), enforcedLanguageSubtitles.First());
				try {
					string movieFileName = Path.GetFileNameWithoutExtension(movieFilePath);
					if (!Path.GetFileNameWithoutExtension(downloadedSubtitleFile).Equals(movieFileName, StringComparison.InvariantCultureIgnoreCase)) {
						string newsubtitleFilePath = Path.Combine(Path.GetDirectoryName(movieFilePath), string.Concat(movieFileName, Path.GetExtension(downloadedSubtitleFile)));
						File.Move(downloadedSubtitleFile, newsubtitleFilePath);
					}
				} catch {
					//soak exceptions from renaming the subtitle file
				}
			}
			return true;
		}

		public bool GetSubtitleForName(string movieName, string movieFilePath) {
			return false;
		}
	}
}
