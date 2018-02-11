using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MasterServer {
	
	public class GameView : MonoBehaviour, IPointerClickHandler {

		[Header("Информация о игре")]
		public Text m_GameName;
		public Text m_GameType;
		public Text m_GamePlayers;
		public Text m_GamePing;

		public Image m_Panel;

		[HideInInspector]
		public string m_Ip;
		[HideInInspector]
		public string m_Port;
		[HideInInspector]
		public int m_NumPlayers;
		[HideInInspector]
		public int m_MaxPlayers;


		public void OnPointerClick (PointerEventData eventData)
		{
			if (ClientManager.instance.m_CurrentGame != null) {
				ClientManager.instance.m_CurrentGame.m_Panel.color = new Color (1f, 1f, 1f, 0.4f);
			}

			ClientManager.instance.m_CurrentGame = this;
			m_Panel.color = new Color (0f, 1f, 0f, 0.4f);
		}
	}
}