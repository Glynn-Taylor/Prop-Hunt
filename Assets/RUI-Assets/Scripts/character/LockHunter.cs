using UnityEngine;
using System.Collections;
using System;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Locks the hunters screen/controls whilst props are running away
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class LockHunter : MonoBehaviour {
	//Texture to cover the screen with
	public Texture2D HunterWaitScreen;
	//Position to lock to
	public Transform LockTransform;
	//Amount of time remaining to lock for
	private float LockedTime = 30f;
	//Initial position
	private Vector3 Origin;
	//Character script
	HunterCharacter Controller;
	
	//Initialization
	void Start () {
		//Set references
		Controller=gameObject.GetComponent<HunterCharacter>();
		Origin=transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		//If owned by player
		if(networkView.isMine){
			//Decrease lock time
			LockedTime-=Time.deltaTime;
			//Lock position
			LockTransform.position=Origin;
			Controller.isLocked=true;
			//Unlock if countdown < 0
			if(LockedTime<0){
				Controller.isLocked=false;
				Destroy (this);
			}
			
		}
	}
	//Draw GUI
	void OnGUI(){
		//Draw overlay texture
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),HunterWaitScreen);
		//Display formatted countdown time
		GUI.Box(new Rect(Screen.width/2-25,Screen.height/2-10,50,20),String.Format("{0:0,0.000}", LockedTime));
	}
}
