using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonHandler : MonoBehaviour
{
	public string SceneName;

	[UsedImplicitly]
	public void Pelaa()
	{
		SceneManager.LoadScene(SceneName);
	}
}