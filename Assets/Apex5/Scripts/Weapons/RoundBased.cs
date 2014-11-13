using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	October 2013
 * Purpose: Handles logic for round based weapons, reloads one bullet at a time until clip full or player fires
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public abstract class RoundBased : Gun
{
	//Reloads weapon
	public override void Reload ()
	{
		//If have spare ammo and not at full and not reloading
		if (SpareAmmo > 0 & CurrentAmmo != MaxCurrentAmmo & !Reloading) {
			//Add ammo in reload time and set reloading + play sound
			Invoke ("AddBullet", ReloadTime);
			SoundManager.Instance.Play (ReloadName);
			Reloading = true;
		} else if (SpareAmmo == 0) {
			SoundManager.Instance.Play ("GunEmpty");
		}
	}
	//Adds a new round to clip
	private void AddBullet ()
	{
		//Add 1 to current ammo
		CurrentAmmo++;
		//Take one from spare ammo
		SpareAmmo -= 1;
		Reloading = false;
		//Keep reloading if not full and not player firing
		if(!Input.GetMouseButton(0))
			Reload ();
	}
}
