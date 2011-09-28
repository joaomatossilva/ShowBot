using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Unix;
using Mono.Unix.Native;

namespace ShowBot_MonoDaemon {
	class Program {
		static void Main(string[] args) {
			UnixSignal intr = new UnixSignal (Signum.SIGINT);
			UnixSignal term = new UnixSignal (Signum.SIGTERM);
			UnixSignal hup = new UnixSignal (Signum.SIGHUP);
			UnixSignal usr2 = new UnixSignal (Signum.SIGUSR2);

			UnixSignal[] signals = new UnixSignal[] { intr, term, hup, usr2 };

			Console.WriteLine("daemon: engine starting...");

			var service = new Service();
			service.Start(null);

			Console.WriteLine("daemon: engine started...");


			for (bool running = true; running; )
			{
				int idx = UnixSignal.WaitAny(signals);

				if (idx < 0 || idx >= signals.Length) continue;

				Console.WriteLine(string.Format("daemon: received signal {0}",signals[idx].Signum));

				if ((intr.IsSet || term.IsSet)) 
				{
					intr.Reset ();
					term.Reset ();

					Console.WriteLine("daemon: stopping...");

					running = false;
				}
				else if (hup.IsSet)
				{
					// Ignore. Could be used to reload configuration.
					hup.Reset();
				}
				else if (usr2.IsSet)
				{
					usr2.Reset();
					// do something
				}
			}

			service.Stop();
		}
	}
}
