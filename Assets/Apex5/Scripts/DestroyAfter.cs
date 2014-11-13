using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Helper script for destroying a game object after an elapsed time
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class DestroyAfter : MonoBehaviour {
	//Time to destroy after
	public float time;
	
	//Initialization, intiates destruction
	void Start () {
		Invoke ("DestroyThis",time);
	}
	//Function to destroy gameobject
	void DestroyThis(){
		Destroy (gameObject);	
	}
}
