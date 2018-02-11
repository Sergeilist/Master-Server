using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;

namespace MasterServer {

	public class ClientData
	{
		public int m_ConnId;
	}

	public class ServerData
	{
		public int m_ConnId;
		public string m_ServerIp;
		public string m_ServerPort;
		public string m_ServerName;
		public string m_ServerType;
		public string m_ServerNumPlayers;
		public string m_ServerMaxPlayers;
	}

	public class MasterServer : MonoBehaviour {

		// Параметры сети мастер сервера
		private const int m_MaxConnection = 100;
		private int m_Port = 5701;

		private int m_ReliableChannel;
		//private int m_UnreliableChanel;
		private int m_HostId;

		private bool m_IsStarted = false;

		private List<ClientData> m_Clients = new List<ClientData>();
		private List<ServerData> m_Servers = new List<ServerData>();


		private void Start ()
		{
			// Инициализируем сеть
			NetworkTransport.Init ();
			ConnectionConfig conConfig = new ConnectionConfig ();

			m_ReliableChannel = conConfig.AddChannel (QosType.Reliable);
			//m_UnreliableChanel = conConfig.AddChannel (QosType.Unreliable);

			HostTopology hostTopo = new HostTopology (conConfig, m_MaxConnection);

			// Создаем мастер сервер
			m_HostId = NetworkTransport.AddHost (hostTopo, m_Port, null);

			m_IsStarted = true;
		}

		private void Update ()
		{
			if (!m_IsStarted)
				return;

			// Параметры сообщений
			int recHostId; 
			int connectionId; 
			int channelId; 
			byte[] recBuffer = new byte[1024]; 
			int bufferSize = 1024;
			int dataSize;
			byte error;

			// Прослушиваем входящие сообщения
			NetworkEventType recData = NetworkTransport.Receive (out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

			// Проверяем сообщение
			switch (recData) {

			// Произошло подключение
			case NetworkEventType.ConnectEvent:
				OnConnect (connectionId);
				break;

			// Пришла информация
			case NetworkEventType.DataEvent:

				// Раскодирируем информацию и делим на части по разделителю
				string msg = Encoding.Unicode.GetString (recBuffer, 0, dataSize);
				string[] splitData = msg.Split ('|');

				// Проверяем информацию
				switch (splitData [0]) {
				case "SERVER":
					OnServer (connectionId, splitData);
					break;
				case "CLIENT":
					OnClient (connectionId, splitData);
					break;
				case "UPNUM":
					OnUpNum (connectionId, splitData);
					break;
				default:
					Debug.Log ("invalid message: " + msg);
					break;
				}
				break;

			// Произошло отключение
			case NetworkEventType.DisconnectEvent:
				OnDisconnect (connectionId);
				break;
			}
		}

		private void OnConnect (int connId)
		{
			// Заполняем данные о клиенте
			ClientData cData = new ClientData ();
			cData.m_ConnId = connId;
			m_Clients.Add (cData);

			// Формируем сообщение
			string message = "IDENT|" + connId;
			byte[] msg = Encoding.Unicode.GetBytes (message);

			// Отправляем (спрашиваем сервер он или клиент)
			byte error;
			NetworkTransport.Send (m_HostId, connId, m_ReliableChannel, msg, message.Length * sizeof(char), out error);
		}

		private void OnServer (int connId, string[] data)
		{
			// Заполняем данные о сервере
			ServerData sData = new ServerData ();
			sData.m_ConnId = connId;
			sData.m_ServerIp = data [1];
			sData.m_ServerPort = data [2];
			sData.m_ServerName = data [3];
			sData.m_ServerType = data [4];
			sData.m_ServerNumPlayers = data [5];
			sData.m_ServerMaxPlayers = data [6];
			m_Servers.Add (sData);

			foreach (ClientData cd in m_Clients) {
				if (cd.m_ConnId == connId) {
					m_Clients.Remove (cd);
					return;
				}
			}
		}

		private void OnClient (int connId, string[] data)
		{
			int count = int.Parse (data [1]);
			if (count >= 20 && count - 20 < m_Servers.Count) {

				// Формируем информацию о серверах в пределах запрошенного колличества
				string message = "SERVERLIST|";
				for (int i = count - 20; i < m_Servers.Count; i++) {
					if (i >= count)
						break;
					message += m_Servers [i].m_ServerIp + "%" + m_Servers [i].m_ServerPort + "%" + m_Servers [i].m_ServerName + "%" + m_Servers [i].m_ServerType + "%" + m_Servers [i].m_ServerNumPlayers + "%" + m_Servers[i].m_ServerMaxPlayers + "|";
				}

				// Удаляем разделитель только по краям
				message = message.Trim ('|');
				byte[] msg = Encoding.Unicode.GetBytes (message);

				// Отправляем клиенту
				byte error;
				NetworkTransport.Send (m_HostId, connId, m_ReliableChannel, msg, message.Length * sizeof(char), out error);
			}
		}

		private void OnDisconnect (int connId)
		{
			// Отчищаем данные клиента или сервера
			foreach (ClientData cd in m_Clients) {
				if (cd.m_ConnId == connId) {
					m_Clients.Remove (cd);
					return;
				}
			}
			foreach (ServerData sd in m_Servers) {
				if (sd.m_ConnId == connId) {
					m_Servers.Remove (sd);
					return;
				}
			}
		}

		private void OnUpNum (int connId, string[] data)
		{
			// Обновляем колличество игроков на сервере
			foreach (ServerData sd in m_Servers) {
				if (sd.m_ConnId == connId) {
					sd.m_ServerNumPlayers = data [1];
					return;
				}
			}
		}
	}
}