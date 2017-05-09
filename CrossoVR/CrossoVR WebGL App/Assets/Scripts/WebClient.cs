using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WebPlayer
{
	public int connectionId;
}

public class WebClient : MonoBehaviour {
	private const int MAX_CONNECTIONS = 8;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	private int ourClientId;
	private int connectionId;

	private float connectionTime;
	private bool isConnected = false;
	private bool isStarted = false;
	private byte error;

	public List<WebPlayer> players = new List<WebPlayer>();

	private Vector3 HMDPos;
	private Vector3 LeftControllerPos;
	private Vector3 RightControllerPos;

	private Quaternion HMDRot;
	private Quaternion LeftControllerRot;
	private Quaternion RightControllerRot;

	public Transform HMD;
	public Transform LeftController;
	public Transform RightController;


	public void Connect()
	{
		//Connect to host

		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();

		reliableChannel = cc.AddChannel(QosType.Reliable);
		unreliableChannel = cc.AddChannel(QosType.Unreliable);

		HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

		hostId = NetworkTransport.AddHost(topo, 0);

		string ip = GameObject.Find("IPInput").GetComponent<InputField>().text;
		Debug.Log(ip);
		connectionId = NetworkTransport.Connect(hostId, ip, port, 0, out error);

		connectionTime = Time.time;
		isConnected = true;

	}

	private void Update()
	{
		if(!isConnected)
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
			case NetworkEventType.DataEvent:       //3
				string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
				// Debug.Log("Recieving " + msg);

				string[] splitMsg = msg.Split('|');

				switch(splitMsg[0])
				{

					case "YOURCNN":
						OurConnection(connectionId);
						break;

					case "HMD":
						PositionHMD(splitMsg);
						break;

					case "LeftController":
						PositionLeftController(splitMsg);
						break;

					case "RightController":
						PositionRightController(splitMsg);
						break;
				}
				break;
		}

		PositionModels();
	}

	private void OurConnection(int cnnId)
	{	
		ourClientId = cnnId;
		GameObject.Find("Canvas").SetActive(false);
		isStarted = true;
		WebPlayer p = new WebPlayer();
		p.connectionId = cnnId;
		players.Add(p);
	}

	private void Send(string message, int channelId)
	{
		Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);
		NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
	}

	private void PositionHMD(string[] splitMsg)
	{
		Vector3 position = new Vector3(float.Parse(splitMsg[1]), float.Parse(splitMsg[2]), float.Parse(splitMsg[3]));
		Vector3 rotation = new Vector3(float.Parse(splitMsg[4]), float.Parse(splitMsg[5]), float.Parse(splitMsg[6]));
		HMDPos = position;
		HMDRot = Quaternion.Euler(rotation);
		// HMD.rotation = Quaternion.Euler(Vector3.Lerp(HMD.rotation.eulerAngles, rotation, Time.deltaTime * 20));
	}
	private void PositionLeftController(string[] splitMsg)
	{
		Vector3 position = new Vector3(float.Parse(splitMsg[1]), float.Parse(splitMsg[2]), float.Parse(splitMsg[3]));
		Vector3 rotation = new Vector3(float.Parse(splitMsg[4]), float.Parse(splitMsg[5]), float.Parse(splitMsg[6]));
		LeftControllerPos = position;
		LeftControllerRot = Quaternion.Euler(rotation);
	}
	private void PositionRightController(string[] splitMsg)
	{
		Vector3 position = new Vector3(float.Parse(splitMsg[1]), float.Parse(splitMsg[2]), float.Parse(splitMsg[3]));
		Vector3 rotation = new Vector3(float.Parse(splitMsg[4]), float.Parse(splitMsg[5]), float.Parse(splitMsg[6]));
		RightControllerPos = position;
		RightControllerRot= Quaternion.Euler(rotation);
	}

	private void PositionModels()
	{
		HMD.position = Vector3.Lerp(HMD.position, HMDPos, Time.deltaTime * 10);
		LeftController.position = Vector3.Lerp(LeftController.position, LeftControllerPos, Time.deltaTime * 10);
		RightController.position = Vector3.Lerp(RightController.position, RightControllerPos, Time.deltaTime * 10);

		HMD.rotation = Quaternion.Lerp(HMD.rotation, HMDRot, Time.deltaTime * 10);
		LeftController.rotation = Quaternion.Lerp(LeftController.rotation, LeftControllerRot, Time.deltaTime * 10);
		RightController.rotation = Quaternion.Lerp(RightController.rotation, RightControllerRot, Time.deltaTime * 10);
	}
}
