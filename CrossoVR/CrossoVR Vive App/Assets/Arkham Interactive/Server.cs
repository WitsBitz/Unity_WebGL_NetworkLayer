using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
	public int connectionId;
	public string playerName;
}

public class Server : MonoBehaviour {
	private const int MAX_CONNECTIONS = 8;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	private bool isStarted = false;
	private byte error;

	private List<ServerClient> clients = new List<ServerClient>();

	private void Start()
	{
		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();

		reliableChannel = cc.AddChannel(QosType.Reliable);
		unreliableChannel = cc.AddChannel(QosType.Unreliable);

		HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

		hostId = NetworkTransport.AddHost(topo, port, null);
		webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

		isStarted = true;
	}

	private void Update()
	{
		if(!isStarted)
			return;

		int recHostId; 
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
		switch (recData)
		{
			case NetworkEventType.ConnectEvent:    //2
				Debug.Log("Player " + connectionId + " has connected");
				OnConnection(connectionId);
				break;
			case NetworkEventType.DataEvent:       //3
				string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
				Debug.Log("Recieving from" + connectionId + " : " + msg);

				string[] splitMsg = msg.Split('|');

				switch(splitMsg[0])
				{
					case "NAMEIS":
						OnNameIs(connectionId, splitMsg[1]);
						break;

					case "CNN":
						break;

					case "DC":
						break;
				}
				break;
			case NetworkEventType.DisconnectEvent: //4
				Debug.Log("Player " + connectionId + " has disconnected");
				break;
		}		
	}

	private void OnConnection (int cnnId)
	{
		//Add client to a list
		ServerClient c = new ServerClient();
		c.connectionId = cnnId;
		c.playerName = "TEMP";
		clients.Add(c);

		string msg = "ASKNAME|" + cnnId + "|";
		foreach(ServerClient sc in clients)
			msg += sc.playerName + '%' + sc.connectionId + '|';
		
		msg = msg.Trim('|');

		// Example msg: ASKNAME|3|DAVE%1|MICHAEL%2|TEMP%3

		Send(msg, reliableChannel, cnnId);
	}

	private void OnNameIs(int cnnId, string pName)
	{
		//Link the name to the connectionId
		clients.Find(x => x.connectionId == cnnId).playerName = pName;

		//Tell everybody that a new player has connected
		Send("CNN|" + pName + '|' + cnnId, reliableChannel, clients);
	}

	private void Send(string message, int channelId, int cnnId)
	{
		List<ServerClient> c = new List<ServerClient>();
		c.Add(clients.Find(x=> x.connectionId == cnnId));
		Send(message, channelId, c);
	}

	private void Send(string message, int channelId, List<ServerClient> c)
	{
		Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);

		foreach(ServerClient sc in c)
		{
			NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
		}
	}
}
