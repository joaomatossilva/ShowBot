using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendasTvScrapper {
	public class Legenda {
		public string Id { get; private set; }
		public string Name { get; private set; }

		public Legenda(string id, string name) {
			Id = id;
			Name = name;
		}
	}
}
