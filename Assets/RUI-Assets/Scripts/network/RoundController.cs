using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Handles spawning and round logic: starting and ending conditions
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class RoundController  : MonoBehaviour
{
	//Amount of players currently alive
	public int PlayersAlive = 0;
	//Prefab for spawning new observers
	public GameObject ObserverPrefab;
	//Prefab for spawning new prop characters
	public GameObject PropPlayerPrefab;
	//Prefab for spawning new hunter characters
	public GameObject HunterPlayerPrefab;
	//A list of spawns to use (children of this object)
	private Transform[] Spawns;
	//Time to wait after level loading to start a round (to allow people time to conenct)
	private const float TimeToStart=15f;
	//Interval between checking how many players alive
	private const float TimeToCheckPlayers=0.4f;
	//Length of a match (changeable per level)
	public  float MatchTime=210f;
	//Timers for elapsed times
	private float Timer_RoundStart=0f;
	private float Timer_TimeToCheckPlayers=0f;
	private int Timer_RoundStartInt=0;
	private float Timer_MatchTimer=0;
	//If the round has started
	public bool MatchStarted=false;
	//If there are enough players to play
	private bool PlayersConnected=false;
	
	//Initialisation
	void Start(){
		//Assign RoundHandler to NetworkHandler
		GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>().RoundHandler=this;
		//Get list of spawns
		Spawns= new Transform[transform.childCount];
		for(int i=0;i<transform.childCount;i++){
			Spawns[i]=transform.GetChild(i);
		}
		//Spawn spectator for owner
		SpawnPlayer();
	}
	//Creates a spectator
	public void SpawnPlayer(){
		//Create a controllable spectator camera
		GameObject.Instantiate(ObserverPrefab,Spawns[0].position,Spawns[0].rotation);
	}
	//Tells all connected clients to destroy their spectator and spawn in on a specified team
	[RPC]
	void StartRound (int pos, bool b)
	{
		Debug.Log ("Starting round");
		//Destroy observer
		GameObject.Destroy (GameObject.FindGameObjectWithTag ("Observer"));
		//Create either a hunter or a prop dependent on information passed by server
		Network.Instantiate (b?HunterPlayerPrefab:PropPlayerPrefab, Spawns[pos].position, Spawns[pos].rotation, 0);
		Timer_RoundStartInt=0;
		Timer_MatchTimer=MatchTime;
		MatchStarted=true;
	}
	//Called every frame, updates the match timer
	void Update(){
		if(MatchStarted)
			Timer_MatchTimer-=Time.deltaTime;
	}
	//Called (average) 50 times per sec
	void FixedUpdate ()
	{
		//If client
		if (!Network.isServer) {
			
		//If server	
		} else {
			//Check if match time limit has run out
			if(MatchStarted&&Timer_MatchTimer<0)
				EndGame();
			//Check if draw/no players currently non-spectators
			if (PlayersAlive == 0) {
				//Check if have enough people to start a round
				if (!MatchStarted&&Network.connections.Length+1 > 1) {
					//If time left till the round starts
					if(Timer_RoundStart>0){
						//Decrease time left till round starts
						Timer_RoundStart-=Time.deltaTime;
						//Show clients time left for round to start
						if((Timer_RoundStartInt)!=(int)Timer_RoundStart){
							Timer_RoundStartInt=(int)Timer_RoundStart;
							networkView.RPC ("setRoundStartInt",RPCMode.Others,Timer_RoundStartInt);
						}
					}else{
						//If just recieved enough players to start a round, set time to start round
						if(!PlayersConnected){
							PlayersConnected=true;
							Timer_RoundStart=TimeToStart;
							Timer_RoundStartInt=(int)TimeToStart;
							//Show clients time left for round to start
							networkView.RPC ("setRoundStartInt",RPCMode.Others,Timer_RoundStartInt);
						}else{
							//Set amount of props and hunters, half and half, weighted towards props
							int Props = Mathf.CeilToInt(((float)Network.connections.Length+1f)/2f);
							int Hunters = Mathf.FloorToInt(((float)Network.connections.Length+1f)/2f);
							//Get list of connections
							NetworkPlayer[] Players = Network.connections;
							int i = 0;
							bool Hunter=false;
							foreach (NetworkPlayer np in Players) {
								Debug.Log ("Spawned someone else");
								//Set to either hunter or prop unless amount runs out of one
								Hunter = (Props==0?true:(Hunters==0?false:randomBoolean()));
								//Tell player to start round on the above team
								networkView.RPC ("StartRound", np, i,Hunter);
								i++;
								//Decrease hunter/prop slots left
								if(Hunter)
									Hunters--;
								else
									Props--;
							}
							//Get a random assignment for server host
							Hunter = (Props==0?true:(Hunters==0?false:randomBoolean()));
							StartRound(i,Hunter);
							//Set amount of players alive (clients + server)
							PlayersAlive=Network.connections.Length+1;
							//Round has been started
							MatchStarted=true;
							//Check for players connected again for next round so that doesnt start it again with < 2 players
							PlayersConnected=false;
						}
					}
				}
			//Otherwise game is running so check status
			} else {
				//If is time to count players again
				if(Timer_TimeToCheckPlayers>TimeToCheckPlayers){
					Timer_TimeToCheckPlayers=0;
					//Calculate amount of hunters, props left
					int hunters = GameObject.FindGameObjectsWithTag("Hunter").Length;
					int props = GameObject.FindGameObjectsWithTag("PropCollider").Length;
					//If either team has one then end the game
					if(hunters==0||props==0)
						EndGame();
					//Set players alive
					PlayersAlive=hunters+props;
				//Otherwise increment time since last check
				}else{
					Timer_TimeToCheckPlayers+=Time.deltaTime;
				}
			}
		}
	}
	//Updates the clients UI to show time till round start
	[RPC]
	void setRoundStartInt(int i){
		Timer_RoundStartInt=i;
	}
	//Ends the round via the network controller
	void EndGame ()
	{
		GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>().RestartLevel();
	}
	//Checks if player owns an observer and if not spawns one in
	[RPC]
	void CheckObserver(){
		//Get array of observers in game
		GameObject[] goa = GameObject.FindGameObjectsWithTag("Observer");
		if(goa.Length==0)
			SpawnPlayer();
	}
	//Draws GUI
	void OnGUI(){
		//If waiting for round to start then display timer if round is starting
		if(!MatchStarted&&Timer_RoundStartInt>0){
			GUI.Box(new Rect(Screen.width/2-75,Screen.height/2-20,150,40),"Round starting in:"+Environment.NewLine+Timer_RoundStartInt.ToString());
		//Otherwise if in match, display time left till round ends
		}else if (MatchStarted){
			int i = (int)Timer_MatchTimer;
			GUI.Box(new Rect(Screen.width-100,0,100,25),(i/60).ToString()+":"+(i%60).ToString());
		}
	}
	//Gets a random bool
	bool randomBoolean()
	{
		return UnityEngine.Random.value >= 0.5;
	}
}
