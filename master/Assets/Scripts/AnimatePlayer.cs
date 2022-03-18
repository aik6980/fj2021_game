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

	GameObject player;

	public static bool leftStep;

	void Start()
    {
		anim = GetComponent<Animator>();
		move = GetComponentInParent<OnFootMovement>();

    }

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


	public void LeftStepSound()
	{
		leftStep = true;
		GetComponent<Audio_FootstepSounds>().PlayFootstepSound();
	}
	public void RightStepSound()
	{
		leftStep = false;
		GetComponent<Audio_FootstepSounds>().PlayFootstepSound();
	}



}
