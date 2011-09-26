using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;

namespace ShowBot.DefaultServices {
	public class ShowRssNewShowProvider : INewShowsProvider {
		public IEnumerable<Show> GetNewShowsSince(DateTime sinceDate) {
			return new List<Show>();
		}
	}
}
