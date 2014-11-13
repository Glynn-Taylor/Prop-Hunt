using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Handles bullet collision and network position/rotation synchronization
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class ProjectileScript : MonoBehaviour
{
	//Movement vector
	protected Vector3 MoveVelocity;
	//Initial position for range check
	protected Vector3 InitialPosition;
	//Recent position for better physics check
	protected Vector3 PreviousPosition;
	//Is bullet needing to be clean up (still playing audio)
	protected bool Dead;
	//Range that bullet can travel before self destruction
	protected float Range = 50f;
	//Damage of the bullet
	public float Damage =1;
	//Who fired the bullet
	public HunterCharacter Owner;
	//Masks for excluding objects from collisions
	public LayerMask excludeMask1;
	public LayerMask excludeMask2;
	//Transform to send across the network
	public Transform NetworkTransform;
	//Synchronisation timers
	protected float lastSynchronizationTime = 0f;
	protected float syncDelay = 0f;
	protected float syncTime = 0f;
	//Synchronisation positions for interpolation
	protected Vector3 syncStartPosition = Vector3.zero;
	protected Vector3 syncEndPosition = Vector3.zero;
	
	//Initialisation
	void Start(){
		PreviousPosition=transform.position;
		syncStartPosition=transform.position;
		syncEndPosition=transform.position;
		//Form composite bit mask
		excludeMask1=~(excludeMask1|excludeMask2);
	}
	//Set initial velocity
	public void SetVelocities (float x, float y, float z)
	{
		MoveVelocity = new Vector3 (x, y, z);
		InitialPosition = transform.position;
	}
	//Called every frame, handles movement
	void Update ()
	{
		//If owner then run logic
		if(networkView.isMine){
			UpdateMe();
		//Otherwise sync positional data
		}else{
			SyncedMovement();
		}
		
	}

	void UpdateMe ()
	{
		//If alive and moving
		if (MoveVelocity != null&&!Dead) {
			//Move
			transform.position += MoveVelocity;
			//Range check, destroy if travelled too far
			if (Vector3.Distance (InitialPosition, transform.position) > Range) {
				Network.Destroy (gameObject);
			//Advanced collision check	
			}else{
				//Get vector moved since last check
				Vector3 movementThisStep = transform.position - PreviousPosition;
				RaycastHit hit;
				//Check for collisions between there and here
				if (Physics.Raycast(PreviousPosition, movementThisStep, out hit, Vector3.Magnitude (movementThisStep), excludeMask1.value)){
					Debug.Log ("hit "+hit.collider.gameObject.name);
					//If hit prop player then do damage
					if (hit.collider.tag == "PropCollider") {
						hit.transform.parent.gameObject.GetComponent<PropCharacter>().DamageMe();
						//Start bullet death
						Destroy (collider);
						if(!Dead){
							//Create a blood splatter
							GameObject go = (GameObject)GameObject.Instantiate(Resources.Load ("BloodHit"),hit.point,transform.rotation);
							//Attach blood splatter to prop character so doesnt hang in the air when moving
							go.transform.parent=hit.collider.transform;
						}
						Dead = true;
					} else if (hit.collider.tag == "Prop") {
						//If hit a normal prop then push that prop slightly under force of bullet
						Rigidbody body = hit.collider.attachedRigidbody;
						Vector3 pushDir = MoveVelocity.normalized;
						body.velocity = pushDir * 0.75f;
						//Damage whoever fired this bullet
						Owner.DamageMe();
						//Start bullet death
						Destroy (collider);
						if(!Dead)
							//Create dirt poof
							GameObject.Instantiate(Resources.Load ("DirtHit"),hit.point,transform.rotation);
						Dead = true;
					//If hit geometry
					}else{
						//Kill bullet
						if(!Dead)
							Destroy (collider);
						//Spawn dirt poof
						GameObject.Instantiate(Resources.Load ("DirtHit"),hit.point,transform.rotation);
						Dead = true;
					}
				}
				//Reset for next time around
				PreviousPosition=transform.position;
			}
		}
		//Once audio is finished and death then terminate
		if (Dead && !audio.isPlaying)
			Network.Destroy (gameObject);
	}
	//Sets the range of the bullet
	public void setRange (float r)
	{
		Range = r;	
	}
	//Ignores collisions between this bullet and a collider
	public void setIgnore (Collider col)
	{
		Physics.IgnoreCollision (collider, col);
	}
	//Returns the movement vector of this bullet
	public Vector3 getVelocity ()
	{
		return MoveVelocity;	
	}
	//Sets who owns this bullet
	public void setOwner (HunterCharacter owner)
	{
		this.Owner=owner;
	}
	//Function for syncing movement
	protected void SyncedMovement()
	{
		//Advance sync time
		syncTime += Time.deltaTime;
		//Interpolate the position
		NetworkTransform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	//On sending/recieving network data
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
		}
	}
}
