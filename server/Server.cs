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
			System.Console.WriteLine(e.Message);
			System.Console.WriteLine();
			try { CliParser.Parse<Options>(new string[] {"-h"}); }
			catch (clipr.Core.ParserExit) { }
		}   catch (clipr.Core.ParserExit) { }
	}

	const double MANAGE_FPS = 1;
	const int LOCK_TIMEOUT = 5000;
	
	static void Listen(ushort port) {
		Log.Init("log/server.txt", Log.DEBUG);
		
		var server = new Server();
		var world  = new World();
		world.Load(server.resManager);
		//world.Init();
		
		Thread  logicThread = new Thread(() => server.logicLoop(world));
		Thread listenThread = new Thread(() => server.listen(port));
		logicThread.Start();
		listenThread.Start();
		
		while (!server.done) {
			Thread.Sleep((int)(1000 / MANAGE_FPS));
		}
		Log.Info("Server done. Joining threads.");
		logicThread.Join((int)(1000 / MANAGE_FPS));
		listenThread.Join((int)(1000 / MANAGE_FPS));
		logicThread.Abort();
		listenThread.Abort();
	}
	
	private Server() {
		done = false;
		resManager = new Res.Manager("gamedata");
		Log.Info("Loading packages...");
		foreach (var package in resManager.Packages) {
			try {
				resManager.LoadPackage(package);
			} catch(System.IO.IOException e) {
				Log.Warn("{0}", e);
				continue;
			}
		}
	}
	
	private void logicLoop(World world) {
		var prev = DateTime.Now;
		Log.Info("Logic loop begin...");
		while (!done) {
			var now = System.DateTime.Now;
			double diff = (now - prev).TotalSeconds;
			if (diff > 1.0 / Net.LogicalFPS) {
				//world.Update(diff);
			} else {
                Thread.Sleep((prev + new TimeSpan(0, 0, 0, 0, 1000 / Net.LogicalFPS)) - now);
				diff = (System.DateTime.Now - prev).TotalSeconds;
				//world.Update(diff);
			}
			prev = now;
		}
	}
	
	private void listen(ushort port) {
		var clientThreads = new List<Thread>();
		Log.Info("Listening for clients on port {0}.", port);
		try {
			var ip = System.Net.Dns.GetHostEntry("localhost").AddressList[0];
			var listener = new TcpListener(ip, port);
			listener.Start();
			while (!done) {
				var tcp = listener.AcceptTcpClient();
				Thread clientThread = new Thread(() => handleNewClient(tcp));
				clientThreads.Add(clientThread);
				clientThread.Start();
			}
		} catch (ThreadAbortException) {
		} catch (SocketException e) {
			Log.Warn("{0}", e);
		} finally {
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
			req.package   = resManager.Packages[res.Name.Package].Name;
			req.name      = res.Name.ID;
			req.hash      = res.Hash;
			req.sessionID = res.SessionID;
			checkResourcesMessage.Resources.Add(req);
			Log.Info("{0} requested.", res.Name.ID);
		}
		resManagerLock.ReleaseReaderLock();
        Net.SendMessage(tcp, checkResourcesMessage);
		
		// get resource request
        var request = Net.ReceiveMessage<Net.CMessage.RequestResources>(tcp);
		
		// send missing resources
		foreach (ushort sessionID in request.Resources) {
			resManagerLock.AcquireReaderLock(LOCK_TIMEOUT);
			Res.Res res = resManager.SessionIDResourceDictionary[sessionID];
            Net.SendMessage(tcp, new Net.SMessage.Resource(sessionID, res.Type, res.Data));
		}
		
		// get ready message
        Net.ReceiveMessage<Net.CMessage.Ready>(tcp);
	}
	
	private readonly Res.Manager resManager;
	private readonly ReaderWriterLock resManagerLock = new ReaderWriterLock();
	
	private bool done;
}
