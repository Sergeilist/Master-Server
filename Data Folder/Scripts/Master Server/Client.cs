using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;

namespace MasterServer {

	public class ServerInfo
	{
		public string m_ServerIp;
		public string m_ServerPort;
		public string m_ServerName;
		public string m_ServerType;
		public string m_ServerNumPlayers;
		public string m_ServerMaxPlayers;
	}

	public class Client : MonoBehaviour {

		// Параметры сети мастер сервера
		private const int m_MaxConnection = 100;
		private string m_IpAddress = "127.0.0.1";
		private int m_Port = 5701;

		private int m_ReliableChannel;
		//private int m_UnreliableChanel;
		private int m_HostId;
		private int m_ConnectionId;

		private bool m_IsInit = false;
		private bool m_IsConnect = false;

		// Параметры для клиента - сервера
		[HideInInspector]
		public bool m_IsGameServer = false;
		public ServerInfo m_GameServer = new ServerInfo();

		// Список получаемых игровых серверов
		public List<ServerInfo> m_Servers = new List<ServerInfo>();

		//private int m_OurClientId;
		private int m_Count = 0;


		public void StartConnect ()
		{
			if (!m_IsInit) {
				
				// Инициализируем сеть
				NetworkTransport.Init ();
				ConnectionConfig conConfig = new ConnectionConfig ();

				m_ReliableChannel = conConfig.AddChannel (QosType.Reliable);
				//m_UnreliableChanel = conConfig.AddChannel (QosType.Unreliable);

				HostTopology hostTopo = new HostTopology (conConfig, m_MaxConnection);

				// Подключаемся к мастер серверу
				m_HostId = NetworkTransport.AddHost (hostTopo, 0);
				byte error;
				m_ConnectionId = NetworkTransport.Connect (m_HostId, m_IpAddress, m_Port, 0, out error);

				m_IsInit = true;
			} else {
				if (!m_IsConnect) {
					byte error;
					m_ConnectionId = NetworkTransport.Connect (m_HostId, m_IpAddress, m_Port, 0, out error);
				}
			}
		}

		public void StartDisconnect ()
		{
			if (m_IsConnect) {
				byte error;
				NetworkTransport.Disconnect (m_HostId, m_ConnectionId, out error);
				m_IsConnect = false;
			}
		}

		private void Update ()
		{
			if (!m_IsInit)
				return;

			// Параметры сообщений
			int connectionId; 
			int channelId; 
			byte[] recBuffer = new byte[1024];
			int bufferSize = 1024;
			int dataSize;
			byte error;

			// Прослушиваем входящие сообщения адресованные только созданному хосту
			NetworkEventType recData = NetworkTransport.ReceiveFromHost (m_HostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

			// Проверяем сообщение
			switch (recData) {

			// Произошло подключение
			case NetworkEventType.ConnectEvent:
				m_IsConnect = true;
				break;

			// Пришла информация
			case NetworkEventType.DataEvent:
				
				// Раскодирируем информацию и делим на части по разделителю
				string msg = Encoding.Unicode.GetString (recBuffer, 0, dataSize);
				string[] splitData = msg.Split ('|');

				// Проверяем информацию
				switch (splitData [0]) {
				case "IDENT":
					OnIdent (splitData);
					break;
				case "SERVERLIST":
					OnServerList (splitData);
					break;
				default:
					Debug.Log ("invalid message: " + msg);
					break;
				}
				break;

			// Произошло отключение
			case NetworkEventType.DisconnectEvent:
				m_IsConnect = false;
				break;
			}
		}

		private void OnIdent (string[] data)
		{
			//Получаем свой ID
			//m_OurClientId = int.Parse (data [1]);

			if (m_IsGameServer) {

				// Отправляем данные сервера
				string message = "SERVER|" + m_GameServer.m_ServerIp + "|" + m_GameServer.m_ServerPort + "|" + m_GameServer.m_ServerName + "|" + m_GameServer.m_ServerType + "|" + m_GameServer.m_ServerNumPlayers + "|" + m_GameServer.m_ServerMaxPlayers;
				byte[] msg = Encoding.Unicode.GetBytes (message);
				byte error;
				NetworkTransport.Send (m_HostId, m_ConnectionId, m_ReliableChannel, msg, message.Length * sizeof(char), out error);
			} else {

				// Запрашиваем данные серверов
				GetServerList ();
			}
		}

		public void GetServerList ()
		{
			if (m_IsConnect) {
				
				// Запрашиваем данные серверов, но не все сразу (только следующие 20)
				m_Count += 20;
				string message = "CLIENT|" + m_Count;
				byte[] msg = Encoding.Unicode.GetBytes (message);
				byte error;
				NetworkTransport.Send (m_HostId, m_ConnectionId, m_ReliableChannel, msg, message.Length * sizeof(char), out error);
			}
		}

		private void OnServerList (string[] data)
		{
			// Заполняем список информацией о серверах
			for (int i = 1; i < data.Length; i++) {
				
				string[] splitData = data [i].Split ('%');

				ServerInfo sInfo = new ServerInfo ();
				sInfo.m_ServerIp = splitData [0];
				sInfo.m_ServerPort = splitData [1];
				sInfo.m_ServerName = splitData [2];
				sInfo.m_ServerType = splitData [3];
				sInfo.m_ServerNumPlayers = splitData [4];
				sInfo.m_ServerMaxPlayers = splitData [5];

				m_Servers.Add (sInfo);
			}

			//Обновляем визуальный список игр
			ClientManager.instance.ReGameList();
		}

		public void ClearServerList ()
		{
			// Отчищаем список серверов
			m_Servers.Clear ();
			m_Count = 0;

			//Обновляем визуальный список игр
			ClientManager.instance.ReGameList();
		}

		public void UpNumPlayers ()
		{
			// Сервер обновляет информацию о колличестве подключенных к нему игроков
			if (m_IsConnect && m_IsGameServer) {
				string message = "UPNUM|" + m_GameServer.m_ServerNumPlayers;
				byte[] msg = Encoding.Unicode.GetBytes (message);
				byte error;
				NetworkTransport.Send (m_HostId, m_ConnectionId, m_ReliableChannel, msg, message.Length * sizeof(char), out error);
			}
		}
	}
}