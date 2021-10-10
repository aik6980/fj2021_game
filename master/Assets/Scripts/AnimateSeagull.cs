using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateSeagull : MonoBehaviour
{
	public Animator anim;
	public SeagullMovement move;

    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponent<Animator>();
		move = GetComponentInParent<SeagullMovement>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!move) return;

		anim.SetBool("InAir", !move.footMove.onGround);
		Vector3 vel2D = move.worldVel - move.transform.up * Vector3.Dot(move.worldVel, move.transform.up);
		anim.SetFloat("Speed", vel2D.magnitude);
		anim.SetBool("Effort", move.footMove.jumping);	//space
    }
}
