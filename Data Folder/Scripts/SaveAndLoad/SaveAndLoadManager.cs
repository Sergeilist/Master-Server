using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace SaveAndLoad {

	public class SaveAndLoadManager : MonoBehaviour {

		public static SaveAndLoadManager instance;

		[HideInInspector]
		public SettingsData m_Settings;

		private Dictionary<string, string> m_LocalizedText;
		private string m_MissingTextString = "Localized text not found";
		private string m_FilePath;
		private bool m_IsReady = false;


		void Awake ()
		{
			//Задаем статичную ссылку
			if (instance == null) {
				instance = this;
			} else if (instance != this) {
				Destroy (gameObject);
			}

			//Не разрушаем объект при загрузке новых сцен
			DontDestroyOnLoad (gameObject);

			//Создаем путь к файлу
			m_FilePath = Path.Combine (Application.streamingAssetsPath, "Settings.json");
			//Загружаем настройки из файла
			StartCoroutine (LoadSettings ());
		}

		IEnumerator LoadSettings ()
		{
			//Если файл есть
			if (File.Exists (m_FilePath)) {

				//Если путь URL (Андройд)
				string dataAsJson = "";
				if (m_FilePath.Contains ("://")) {
					WWW www = new WWW (m_FilePath);
					yield return www;
					if (string.IsNullOrEmpty (www.error)) {
						dataAsJson = www.text;
					}
				} else {
					dataAsJson = File.ReadAllText (m_FilePath);
				}

				//Заполняем настройки загруженными данными
				m_Settings = JsonUtility.FromJson<SettingsData> (dataAsJson);
			} else {

				//Сериализуем настройки по умолчанию в текст
				string dataAsJson = JsonUtility.ToJson (m_Settings);
				//Создаем или перезаписываем файл
				File.WriteAllText (m_FilePath, dataAsJson);
			}

			//Создаем словарь и путь к файлу перевода
			m_LocalizedText = new Dictionary<string, string> ();
			string filePath = Path.Combine (Application.streamingAssetsPath, m_Settings.m_Language);

			if (File.Exists (filePath)) {

				//Если путь URL (Андройд)
				string dataAsJson = "";
				if (filePath.Contains ("://")) {
					WWW www = new WWW (filePath);
					yield return www;
					if (string.IsNullOrEmpty (www.error)) {
						dataAsJson = www.text;
					}
				} else {
					dataAsJson = File.ReadAllText (filePath);
				}

				//Десериализуем данные
				LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

				//Заполняем словарь
				for (int i = 0; i < loadedData.items.Length; i++) 
				{
					m_LocalizedText.Add (loadedData.items [i].key, loadedData.items [i].value);   
				}

				Debug.Log ("Data loaded, dictionary contains: " + m_LocalizedText.Count + " entries");
			} else 
			{
				Debug.LogError ("Cannot find file!");
			}

			m_IsReady = true;
		}

		public bool GetIsReady()
		{
			//Проверяем готовы ли данные с языком и настройками
			return m_IsReady;
		}

		public string GetLocalizedValue(string key)
		{
			//Получаем текст выбраного языка по ключу
			string result = m_MissingTextString;
			if (m_LocalizedText.ContainsKey (key))
			{
				result = m_LocalizedText [key];
			}

			return result;
		}

		public IEnumerator SaveSettings ()
		{
			//Сериализуем настройки в текст
			string dataAsJson = JsonUtility.ToJson (m_Settings);
			//Создаем или перезаписываем файл
			File.WriteAllText (m_FilePath, dataAsJson);

			yield return null;
		}
	}
}