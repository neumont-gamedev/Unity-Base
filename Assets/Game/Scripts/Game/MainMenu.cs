using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] GameObject titleUI;
	[SerializeField] GameObject optionsUI;

	[SerializeField] Slider masterVolumeSlider;
	[SerializeField] FloatData masterVolume;
	[SerializeField] Event onAudioChange;

	[SerializeField] StringEvent onSceneLoadEvent;
	[SerializeField] StringData startSceneNameData;

	void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		 masterVolumeSlider.value = masterVolume.Value;
	}

	public void OnStartButton()
	{
		onSceneLoadEvent.RaiseEvent(startSceneNameData);
	}

	public void OnQuitButton()
	{
		Application.Quit();
	}

	public void OnOptionsButton()
	{
		titleUI.SetActive(false);
		optionsUI.SetActive(true);
	}

	public void OnReturnToTitleButton()
	{
		titleUI.SetActive(true);
		optionsUI.SetActive(false);
	}

	public void OnMasterVolumeChange(float volume)
	{
		masterVolume.Value = volume;
		onAudioChange.RaiseEvent();
	}
}
