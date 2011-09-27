using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;

namespace ShowBot.Infrastructure {
	public class ConfigProvider : IProvider  {
		public object Create(IContext context) {
			return new DynamicConfigReader(context.Request.Target.Member.ReflectedType.Name);
		}

		public Type Type {
			get { return typeof(DynamicConfigReader); }
		}
	}
}
