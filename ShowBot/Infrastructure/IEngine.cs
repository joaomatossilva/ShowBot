using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowBot.Infrastructure {
	public interface IEngine {
		void CheckForNewShows(DateTime lastExecutionDate);
		void CheckStatus();
	}
}
