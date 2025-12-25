using UnityEngine;
using System.Collections;

public class SquirrelNPC : MonoBehaviour {

		private Animator anim;

		public float speed = 600.0f;
		public float turnSpeed = 400.0f;
		private Vector3 moveDirection = Vector3.zero;
		public float gravity = 20.0f;

		void Start () {
			anim = gameObject.GetComponentInChildren<Animator>();
		}

		void Update (){
			anim.SetInteger ("AnimationPar", 1);

			moveDirection = transform.forward * 1 * speed;

			float turn = 1;
			transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
			transform.position += moveDirection * Time.deltaTime;
			moveDirection.y -= gravity * Time.deltaTime;
		}
}
