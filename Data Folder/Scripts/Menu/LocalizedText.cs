using UnityEngine;
using UnityEngine.UI;
using SaveAndLoad;

namespace TwelfthStar {

	public class LocalizedText : MonoBehaviour {

		public string key;


		void Start ()
		{
			Text text = GetComponent<Text> ();
			text.text = SaveAndLoadManager.instance.GetLocalizedValue (key);
		}
	}
}