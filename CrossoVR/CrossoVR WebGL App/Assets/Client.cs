﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Client : MonoBehaviour {
	private const int MAX_CONNECTIONS = 8;

	private int port = 5701;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unreliableChannel;

	private int clientId;
	private int connectionId;

	private float connectionTime;
	private bool isConnected = false;
	private bool isStarted = false;
	private byte error;

	private string playerName;

	public void Connect()
	{
		//Does player have a name?

		string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
		if(pName == "")
		{
			Debug.Log("You must enter a name");
			return;
		}
		
		playerName = pName;

		//Connect to host

		NetworkTransport.Init();
		ConnectionConfig cc = new ConnectionConfig();

		reliableChannel = cc.AddChannel(QosType.Reliable);
		unreliableChannel = cc.AddChannel(QosType.Unreliable);

		HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

		hostId = NetworkTransport.AddHost(topo, 0);
		connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", port, 0, out error);

		connectionTime = Time.time;
		isConnected = true;
		Debug.Log("Connected!");
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
				Debug.Log("Recieving " + msg);

				string[] splitMsg = msg.Split('|');

				switch(splitMsg[0])
				{
					case "ASKNAME":
						OnAskName(splitMsg);
						break;

					case "CNN":
						break;

					case "DC":
						break;
				}
				break;
		}		
	}

	private void OnAskName(string[] data)
	{
		//Set this client's ID
		clientId = int.Parse(data[1]);

		//Send our name to the server
		Send("NAMEIS|" + playerName, reliableChannel);
		//Create all the other players
		for(int i = 2; i < data.Length - 1; i++)
		{
			string[] d = data[i].Split('%');
			SpawnPlayer(d[0], int.Parse(d[1]));
		}
	}

	private void SpawnPlayer(string playerName, int playerId)
	{

	}

	private void Send(string message, int channelId)
	{
		Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);
		NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);
	}
}
