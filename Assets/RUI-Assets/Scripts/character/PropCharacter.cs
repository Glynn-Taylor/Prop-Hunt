using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Controls prop character, which can change into other props upon pressing E
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class PropCharacter : Character
{
	//Damage to take from bullets
	private const int BULLET_DMG = 5;
	//Current duplicate of prop
	private GameObject Duplicate;
	//Renderer of the starting controller (a ball)
	public MeshRenderer BallRenderer;
	//Collider for bullet collisions (non movement)
	public BoxCollider MyCollider;
	//Collider for movement collisions
	private SphereCollider ballCollider;
	//Largest size of movement collider (for very large props to fit through doors)
	private const float MaxRadius = 1f;
	
	//Initialisation
	void Start(){
		//Initial references
		syncStartPosition=transform.position;
		syncEndPosition=transform.position;
		ballCollider=BallRenderer.gameObject.GetComponent<SphereCollider>();
	}
 	//Handles keyboard input
	void UpdateMe ()
	{
		//Attempt to become a nearby prop
		if(Input.GetKeyDown(KeyCode.E)){
			Collider[] cols;
			//Check for surrounding objects
			cols=Physics.OverlapSphere(MyCollider.gameObject.transform.position,ballCollider.radius+0.2f);
			if(cols.Length>0){
				for(int i=0;i<cols.Length;i++){
					//If surrounding object is a prop
					if (cols[i].tag == "Prop") {
						//Attempt to create a duplicate and render/collide as it
						setDuplicate(cols[i].gameObject.networkView.viewID);
					}
				}
			}
		}
		//Unlock mouse cursor on escape
		if (Input.GetKeyDown (KeyCode.Escape))
			Screen.lockCursor = false;
		//Rotate duplicated model on R
		if(Input.GetKeyDown(KeyCode.R)){
			if(Duplicate!=null){
				//Rotate prop by 15 degrees
				Duplicate.transform.eulerAngles+=Vector3.up*15;
				//Apply changes over network
				networkView.RPC ("setPropRotation",RPCMode.Others,Duplicate.transform.eulerAngles.y);
			}
		}
	}
	//RPC call for applying rotational change (yaw)
	[RPC]
	public void setPropRotation(float yaw){
		Duplicate.transform.eulerAngles=new Vector3(Duplicate.transform.eulerAngles.x,yaw,Duplicate.transform.eulerAngles.z);
	}
	
	// Update is called once per frame, handles movement or tranform sync dependent on ownership
	void Update ()
	{
		//Check controls if owner
		if(networkView.isMine){
			UpdateMe();
		//Otherwise sync transform
		}else{
			DisableControls();
			SyncedMovement();
		}
	}
	//Attempts to duplicate a prop and use as model/collider 
	[RPC]
	void setDuplicate(NetworkViewID id){
		//If are owner tell others to also do this
		if(networkView.isMine){
			networkView.RPC("setDuplicate",RPCMode.OthersBuffered,id);
		}
		//Get game object to duplicate
		GameObject gameobj = NetworkView.Find(id).gameObject;
		//Duplicate it
		GameObject duplicate = (GameObject)GameObject.Instantiate (gameobj);
		//Get collider
		BoxCollider bc  =((BoxCollider)duplicate.collider);
		//Set our bullet collider to same size, offset as original
		MyCollider.size=bc.size;
		MyCollider.center=bc.center;
		
		//Get ground level and largest length of bullet collider
		float GroundLevel = ballCollider.transform.position.y-ballCollider.radius;
		float maxR = (MyCollider.size.x>MyCollider.size.z?MyCollider.size.x:MyCollider.size.z)/2;
		//Ensure isnt too big to set movement collider to
		if(maxR>MaxRadius)
			maxR=MaxRadius;
		//Set movement collider
		ballCollider.radius=maxR;
		//Move self up/down so that not in floor/above ground
		ballCollider.transform.position = new Vector3(ballCollider.transform.position.x,GroundLevel+ballCollider.radius,ballCollider.transform.position.z);
		//Offset center dependent on movement collider
		MyCollider.center-=new Vector3(0,ballCollider.radius,0);
		//Tether duplicate to bullet collider
		duplicate.transform.parent = MyCollider.gameObject.transform;
		//Sync position and rotation offset by ball collider
		duplicate.transform.localPosition=Vector3.zero-new Vector3(0,ballCollider.radius,0);
		duplicate.transform.eulerAngles=new Vector3(0,duplicate.transform.eulerAngles.y,0);
		//Remove duplicate collider
		duplicate.collider.enabled = false;
		duplicate.tag="PlayerProp";
		//Stop renderering movement ball as now have prop graphic
		BallRenderer.enabled=false;
		//Prevent duplicate acting like a prop
		duplicate.GetComponent<Prop>().enabled=false;
		Destroy(duplicate.GetComponent<NetworkView>());
		//Remove physics
		if (duplicate.rigidbody)
			Destroy (duplicate.GetComponent<Rigidbody> ());
		//Destroy old duplicate instance
		if (Duplicate != null)
			DestroyImmediate (Duplicate);
		//Set duplicate to the new duplicate
		Duplicate = duplicate;
	}
	//Function for getting distance to the ground for offsets
	float getDistanceFromGround (float y, float yOffset)
	{
		return MyCollider.transform.position.y + (y / 2f-MyCollider.size.y/2f)-yOffset;
	}
	//Draw GUI
	void OnGUI(){
		//If owned by player then draw health
		if(networkView.isMine)
			GUI.Box (new Rect(0,0,100,25),"Health:"+Health.ToString());
	}
	//Call to take damage from bullet
	[RPC]
	public void TakeDamage(){
		DamageHealth(BULLET_DMG);
	}
	//Unused overrides
	protected override void SerializeNetworkData (BitStream stream, NetworkMessageInfo info){}
	protected override void SyncData (){}
	protected override void MovementAnimationCheck(Vector3 velocity){}
}
