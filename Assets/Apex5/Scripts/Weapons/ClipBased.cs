using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Handles reloading for clip based guns, when reloading entire clip is replaced with new one
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public abstract class ClipBased : Gun
{
	#region implemented abstract members of Gun
	public override void Reload ()
	{
		//If have spare ammo and not reloading and not at max ammo
		if (SpareAmmo > 0 & CurrentAmmo != MaxCurrentAmmo&!Reloading) {
			//Replace clip after reload time and set reloading + play reloading sound
			Invoke ("NewClip", ReloadTime);
			SoundManager.Instance.Play (ReloadName);
			Reloading=true;
		}else if(SpareAmmo==0){
			//Play gunempty sound if no spare ammo
			SoundManager.Instance.Play ("GunEmpty");
		}
	}
	//Replaces current clip with new clip
	private void NewClip ()
	{
		//Reset ammo
		CurrentAmmo = MaxCurrentAmmo;
		//Remove clip from spares
		SpareAmmo -= 1;
		Reloading=false;
	}
	#endregion
}
