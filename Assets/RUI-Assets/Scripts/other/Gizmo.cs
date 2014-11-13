using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Draws a sphere in the editor window when attached to an object
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class Gizmo : MonoBehaviour {
	//Colour of sphere
	public Vector3 Colour;
	//Radius of sphere
	public float Size=1;
	
	//Draws a sphere in the editor window (to show spawn positions etc)
	void OnDrawGizmos ()
	{
		Gizmos.color = new Color (Colour.x,Colour.y,Colour.z,0.8f);
		Gizmos.DrawSphere(transform.position, Size);
	}
}
