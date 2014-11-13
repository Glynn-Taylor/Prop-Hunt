using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Pistol type weapon, can have modified firerate to act like a submachine gun, as seen in game
 *			Crosshair based on: http://answers.unity3d.com/questions/203653/smart-crosshair.html
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class Pistol : ClipBased {
	//Point that currently looking at, gun looks/fires towards it
	private Vector3 Target;
	//The crosshair color
	public Color crosshairColor = Color.white;
	//Style for drawing the cross hair with
	private GUIStyle CrossHairBoxSkin;
	//Texture for cross hair parts (white)
	private Texture2D tex;
	//Crosshair width
	const float width= 2;
	//Crosshair height
	const float  height = 10;
	//Whether or not are displaying the GUI (ammo, crosshair)
	bool DisplayGUI=false;
	//Multiplier for spreading out crosshair
	private const float GUIMultiplier=10f;
	
	//Intialisation
	void Start () {
		//Generate texture
		tex = new Texture2D(1,1);
		//Assign color to texture
		SetColor(tex, crosshairColor);
		//Make skin based on that texture
		CrossHairBoxSkin = new GUIStyle();
		CrossHairBoxSkin.normal.background = tex;
		//Initially point gun forward
		Transform cam = Camera.main.transform;
		Target=cam.position+cam.forward*5f;
		//Only display if owner of this character
		if(Owner.networkView!=null)
			DisplayGUI=Owner.networkView.isMine;
	}
	
	//Called every 0.02 seconds, handles aiming at whatever player is looking directly at
	void FixedUpdate () {
		//If owner of this gun
		if(DisplayGUI){
			//Get camera and make new ray based on the direct forward vector from it
			Transform cam = Camera.main.transform;
			Ray ray = new Ray(cam.position, cam.forward);
			RaycastHit hit;
			if(Physics.Raycast (ray, out hit, 500)){
				//Attempt to find what are looking at and assign the hit point to target
				Target=hit.point;
			}else{
				//If nothing in front default to directly forwards
				Target=cam.position+cam.forward*5;
			}
			//Attempt to point gun at the target
			LookAtTarget();
			//Point the firing position at the target so bullets head that way too
			LookAtTarget(FireLocation);
		}
	}
	//Draws GUI
	void OnGUI(){
		if(DisplayGUI){
			//Get point of target on camera
			Vector2 centerPoint = Camera.main.WorldToScreenPoint(Target);
			//Draw ammo info
			GUI.Box (new Rect(0,25,70,25),"Bullets: "+CurrentAmmo.ToString());
			GUI.Box (new Rect(70,25,70,25),"Clips: "+SpareAmmo.ToString());
			//Draw cross hairs with spread: about centerPoint
			GUI.Box(new Rect(centerPoint.x - width / 2, centerPoint.y - (height + GUISpread+spread*GUIMultiplier), width, height), "",CrossHairBoxSkin);
			GUI.Box(new Rect(centerPoint.x - width / 2, centerPoint.y + GUISpread+spread*GUIMultiplier, width, height), "",CrossHairBoxSkin);
			GUI.Box(new Rect(centerPoint.x + GUISpread+spread*GUIMultiplier, (centerPoint.y - width / 2), height , width), "",CrossHairBoxSkin);
			GUI.Box(new Rect(centerPoint.x - (height + GUISpread+spread*GUIMultiplier), (centerPoint.y - width / 2), height , width), "",CrossHairBoxSkin);
		}
	}
	//Causes the gun to point towards Target
	protected void LookAtTarget(){
		//Get slerp towards the target so gun doesnt snap so hard
		Quaternion newRot = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (Target - transform.position), 0.3f);
		//Set rotation
		transform.rotation = newRot;
		transform.LookAt(Target);	
	}
	//Causes a transform to look towards the Target
	protected void LookAtTarget(Transform t){
		t.LookAt(Target);	
	}
	//Sets all pixels of a texture to be equal to a specified color
	void SetColor(Texture2D myTexture ,  Color myColor ){
		//Iterate through pixels
		for (int y  = 0; y < myTexture.height; ++y){
			for (int x = 0; x < myTexture.width; ++x){
				//Set pixel color
				myTexture.SetPixel(x, y, myColor);
			}
		}
		//Apply changes
		myTexture.Apply();
	}
}
