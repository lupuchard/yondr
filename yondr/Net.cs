using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class Net {
	
	public const int LogicalFPS = 60;
	public const int NetworkFPS = 20;
	public const ushort DefaultPort = 41414;
	
	/// Messeges that the Server can send to the Client.
	[Serializable]
	public abstract class SMessage {
		
		/// First message. Tells the Client if they are rejected or not and a reason why.
		[Serializable]
		public class Welcome: SMessage {
			public Welcome(bool isActually, string message) {
				Is      = isActually;
				Message = message;
			}
			public bool Is { get; set; }
			public string Message { get; set; }
		}
		
		/// A list of all the required resources.
		[Serializable]
		public class CheckResources: SMessage {
			public CheckResources() {
				Resources = new List<Res>();
			}
			[Serializable]
			public struct Res {
				public string package;
				public string name;
				public long hash;
				public ushort sessionID;
			}
			public List<Res> Resources { get; set; }
		}
		
		/// The data for a particular resource.
		[Serializable]
		public class Resource: SMessage {
			public Resource(ushort sessionID, Res.Type type, byte[] data) {
				SessionID = sessionID;
				ResType   = type;
				Data      = data;
			}
			public ushort SessionID { get; set; }
			public Res.Type ResType { get; set; }
			public byte[] Data      { get; set; }
		}
		
		[Serializable]
		public class Goodbye: SMessage { 
			public Goodbye(string message) {
				Message = message;
			}
			public string Message { get; set; }
		}
	}
	
	
	/// Messeges that the Client can send to the Server.
	[Serializable]
	public abstract class CMessage {
		
		/// A list of all the resources among those mentioned in CheckResources that
		/// the Client does not have and needs the Server to send them.
		[Serializable]
		public class RequestResources: CMessage {
			public RequestResources() {
				Resources = new List<ushort>();
			}
			public List<ushort> Resources { get; set; }
		}
		
		[Serializable]
		public class Ready: CMessage {
			public Ready() { }
		}
		
		[Serializable]
		public class Goodbye: CMessage { 
			public Goodbye(string message) {
				Message = message;
			}
			public string Message { get; set; }
		}
	}
	
	public static void SendMessage<T>(TcpClient tcp, T message) {
		IFormatter formatter = new BinaryFormatter();
		formatter.Serialize(tcp.GetStream(), message);
	}
	public static T ReceiveMessage<T>(TcpClient tcp) {
		IFormatter formatter = new BinaryFormatter();
		return (T)formatter.Deserialize(tcp.GetStream());
	}
}
