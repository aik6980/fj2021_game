using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControls : MonoBehaviour
{
	public GameObject pauseMenuRoot;
	public Button quitButton;
	public GameObject inGameMenuRoot;

	public bool paused;
	public bool ignoreInput;


	void Awake()
	{
	}


	// Start is called before the first frame update
	void Start()
    {
		quitButton.onClick.AddListener(this.OnQuitButton);

		pauseMenuRoot.SetActive(paused);
		inGameMenuRoot.SetActive(!paused);
	}

	// Update is called once per frame
	void Update()
    {
		if (ignoreInput) return;

        if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!paused)
			{
				paused = true;
			} else
			{
				paused = false;
			}
			pauseMenuRoot.SetActive(paused);
			inGameMenuRoot.SetActive(!paused);

			PauseGame(paused);
		}
	}

	public void PauseGame(bool paused)
	{
		Time.timeScale = paused ? 0 : 1;
/*
		CameraControl camcon = CameraControl.singleton;
		if (paused)
		{
			camcon.footMove.StopMove();
			camcon.shipMove.StopMove();
		} else
		{
			camcon.footMove.Continue();
			camcon.shipMove.Continue();
		}
*/
	}

	public void OnQuitButton()
	{
		Application.Quit();
	}

	public void IgnoreInput(bool on)
	{
		ignoreInput = on;
	}
}
