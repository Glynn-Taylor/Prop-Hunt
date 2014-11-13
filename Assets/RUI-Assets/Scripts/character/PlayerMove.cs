using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Movement for spectator entity
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class PlayerMove : MonoBehaviour
{
	//Character controller to use for movement
	private CharacterController C;
	//Speed to move at
	public float Speed;
	
	//Initialization
	void Start ()
	{
		C = GetComponent<CharacterController> ();
	}
	
	// Update is called once per frame, handles movement, spectator only exists locally so no need to do network check
	void Update ()
	{
		//On WASD, move in direction relative to transform vectors
		if (Input.GetKey (KeyCode.W)) {
			C.Move (transform.forward * Speed);
		}
		if (Input.GetKey (KeyCode.A)) {
			C.Move (-transform.right * Speed);
		}
		if (Input.GetKey (KeyCode.S)) {
			C.Move (-transform.forward * Speed);
		}
		if (Input.GetKey (KeyCode.D)) {
			C.Move (transform.right * Speed);
		}
		//Unlock mouse cursor on escape
		if (Input.GetKeyDown (KeyCode.Escape))
			Screen.lockCursor = false;
	}
}
