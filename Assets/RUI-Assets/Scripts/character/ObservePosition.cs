using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Forces an object to stay at a certain position
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class ObservePosition : MonoBehaviour {
	//Position to observe
	public Transform observed;
	
	//Called every frame, sets position to that of observed
	void FixedUpdate() {
		transform.position=observed.position;
	}
}
