using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Base class for entities that can take damage and can syncronize transform/other data across the network
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public abstract class Character : MonoBehaviour {
	//Camera for this player
	public GameObject CharacterCam;
	//Movement controller
	public CharacterController CharacterControl;
	//Associated controls such as looking
	public MonoBehaviour[] Controls;
	//Health
	protected int Health=100;
	//If control of this object has been disabled
	private bool AlreadyDisabled=false;
	//Audio listener to disable if controls are disabled
	public AudioListener DisableListener;
	//Tranform to synchronize
	public Transform NetworkTransform;
	//Time of last sync
	protected float lastSynchronizationTime = 0f;
	protected float syncDelay = 0f;
	//Sync timer
	protected float syncTime = 0f;
	//Sync positions for interpolation
	protected Vector3 syncStartPosition = Vector3.zero;
	protected Vector3 syncEndPosition = Vector3.zero;
	
	//Initialisation
	void Start(){
		//Set initial sync positions
		syncStartPosition=transform.position;
		syncEndPosition=transform.position;
	}
	//Method for for taking damage, spawns a spectator object on death
	public void DamageHealth(int amount){
		Health-=amount;
		//If belonging to player, destroy this
		if(Health<=0&&networkView.isMine){
			Network.Destroy(gameObject);
			GameObject.FindGameObjectWithTag("GameController").GetComponent<RoundController>().SpawnPlayer();
		}
	}
	//Disables this players controls so cant move/see
	protected void DisableControls(){
		if(!AlreadyDisabled){
			//Disable camera
			CharacterCam.SetActive(false);
			//Disable movement
			if(CharacterControl)
				CharacterControl.enabled=false;
			//Disable other controls
			for(int i=0;i<Controls.Length;i++){
				Controls[i].enabled=false;
			}
			if(DisableListener!=null)
				DisableListener.enabled=false;
			AlreadyDisabled=true;
		}
	}
	//Helper function for TakeDamage network call
	public void DamageMe(){
		networkView.RPC ("TakeDamage",RPCMode.All);
	}
	//Function for syncing movement
	protected void SyncedMovement()
	{
		//Advance sync time
		syncTime += Time.deltaTime;
		//Interpolate the position
		NetworkTransform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
		//Sync other non transform data
		SyncData();
	}
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		//If sending
		if (stream.isWriting)
		{
			//Send position
			syncPosition = NetworkTransform.position;
			stream.Serialize(ref syncPosition);
			//Send velocity
			syncVelocity = NetworkTransform.rigidbody.velocity;
			stream.Serialize(ref syncVelocity);
		}
		//If recieving
		else
		{
			//Recieve position and velocity
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			//Calculate delay
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			//Interpolate based on delay		
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = NetworkTransform.position;
			//Set animation based on movement
			MovementAnimationCheck(syncEndPosition-syncStartPosition);
		}
		SerializeNetworkData(stream, info);
	}
	//Abstract method for send/recieving non transform data
	protected abstract void SerializeNetworkData (BitStream stream, NetworkMessageInfo info);
	protected abstract void SyncData ();
	//Abstract method for changing animation based on velocity
	protected abstract void MovementAnimationCheck (Vector3 velocity);
}
