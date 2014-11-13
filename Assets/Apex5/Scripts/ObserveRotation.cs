using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Syncs rotation of two objects with reflection
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class ObserveRotation : MonoBehaviour {
	//Object to observe
	public Transform Observe;
	//Axis to sync
	public bool x,y,z;
	
	// Update is called once per frame
	void FixedUpdate () {
		//Sync rotation
		transform.localEulerAngles = new Vector3(x? Mathf.LerpAngle(transform.localEulerAngles.x, Observe.localEulerAngles.x, Time.time*0.07f):transform.localEulerAngles.x,y?Observe.localEulerAngles.y:transform.localEulerAngles.y,z?Observe.localEulerAngles.z:transform.localEulerAngles.z);
	
	}
}
