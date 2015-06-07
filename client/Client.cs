using System.Net.Sockets;
using System.Collections.Generic;
using clipr;

class Client {
	[ApplicationInfo(Description = "The yondr engine client.")]
	class Options {
		[PositionalArgument(0, MetaVar = "HOST", Description = "The server to connect to.")]
		public string Host { get; set; }

		[NamedArgument('p', "port", Description = "The port to connect to.")]
		public ushort Port { get; set; } = Net.DefaultPort;
	}
	public static void Main(string[] args) {
		try {
			var options = CliParser.Parse<Options>(args);
			string[] split = options.Host.Split(':');
			if (split.Length == 1) {
				Connect(split[0], options.Port);
			} else {
				Connect(split[0], System.Convert.ToUInt16(split[1]));
			}
		} catch (ParseException e) {
			System.Console.WriteLine(e.Message);
			System.Console.WriteLine();
			try { CliParser.Parse<Options>(new string[] {"-h"}); }
			catch (clipr.Core.ParserExit) { }
		} catch (clipr.Core.ParserExit) { }
	}

	public static void Connect(string host, ushort port) {
		Log.Init("log/client.txt", Log.DEBUG);

		Client client = new Client();

		client.connect(host, port);

		Log.Info("Successfully connected.");

		var world = new World();
		var packages = world.Load(client.resManager);

		Log.Info("Starting window...");

		Game game = new Game(client.resManager, world, 600, 600, (d) => {});

		Log.Info("Loading scripts...");

		foreach (EntityGroup g in world.Groups) {
			var comp = g.GetComponent<GraphicalComponent>();
			if (comp != null) comp.Renderer = game.Renderer;
		}

		var scripts = new ScriptManager(world, game.Renderer);
		foreach (var package in packages) {
			scripts.Compile(package);
		}
		scripts.Init();

		game.Run();
	}
	
	private Client() {
		
		resManager = new Res.Manager("cache");
	}
	
	private void connect(string host, ushort port) {
		Log.Info("Connecting to {0} on port {1}.", host, port);
		var tcp = new TcpClient(host, port);
		Log.Info("Connected successfully.");
		
		var welcomeMessage = Net.ReceiveMessage<Net.SMessage.Welcome>(tcp);
		if (welcomeMessage.Is) {
			Log.Info("Client has been welcomed with message '{0}'.", welcomeMessage.Message);
		} else {
			Log.Info("Client has been rejected with message '{0}'.", welcomeMessage.Message);
			return;
		}
		
		Log.Info("Receiving resource request...");
		var resourcesMessage = Net.ReceiveMessage<Net.SMessage.CheckResources>(tcp);
		var resourceRequest  = new Net.CMessage.RequestResources();
		var missingResources = new Dictionary<ushort, Net.SMessage.CheckResources.Res>();
		foreach (var req in resourcesMessage.Resources) {
			Res.Package package;
			if (resManager.Packages.TryGetValue(req.package, out package)) {
				if (resManager.CheckResource(req.name, package, req.hash, req.sessionID)) {
					continue;
				}
			}
			Log.Info("{0} requested.", req.name);
			resourceRequest.Resources.Add(req.sessionID);
			missingResources.Add(req.sessionID, req);
		}
		
		Log.Info("Client lacks {0} resources. Requesting...", resourceRequest.Resources.Count);
		Net.SendMessage(tcp, resourceRequest);
		while (missingResources.Count > 0) {
			var resourceData = Net.ReceiveMessage<Net.SMessage.Resource>(tcp);
			var res = missingResources[resourceData.SessionID];
			missingResources.Remove(resourceData.SessionID);
			Res.Package package;
			if (!resManager.Packages.TryGetValue(res.package, out package)) {
				package = resManager.CreatePackage(res.package);
			}
			resManager.CreateResource(res.name, package, resourceData.ResType,
			                          res.hash, res.sessionID, resourceData.Data);
			Log.Info("Received {0}:{1}.", res.package, res.name);
		}
		
		Log.Info("Declaring ready.");
		Net.SendMessage(tcp, new Net.CMessage.Ready());
	}
	
	private readonly Res.Manager resManager;
}
