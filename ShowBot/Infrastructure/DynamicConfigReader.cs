using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using System.Configuration;

namespace ShowBot.Infrastructure {
	public class DynamicConfigReader : IConfig {

		private string configSectionName;

		public DynamicConfigReader(string configSectionName) {
			this.configSectionName = configSectionName;
		}

		public dynamic GetConfigurationSettings() {

			var section = (DynamicConfigSection) ConfigurationManager.GetSection(configSectionName);

			return section.ConfigurationSettings;
		}
	}
}
