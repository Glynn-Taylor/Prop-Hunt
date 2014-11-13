using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Locks pitch/yaw/roll of an object
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class LockRotation : MonoBehaviour {
	//Which axis to lock
	public bool lockX,lockY,lockZ;
	//Saved starting vector
	private Vector3 InitialRotation;
	
	//Initialization
	void Start () {
		InitialRotation=transform.eulerAngles;
	}
	// Update is called once per frame
	void FixedUpdate () {
		//Set rotation based on which axis are locked
		Vector3 newRot =new Vector3(lockX?InitialRotation.x:transform.rotation.eulerAngles.x,lockY?InitialRotation.y:transform.rotation.eulerAngles.y,lockZ?InitialRotation.z:transform.rotation.eulerAngles.z);
		transform.eulerAngles = newRot;
	}
}