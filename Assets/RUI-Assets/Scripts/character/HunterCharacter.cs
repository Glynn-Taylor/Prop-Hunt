using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Hunter character, syncs network data and handles movement/weapon controls
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class HunterCharacter : Character {
	//Character's weapon
	public Gun GunComponent;
	//Is controls locked
	public bool isLocked =false;
	//Is character currently walking
	private bool Walking=false;
	//Animation reference
	public Animation animator;
	//Sync data
	protected Vector3 syncRotation = Vector3.zero;
	protected int syncArmRotation = 0;
	
	//Intialisation
	void Start(){
		//Set initial sync data
		syncStartPosition=transform.position;
		syncEndPosition=transform.position;
		syncRotation=transform.rotation.eulerAngles;
	}
	//
	void Update ()
	{
		//If owned by player handle controls
		if(networkView.isMine){
			UpdateMe();
		}else{
			//Disable controls such as mouse look
			DisableControls();
			//Sync transform
			SyncedMovement();
		}
	}

	void UpdateMe ()
	{
		if(!isLocked){
			//Fire weapon on LMB, decreases acurracy
			if (Input.GetMouseButton (0)) {
				GunComponent.Fire();
				Screen.lockCursor = true;
			//If not firing make gun more accurate
			}else{
				GunComponent.ReduceSpread();
			}
			//If R, attempt to reload
			if (Input.GetKeyDown (KeyCode.R)) {
				GunComponent.Reload ();
			}
		}
		//Unlock cursor on escape
		if (Input.GetKeyDown (KeyCode.Escape))
			Screen.lockCursor = false;
	}
	//RPC call to take damage, handles how much damage to take when shooting non-player prop
	[RPC]
	public void TakeDamage(){
		DamageHealth(4);
	}
	//Handles drawing health to screen
	void OnGUI(){
		if(networkView.isMine)
			GUI.Box (new Rect(0,0,100,25),"Health:"+Health.ToString());
	}
	//Handles serializing rotation over the network
	protected override void SerializeNetworkData (BitStream stream, NetworkMessageInfo info){
		Vector3 networkRotation = Vector3.zero;
		//If sending
		if (stream.isWriting)
		{
			//Serialize rotation to stream
			networkRotation = NetworkTransform.rotation.eulerAngles;
			stream.Serialize(ref networkRotation);
		}
		//If recieving
		else
		{
			//Deserialize and set synced rotation
			stream.Serialize(ref networkRotation);
			syncRotation = networkRotation;
		}
	}
	//set rotation to  synced rotation
	protected override void SyncData (){
		NetworkTransform.eulerAngles=syncRotation;
	}
	//Check velocity and if is moving then update animation
	protected override void MovementAnimationCheck(Vector3 velocity){
		//If moving
		if(velocity.magnitude>0.05f){
			if(!Walking){
				//Fade into moving
				animator.CrossFade("Walk",0.01f);
				Walking=true;
			}
		}else{
			//If currently walking then fade into Idle
			if(Walking){
				animator.wrapMode=WrapMode.Loop;
				animator.CrossFade("Idle",0.01f);
				Walking=false;
			}
		}
	}
}
