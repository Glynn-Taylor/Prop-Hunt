using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Base class for guns, handles firing logic for guns including spread
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public abstract class Gun : MonoBehaviour
{
	//Delay between firing
	public float FireDelay;
	//Time of last fire
	private float LastFireTime;
	//If gun can fire
	private bool CanFire = true;
	//Amount of spare ammo (clips/rounds)
	public float SpareAmmo;
	//Amount of current/useable ammo (rounds)
	public float CurrentAmmo;
	//Maximum ammo in clip
	public float MaxCurrentAmmo;
	//Projectile to fire
	public GameObject Projectile;
	//Location to fire from
	public Transform FireLocation;
	//speed of projectile
	public float SpeedMultiplier;
	//Range of projectile
	public float Range;
	//Reloading sound name
	public string ReloadName;
	protected bool Reloading;
	//Time to reload
	public float ReloadTime;
	protected float spread=0f;
	//Amount of visual spread on gun cursor
	protected const float GUISpread=3f;
	//Accuracy decrease per shot
	protected const float SpreadDecrement=0.1f;
	//Cap on accuracy decrease
	protected const float MaxSpread =2f;
	//Multiplier to use when offsetting firing
	protected const float SpreadMultiplier =0.04f;
	//Who fired this bullet
	public HunterCharacter Owner;
	
	//Attempt to fire a projectile
	public void Fire ()
	{
		//Attempt to reload if no ammo
		if (CurrentAmmo == 0) {
			Reload ();
		//If have ammo and not reloading
		} else if(CurrentAmmo>0&!Reloading) {
			//Check if time between shots has not elapsed
			if (!CanFire) {
				//Check if has since last frame
				if (Time.time - LastFireTime > FireDelay) {
					//Fire a projectile
					FireProjectile();
					//Increase spread as continual firing
					spread+=SpreadDecrement;
					spread=spread>MaxSpread?MaxSpread:spread;
				}
			} else {
				//Fire a projectile
				FireProjectile();
			}	
		}
		
	}
	//Fires a bullet
	protected void FireProjectile(){
		//Set time of last fire to now
		LastFireTime=Time.time;
		//Can't fire again till delay elapsed
		CanFire=false;
		//Instantiate a bullet on the network
		GameObject go = (GameObject)Network.Instantiate(Projectile,FireLocation.position,FireLocation.rotation,0);
		
		//Set this bullets velocity based on players movement speed + fire direction and speed, modified by spread
		Vector3 vp = transform.parent.parent.gameObject.GetComponent<CharacterMotor>().movement.frameVelocity;
		go.GetComponent<ProjectileScript>().SetVelocities(FireLocation.forward.x*SpeedMultiplier+vp.x+(Random.value-0.5f)*SpreadMultiplier*spread,FireLocation.forward.y*SpeedMultiplier+vp.y+(Random.value-0.5f)*spread*SpreadMultiplier,FireLocation.forward.z*SpeedMultiplier+vp.z+(Random.value-0.5f)*spread*SpreadMultiplier);
		//Set range of bullet so can kill if travels too far
		go.GetComponent<ProjectileScript>().setRange(Range);
		//Ignore the owner's collider
		go.GetComponent<ProjectileScript>().setIgnore(transform.parent.parent.collider);
		go.GetComponent<ProjectileScript>().setOwner(Owner);
		//Play audio and decrease ammo
		audio.PlayOneShot(audio.clip);
		CurrentAmmo--;
	}
	//Abstract method for reloading
	public abstract void Reload ();
	//Method for increasing accuracy over time when not firing
	public void ReduceSpread ()
	{
		if(spread!=0){
			//Decrease spread until 0
			spread-=SpreadDecrement;
			spread=spread<0?0:spread;
		}
	}
}
