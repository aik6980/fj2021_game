using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePlayer : MonoBehaviour
{
	public Animator anim;
	public OnFootMovement move;

	public float timer;
	public float timerMin = 1.0f;
	public float timerMax = 5.0f;

	public int randMax = 5;

	private FMOD.Studio.EventInstance footstepSound;

	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
		move = GetComponentInParent<OnFootMovement>();
    }

    // Update is called once per frame
    void Update()
    {
		anim.SetBool("moving", move.stepTimer > 0);

		if (timer > 0)
			timer -= Time.deltaTime;
		else
		{
			anim.SetInteger("random", Random.Range(0, randMax));
			timer = Random.Range(timerMin, timerMax);
		}
	}

	public void StepSound()
	{
		footstepSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/footsteps");
        footstepSound.start();
		FMODUnity.RuntimeManager.AttachInstanceToGameObject(footstepSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
		footstepSound.release();
	}
}
