using System.Collections.Generic;

namespace SaveAndLoad {

	//Сериализованые данные настроек игры
	[System.Serializable]
	public class SettingsData {

		public string m_Language = "English.json";
		public List<string> m_SaveList;
	}
}