using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace MasterServer {

	public class ClientManager : MonoBehaviour {

		public static ClientManager instance;

		[Header("Информация о игре")][SerializeField]
		private GameView m_GameView;

		[HideInInspector]
		public Transform m_ContentView;
		[HideInInspector]
		public GameView m_CurrentGame;
		[HideInInspector]
		public Client m_Client;
		[HideInInspector]
		public NetworkManager m_NetworkManager;

		private List<GameView> m_Games = new List<GameView>();
		private IEnumerator m_CoroutinePing;
		private int m_NumPlayers = 0;
		private bool m_IsServer = false;


		private void Awake ()
		{
			if (instance == null) {
				instance = this;
			} else if (instance != this)
			{
				Destroy (gameObject);
			}

			m_Client = GetComponent<Client> ();
			m_NetworkManager = (NetworkManager)FindObjectOfType (typeof(NetworkManager));
		}

		private void Update ()
		{
			if (!m_IsServer)
				return;

			// Обновляем колличество игроков в мастер сервере
			if (m_NumPlayers != m_NetworkManager.numPlayers) {
				m_NumPlayers = m_NetworkManager.numPlayers;
				m_Client.m_GameServer.m_ServerNumPlayers = m_NumPlayers.ToString ();
				m_Client.UpNumPlayers ();
			}
		}

		public void ReGameList ()
		{
			if (m_CoroutinePing != null)
				StopCoroutine (m_CoroutinePing);
			m_CoroutinePing = CheckPing ();

			// Отчищаем визуальный список игр
			foreach (GameView gv in m_Games) {
				Destroy (gv.gameObject);
			}
			m_Games.Clear ();

			// Показываем обновленный список игр
			foreach (ServerInfo si in m_Client.m_Servers) {
				GameView gv = Instantiate (m_GameView) as GameView;
				gv.transform.SetParent (m_ContentView);
				m_Games.Add (gv);

				gv.m_GameName.text = si.m_ServerName;
				gv.m_GameType.text = si.m_ServerType;
				gv.m_GamePlayers.text = si.m_ServerNumPlayers + "/" + si.m_ServerMaxPlayers;

				gv.m_Ip = si.m_ServerIp;
				gv.m_Port = si.m_ServerPort;
				gv.m_NumPlayers = int.Parse (si.m_ServerNumPlayers);
				gv.m_MaxPlayers = int.Parse (si.m_ServerMaxPlayers);
			}

			StartCoroutine (m_CoroutinePing);
		}

		public void StartGameServer (string ip, string port, string name, string type, string maxplayers)
		{
			// Заполняем данные игрового сервера для мастер сервера
			m_Client.m_IsGameServer = true;
			m_Client.m_GameServer.m_ServerIp = ip;
			m_Client.m_GameServer.m_ServerPort = port;
			m_Client.m_GameServer.m_ServerName = name;
			m_Client.m_GameServer.m_ServerType = type;
			m_Client.m_GameServer.m_ServerMaxPlayers = maxplayers;
			m_Client.m_GameServer.m_ServerNumPlayers = "0";

			DontDestroyOnLoad (gameObject);

			// Подключаемся к мастер серверу
			m_Client.StartConnect ();

			// Включаем игровой сервер
			m_NetworkManager.maxConnections = int.Parse (maxplayers);
			m_NetworkManager.networkAddress = ip;
			m_NetworkManager.networkPort = int.Parse (port);
			m_NetworkManager.StartServer();

			m_IsServer = true;
		}

		IEnumerator CheckPing ()
		{
			foreach (GameView gv in m_Games) {
				Ping gvping = new Ping (gv.m_Ip);
				yield return gvping.isDone;
				gv.m_GamePing.text = gvping.time + " ms";
			}
		}
	}
}