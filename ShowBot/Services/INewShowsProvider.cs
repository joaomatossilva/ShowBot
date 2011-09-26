using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ShowBot.Model;

namespace ShowBot.Services {
	public interface INewShowsProvider {
		IEnumerable<Show> GetNewShowsSince(DateTime sinceDate);
	}
}
