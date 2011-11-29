using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LegendasTvScrapper;
using SharpCompress.Common;
using SharpCompress.Reader;
using ShowBot.Services;

namespace ShowBot.DefaultServices {
	public class LegendasTvSubtitler : ISubtitler{
		private string userName;
		private string password;

		public LegendasTvSubtitler(IConfig config) {
			var settings = config.GetConfigurationSettings();
			userName = settings.UserName;
			password = settings.Password;
		}

		public bool GetSubtitleForFile(string movieFilePath) {
			using (var legendasTv = new LegendasTv()) {
				legendasTv.Login(userName, password);
				string fileName = Path.GetFileNameWithoutExtension(movieFilePath);
				var normalizedFileName = NormalizeSearchString(fileName);
				var legendas = legendasTv.Search(normalizedFileName);
				if (legendas.Count == 0) {
					return false;
				}
				string subtitlesZipFile = legendasTv.DownloadLegenda(legendas.First().Id);
				try {
					var tempDiretory = CreateTempDirectory();
					try {
						ExctractFilesFromArchive(subtitlesZipFile, tempDiretory);

						var entriesList = (from string file in Directory.GetFiles(tempDiretory) select Path.GetFileName(file)).ToList();
						var bestMatch = GetBestMatch(fileName, entriesList);
						if (string.IsNullOrEmpty(bestMatch)) {
							return false;
						}

						var destinationSubtitleFile = Path.Combine(Path.GetDirectoryName(movieFilePath), fileName + Path.GetExtension(bestMatch));
						if (File.Exists(destinationSubtitleFile)) {
							File.Delete(destinationSubtitleFile);
						}
						File.Move(Path.Combine(tempDiretory, bestMatch), destinationSubtitleFile);

						return true;
					} finally {
						Directory.Delete(tempDiretory, true);
					}
				} finally {
					File.Delete(subtitlesZipFile);
				}
			}
		}

		private static void ExctractFilesFromArchive(string archiveFile, string extractToPath) {
			using (var zipFileStream = new FileStream(archiveFile, FileMode.Open, FileAccess.Read)) {
				var reader = ReaderFactory.Open(zipFileStream);
				while (reader.MoveToNextEntry()) {
					if (!reader.Entry.IsDirectory) {
						Console.WriteLine(reader.Entry.FilePath);
						reader.WriteEntryToDirectory(extractToPath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
					}
				}
			}
		}

		private static string CreateTempDirectory() {
			var tempFile = Path.GetTempFileName();
			File.Delete(tempFile);
			var tempDirectoryPath = Path.Combine(Path.GetDirectoryName(tempFile), Path.GetFileNameWithoutExtension(tempFile));
			Directory.CreateDirectory(tempDirectoryPath);
			return tempDirectoryPath;
		}

		private static string GetBestMatch(string needle, IEnumerable<string> haystack) {
			var delimiters = new char[] {'.', '-', ' '};
			var needleParts = needle.Split(delimiters);
			var bestMatchRatioSoFar = 0;
			var bestMatchSubjectSoFar = string.Empty;
			var partsCount = needleParts.Length;
			foreach (string subject in haystack) {
				int matches = 0;
				foreach (string needlePart in needleParts) {
					if (subject.Contains(needlePart)) {
						matches++;
					}
				}
				var matchRatio = (matches*100)/partsCount;
				if (matchRatio > bestMatchRatioSoFar) {
					bestMatchRatioSoFar = matchRatio;
					bestMatchSubjectSoFar = subject;
				}
			}
			return bestMatchSubjectSoFar;
		}

		private static readonly Regex showNameEpisodeAndSeasonRegularExpression = new Regex(@"(.*?)\.S?(\d{1,2})E?(\d{2})\.(.*)", RegexOptions.IgnoreCase);

		private static string NormalizeSearchString(string movieFileName) {
			var matches = showNameEpisodeAndSeasonRegularExpression.Match(movieFileName);
			if (matches.Success) {
				return matches.Groups[1].ToString().Trim() + " S" + matches.Groups[2].ToString() + "E" + matches.Groups[3];
			}
			return movieFileName;
		}
	}
}
