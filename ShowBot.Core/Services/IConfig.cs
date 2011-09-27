using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowBot.Services {
	public interface IConfig {
		dynamic GetConfigurationSettings();
	}
}
