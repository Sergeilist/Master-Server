using UnityEngine;

namespace SaveAndLoad {

	//Сериализованные данные для языков игры
	[System.Serializable]
	public class LocalizationData 
	{
		public LocalizationItem[] items;
	}

	[System.Serializable]
	public class LocalizationItem
	{
		public string key;
		[Multiline]
		public string value;
	}
}