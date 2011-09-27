using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Dynamic;
using System.Xml.Linq;

namespace ShowBot.Infrastructure {
	public class DynamicConfigSection : ConfigurationSection {

		private dynamic settings;

		public DynamicConfigSection() {
			settings = new ExpandoObject();
		}

		protected override void DeserializeSection(System.Xml.XmlReader reader) {
			string xml = reader.ReadOuterXml();
			var doc = XDocument.Load(reader);
			var nodes = from node in doc.Document.Root.Descendants()
						select node;
			foreach (var n in nodes) {
				var settingsDict = settings as IDictionary<String, object>;
				settingsDict[n.Name.LocalName] = n.Value.Trim();
			}
			int i = 0;
			//base.DeserializeSection(reader);
		}

		public dynamic ConfigurationSettings {
			get {
				return settings;
			}
		}

		protected override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode) {
			return base.SerializeSection(parentElement, name, saveMode);
		}
	}
}
