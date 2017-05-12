using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
	public int connectionId;
}

public class VRServer : MonoBehaviour {

	private const int MAX_CONNECTIONS = 8;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	private bool isStarted = false;
	private byte error;

	private List<ServerClient> clients = new List<ServerClient>();

	private Vector3 HMDLastPos;
	private Vector3 LeftControllerLastPos;
	private Vector3 RightControllerLastPos;

	private float HMDThreshold = .01f;
	private float LeftControllerThreshold = .01f;
	private float RightControllerThreshold = .01f;

	public Transform HMD;
	public Transform LeftController;
	public Transform RightController;

	private float timer;

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

	void Update()
	{
		timer += Time.deltaTime;
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
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
					Debug.Log("Player " + connectionId + " has connected");
					OnConnection(connectionId);
				break;
			case NetworkEventType.DataEvent:
				break;
			case NetworkEventType.DisconnectEvent:
					Debug.Log("Player " + connectionId + " has disconnected");
					OnDisconnection(connectionId);
				break;
		}

		if(timer > .05f)
		{
			SendVRModelInfo();
			timer = 0;
		}
	}

	private void OnConnection (int cnnId)
	{
		//Add client to a list
		ServerClient c = new ServerClient();
		c.connectionId = cnnId;
		clients.Add(c);

		Send("YOURCNN|" + cnnId, reliableChannel, cnnId);
	}

	private void OnDisconnection(int cnnId)
	{
		// Remove this player form our client list
		clients.Remove(clients.Find(x => x.connectionId == cnnId));

		// Tell everyone that somebody else has disconnected
		Send("DC|" + cnnId, reliableChannel, clients);
	}
	
	private void Send(string message, int channelId, int cnnId)
	{
		List<ServerClient> c = new List<ServerClient>();
		c.Add(clients.Find(x=> x.connectionId == cnnId));
		Send(message, channelId, c);
	}

	private void Send(string message, int channelId, List<ServerClient> c)
	{
		// Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);

		foreach(ServerClient sc in c)
		{
			NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
		}
	}

	private void SendVRModelInfo()
	{
		if(Vector3.Distance(HMDLastPos, HMD.position) > HMDThreshold)
		{
			string msg = "HMD|" + HMD.position.x + "|" 
			+ HMD.position.y + "|" 
			+ HMD.position.z + "|" 
			+ HMD.rotation.eulerAngles.x + "|" 
			+ HMD.rotation.eulerAngles.y + "|" 
			+ HMD.rotation.eulerAngles.z;
			Send(msg, unreliableChannel, clients);
			HMDLastPos = HMD.position;
		}
		if(Vector3.Distance(LeftControllerLastPos, LeftController.position) > LeftControllerThreshold)
		{
			string msg = "LeftController|" + LeftController.position.x + "|" 
			+ LeftController.position.y + "|" 
			+ LeftController.position.z + "|" 
			+ LeftController.rotation.eulerAngles.x + "|" 
			+ LeftController.rotation.eulerAngles.y + "|" 
			+ LeftController.rotation.eulerAngles.z;
			Send(msg, unreliableChannel, clients);
			LeftControllerLastPos = LeftController.position;
		}
		if(Vector3.Distance(RightControllerLastPos, RightController.position) > RightControllerThreshold)
		{
			string msg = "RightController|" + RightController.position.x + "|" 
			+ RightController.position.y + "|" 
			+ RightController.position.z + "|" 
			+ RightController.rotation.eulerAngles.x + "|" 
			+ RightController.rotation.eulerAngles.y + "|" 
			+ RightController.rotation.eulerAngles.z;
			Send(msg, unreliableChannel, clients);
			RightControllerLastPos = RightController.position;
		}
	}

}
