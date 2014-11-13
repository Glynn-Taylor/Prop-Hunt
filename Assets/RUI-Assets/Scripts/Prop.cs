using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Synchronizes the transform of a physics object across the network (in implementation currently done by network view)
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class Prop : MonoBehaviour {
	//Time since last sync for lerping
	protected float lastSynchronizationTime = 0f;
	//Synchronization delay
	protected float syncDelay = 0f;
	protected float syncTime = 0f;
	//Position currently
	protected Vector3 syncStartPosition = Vector3.zero;
	//Position at just recieved sync + interpolation
	protected Vector3 syncEndPosition = Vector3.zero;
	
	//Initialisation
	void Start () {
		syncStartPosition=transform.position;
		syncEndPosition=transform.position;
	}
	//Function for syncing movement
	protected void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		//Lerp to calculated position
		transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	//On sending/receiving network data
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		Vector3 syncRotation = Vector3.zero;
		//If sending
		if (stream.isWriting)
		{
			//Send current position
			syncPosition = transform.position;
			stream.Serialize(ref syncPosition);
			//Send current velocity
			syncVelocity = transform.rigidbody.velocity;
			stream.Serialize(ref syncVelocity);
			//Send current rotation
			syncRotation= transform.rigidbody.rotation.eulerAngles;
			stream.Serialize(ref syncRotation);
		}
		//If reading
		else
		{
			//Get velocity, rotation, position
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			stream.Serialize(ref syncRotation);
			//Set rotation
			transform.eulerAngles=syncRotation;
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			//Calculate an interpolated position to move to over time
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			//Move from here
			syncStartPosition = transform.position;
		}
	}
	
}
