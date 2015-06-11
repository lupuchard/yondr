using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using clipr;

class Server {

	[ApplicationInfo(Description = "The yondr engine server.")]
	class Options {
		[NamedArgument('p', "port", Description = "The port to connect to.")]
		public ushort Port { get; set; } = Net.DefaultPort;
	}
	public static void Main(string[] args) {
		try {
			var options = CliParser.Parse<Options>(args);
			Listen(options.Port);
		} catch (ParseException e) {
			Console.WriteLine(e.Message);
			Console.WriteLine();
			try { CliParser.Parse<Options>(new string[] {"-h"}); }
			catch (clipr.Core.ParserExit) { }
		} catch (clipr.Core.ParserExit) { }
	}

	const double MANAGE_FPS = 1;
	const int LOCK_TIMEOUT = 5000;
	
	static void Listen(ushort port) {
		Log.Init("log/server.txt", Log.DEBUG);
		
		var server = new Server();

		var world    = new World();
		var packages = world.Load(server.resManager);
		var scripts = new ScriptManager(world, null, null);
		foreach (var package in packages) {
			scripts.Compile(package);
		}
		scripts.Init();

		var  logicThread = new Thread(() => server.logicLoop(scripts));
		var listenThread = new Thread(() => server.listen(port));
		logicThread.Start();
		listenThread.Start();

		Console.CancelKeyPress += delegate {
			server.done = true;
			Log.Info("Terminating server...");
			logicThread.Join((int)(1000 / MANAGE_FPS));
			logicThread.Abort();
			listenThread.Abort();
		};
	}
	
	private Server() {
		done = false;
		resManager = new Res.Manager("../../../gamedata");
		Log.Info("Loading packages...");
		foreach (var package in resManager.Packages.Values) {
			try {
				resManager.LoadPackage(package);
			} catch(System.IO.IOException e) {
				Log.Warn("{0}", e);
				continue;
			}
		}
		Log.Info("Loaded all {0} packages.", resManager.Packages.Count);
	}
	
	private void logicLoop(ScriptManager scripts) {
		var prev = DateTime.Now;
		Log.Info("Logic loop begin...");
		while (!done) {
			var now = DateTime.Now;
			double diff = (now - prev).TotalSeconds;
			if (diff > 1.0 / Net.LogicalFPS) {
				scripts.Update((float)diff);
			} else {
				Thread.Sleep((prev + new TimeSpan(0, 0, 0, 0, 1000 / Net.LogicalFPS)) - now);
				diff = (DateTime.Now - prev).TotalSeconds;
				scripts.Update((float)diff);
			}
			prev = now;
		}
		Log.Info("Logic loop exited.");
	}
	
	private void listen(ushort port) {
		var clientThreads = new List<Thread>();
		Log.Info("Listening for clients on port {0}.", port);
		try {
			var ip = System.Net.Dns.GetHostEntry("localhost").AddressList[0];
			var listener = new TcpListener(ip, port);
			listener.Start();
			while (true) {
				var tcp = listener.AcceptTcpClient();
				var clientThread = new Thread(() => handleNewClient(tcp));
				clientThreads.Add(clientThread);
				clientThread.Start();
			}
		} catch (ThreadAbortException) {
		} catch (System.IO.EndOfStreamException) {
			Log.Warn("A client disconnected unexpectedly.");
		} catch (SocketException e) {
			Log.Warn("{0}", e.Message);
		} finally {
			Log.Info("Aborting client threads.");
			foreach (Thread thread in clientThreads) {
				thread.Abort();
			}
		}
	}
	
	private void handleNewClient(TcpClient tcp) {
		// TODO: System.Net.Sockets.SocketException

		// send welcome
        Net.SendMessage(tcp, new Net.SMessage.Welcome(true, "howdy"));
		
		// send resource check
        var checkResourcesMessage = new Net.SMessage.CheckResources();
		resManagerLock.AcquireReaderLock(LOCK_TIMEOUT);
		foreach (Res.Res res in resManager.Resources) {
            var req = new Net.SMessage.CheckResources.Res();
			req.package   = res.Package.Name;
			req.name      = res.Name;
			req.hash      = res.Hash;
			req.sessionID = res.SessionID;
			checkResourcesMessage.Resources.Add(req);
		}
		resManagerLock.ReleaseReaderLock();
        Net.SendMessage(tcp, checkResourcesMessage);
		
		// get resource request
        var request = Net.ReceiveMessage<Net.CMessage.RequestResources>(tcp);
		
		// send missing resources
		foreach (ushort sessionID in request.Resources) {
			resManagerLock.AcquireReaderLock(LOCK_TIMEOUT);
			Res.Res res = resManager.ResourceDictionary[sessionID];
            Net.SendMessage(tcp, new Net.SMessage.Resource(sessionID, res.Type, res.Data));
		}
		
		// get ready message
        Net.ReceiveMessage<Net.CMessage.Ready>(tcp);
	}
	
	private readonly Res.Manager resManager;
	private readonly ReaderWriterLock resManagerLock = new ReaderWriterLock();
	
	private bool done;
}
