using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Custom projectile for shotgun, first projectile creates many small projectiles with it
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class PelletScript : MonoBehaviour {
	//Materials to set pellets to, used to create a rainbow collection of pellets
	public Material[] Mats;
	//Factor to spread pellets out by
	private int spreadFactor = 5;
	
	//Initialization
	void Start () {
		//Look at target
		transform.rotation=Quaternion.LookRotation(GetComponent<ProjectileScript>().getVelocity());
		//Create pellets
		for(int i = 0; i<25; i++){
			//Instantiate a pellet
			GameObject go = (GameObject)Instantiate(gameObject,transform.position,transform.rotation);
			//Spread in a random direction bounded by spread factor
			go.transform.Rotate(new Vector3(Random.Range(-spreadFactor, spreadFactor),Random.Range(-spreadFactor, spreadFactor),0));
			//Set speed
			Vector3 newVelocity = new Vector3(go.transform.forward.x,go.transform.forward.y,go.transform.forward.z).normalized*GetComponent<ProjectileScript>().getVelocity().magnitude;         
			go.GetComponent<ProjectileScript>().SetVelocities(newVelocity.x,newVelocity.y,newVelocity.z);
			//Get a random material from array
			go.transform.GetChild(0).renderer.material=Mats[Random.Range (0,Mats.Length)];
			//Destroy the attached pellet script so does not keep making lots more
			Destroy(go.GetComponent<PelletScript>());
		}
		//Remove this script
		Destroy (this);
	}
}
