using UnityEngine;
using UnityEngine.UI;
using MasterServer;

namespace TwelfthStar {

	public class MenuManager : MonoBehaviour {

		[Header("Информация игрового сервера")]
		public Text m_ServerIp;
		public Text m_ServerPort;
		public Text m_ServerName;
		public Text m_ServerType;
		public Text m_ServerMaxPlayers;

		[Header("Список лобби")]
		public Transform m_ContentView;


		public void ClientConnect ()
		{
			// Указываем где распологать информацию о играх
			ClientManager.instance.m_ContentView = m_ContentView;

			// Подключаемся к мастер серверу и получаем список из первых 20 игровых серверов
			ClientManager.instance.m_Client.StartConnect ();
		}

		public void GetGameList ()
		{
			// Получаем список следующих 20 игровых серверов
			ClientManager.instance.m_Client.GetServerList ();
		}

		public void ClearGameList ()
		{
			// Отчистить весь список игр
			ClientManager.instance.m_Client.ClearServerList ();
		}

		public void ClientDisconnect ()
		{
			// Отключаемся от мастер сервера
			ClientManager.instance.m_Client.StartDisconnect ();
		}

		public void StartSinglePlayer ()
		{
			// Включаем одиночную игру (как хост)
			ClientManager.instance.m_NetworkManager.networkAddress = "127.0.0.1";
			ClientManager.instance.m_NetworkManager.networkPort = 7777;

			ClientManager.instance.m_NetworkManager.StartHost ();
		}

		public void StartMultiPlayer ()
		{
			// Присоединяемся к выбранному игровому серверу
			if (ClientManager.instance.m_CurrentGame != null) {
				ClientManager.instance.m_Client.StartDisconnect ();

				ClientManager.instance.m_NetworkManager.networkAddress = ClientManager.instance.m_CurrentGame.m_Ip;
				ClientManager.instance.m_NetworkManager.networkPort = int.Parse (ClientManager.instance.m_CurrentGame.m_Port);

				ClientManager.instance.m_NetworkManager.StartClient ();
			}
		}

		public void StartGameServer ()
		{
			// Запускаем игровой сервер
			ClientManager.instance.StartGameServer (m_ServerIp.text, m_ServerPort.text, m_ServerName.text, m_ServerType.text, m_ServerMaxPlayers.text);
		}
	}
}