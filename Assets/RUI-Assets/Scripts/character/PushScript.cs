using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Allows a character controller to push an object when colliding with it
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class PushScript : MonoBehaviour {
	//Pushing power
	public float pushPower = 1.0F;
	
	//On colliding with an object
	void OnControllerColliderHit(ControllerColliderHit hit) {
		//get rigidbody of object
		Rigidbody body = hit.collider.attachedRigidbody;
		//If is prop character or immoveable or not a rigidbody then exit
		if (body == null || body.isKinematic||body.tag!="Prop")
			return;
		
		if (hit.moveDirection.y < -0.3F)
			return;
		//Get push vector
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		//Set colliding object's velocity
		body.velocity = pushDir * pushPower;
	}
}
