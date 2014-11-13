using UnityEngine;
using System.Collections;

/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Failsafe trigger for map allowing for players to somehow escape map, teleports them back to spawn
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class TriggerTeleport : MonoBehaviour {
	//Points to teleport back to
	public Transform[] TeleportPoints;

	void OnTriggerEnter(Collider other) {
		//If character
		if(other.tag!="Bullet"){
			//Teleport back to a random spawn
			other.transform.position=TeleportPoints[Mathf.FloorToInt(Random.Range(0,TeleportPoints.Length-0.01f))].position;
		}
	}
}
