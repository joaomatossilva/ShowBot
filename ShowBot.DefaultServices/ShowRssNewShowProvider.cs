using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using Argotic.Syndication;

namespace ShowBot.DefaultServices {
	public class ShowRssNewShowProvider : INewShowsProvider {

		private readonly string feedUrl;

		public ShowRssNewShowProvider(IConfig config) {
			var settings = config.GetConfigurationSettings();
			feedUrl = settings.FeedUrl;
		}

		public IEnumerable<Show> GetNewShowsSince(DateTime sinceDate) {

			RssFeed feed = RssFeed.Create(
				new Uri(feedUrl), 
				new Argotic.Common.SyndicationResourceLoadSettings {
					RetrievalLimit = 10,
					AutoDetectExtensions = true,
			});

			var newShows = from item in feed.Channel.Items
						   where item.PublicationDate >= sinceDate
						   select new Show { Title = item.Title, TorrentFile = item.Link.ToString() };

			return newShows;
		}
	}
}
