using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SaveAndLoad;

namespace TwelfthStar {

	public class StarLoadGame : MonoBehaviour {


		private IEnumerator Start () 
		{
			while (!SaveAndLoadManager.instance.GetIsReady ())
			{
				yield return null;
			}

			SceneManager.LoadScene ("Scene_Menu");
		}
	}
}