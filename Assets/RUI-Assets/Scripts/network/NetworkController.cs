using UnityEngine;
using System.Collections;
using System;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Provides all the UI for network connections and server connection logic.
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class NetworkController : MonoBehaviour
{
		//Script for handling rounds
		public RoundController RoundHandler;
		//Max amount of connections allowed to a server
		private const int NumberOfConnections = 10;
		//Type of game to get from master server
		private const string typeName = "ApexProp";
		//Name to show as info in server browser
		private const string gameName = "Apex Prop Hunter game";
		//Levels that can be loaded
		private String[] supportedNetworkLevels = { "NetworkTestLevel","HouseLevel","DustTestLevel" };
		//Level to load on disconnect
		private String disconnectedLevel = "Menu";
		//Level index counter
		private int lastLevelPrefix = 0;
		//Flag for if currently loading a level
		private bool isLoadingLevel = false;
		//IP for direct connection
		private String stringToEdit = "127.0.0.1";
		//Width of a button
		private int ButtonWidth = (Screen.width - Padding * 2) / 3;
		//Height of a button
		const int ButtonHeight = 25;
		//UI Padding
		const int Padding = 5;
		//UI skin
		private GUISkin UISkin;
		//In level select?
		private bool LevelSelect = false;
		//Is server LAN only
		private bool LAN = false;
		//Are in menu
		private bool Menu = false;
		//Is currently on the level select screen
		private bool levelSet=false;
		//Cache for servers found hosting prop hunt games
		private HostData[] hostList;
		//Connecting to a server
		private bool Connecting=false;
		//Debug helper string, displays status of attempting to join a server
		private String DebugString = "";
			
		//Initialization
		void Start ()
		{
				//Ensure this object isnt destroyed
				DontDestroyOnLoad (gameObject);
				//Get the skin for the browser/menus
				UISkin = ((GUISkin)Resources.Load ("UISkin"));
				networkView.group = 1;
		}
	
		// Update is called once per frame
		void Update ()
		{
			//Toggle menu on escape
			if(Input.GetKeyDown(KeyCode.Escape))
				Menu=!Menu;
			//Lock cursor on LMB if not in menu
			if (!Menu&&!levelSet &&(Network.isClient || Network.isServer)&&Input.GetMouseButton (0)) {
				Screen.lockCursor = true;
			}
		}
		//Creates a server and registers it
		private void StartServer ()
		{
				//Initialise on port 25000 and set local if not connected to internet
				Network.InitializeServer (8, 25000, !Network.HavePublicAddress ());
				//Register with master server
				MasterServer.RegisterHost (typeName, gameName);
		}
		//Starts a lan/non lan server on specified level
		private void StartServer (String s)
		{
				//Starts a server on 
				Network.InitializeServer (8, 25000, !LAN);
				//If not on lan need to register with master server
				if (!LAN)
					MasterServer.RegisterHost (typeName, gameName);
				//Set the level
				setLevel (s, lastLevelPrefix + 1);
		}
		//Restarts the current level
		public void RestartLevel(){
			setLevel (Application.loadedLevelName,lastLevelPrefix+1);
		}
		//Sets a level
		void setLevel (string level, int levelPrefix)
		{
				//Remove old RPC calls
				Network.RemoveRPCsInGroup (0);
				Network.RemoveRPCsInGroup (1);
				//Buffer a call to change levels so all connected + later connecting clients change
				networkView.RPC ("ChangeServerLevel", RPCMode.AllBuffered, level, levelPrefix);
		}
		//Start all users changing level
		[RPC]
		void ChangeServerLevel (string level, int levelPrefix)
		{
				StartCoroutine(WaitForLevelLoad(level,levelPrefix));
		}

		IEnumerator WaitForLevelLoad (string level, int levelPrefix)
		{
				lastLevelPrefix = levelPrefix;
				isLoadingLevel = true;
				// There is no reason to send any more data over the network on the default channel,
				// because we are about to load the level, thus all those objects will get deleted anyway
				Network.SetSendingEnabled (0, false);	
		
				// We need to stop receiving because first the level must be loaded first.
				// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
				Network.isMessageQueueRunning = false;
		
				// All network views loaded from a level will get a prefix into their NetworkViewID.
				// This will prevent old updates from clients leaking into a newly created scene.
				Network.SetLevelPrefix (levelPrefix);
				Application.LoadLevel (level);
				//Wait for level
				while (isLoadingLevel) {
						yield return new WaitForSeconds (0.1f);
				}
		
				// Allow receiving data again
				Network.isMessageQueueRunning = true;
				// Now the level has been loaded and we can start sending out data to clients
				Network.SetSendingEnabled (0, true);
		
				//Let level gameobjects know that they've been loaded in a networked game
				foreach (GameObject go in FindObjectsOfType<GameObject>())
						if (go.networkView != null)
								go.SendMessage ("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
		}
		//Prevents coroutine from running infinitely as triggers on a level change, required for the above logic
		void OnLevelWasLoaded ()
		{
				isLoadingLevel = false;
		}
		//Attempts to connect to a given ip
		void DirectConnect (String ip)
		{
			//Flag to show that are currently in the process of connecting
			Connecting=true;
			Network.Connect (ip, 25000);
		}
		
		void OnServerInitialized ()
		{
			//Server created, hence not connecting anymore
			Connecting=false;
		}
	
		void OnConnectedToServer ()
		{
			//Finished joining a server, hence not connecting anymore
			Connecting=false;
		}
		//Helper function to facilitate spawning an observer when someone connects
		private void SpawnPlayer ()
		{
				RoundHandler.SpawnPlayer ();
		}
		//Draw GUI
		void OnGUI ()
		{
				//If haven't started a server or are connected to a server then display the default menu
			if (!Network.isClient && !Network.isServer) {
						//If we are in the server browser or the level selection (for creating a server)
					if (!LevelSelect) {
							DisplayServers ();
					} else {
							DisplayLevelSelect ();
					}
			}
			//If in menu (in game)
			if(Menu){
				//If not selecting a level then display default menu
				if(!levelSet){
					//Displays a button for changing level if server, or simple resume button if not
					if(GUI.Button(new Rect(Screen.width/2-ButtonWidth/2,Screen.height/2-(ButtonHeight*1+Padding),ButtonWidth,ButtonHeight),Network.isServer?"Level Select":"Resume")){
						//Set menu flags if clicked
						if(Network.isServer){
							levelSet=true;
						}else{
							Menu=false;
						}
					}
					//Renders button for disconnecting
					if(GUI.Button(new Rect(Screen.width/2-ButtonWidth/2,Screen.height/2,ButtonWidth,ButtonHeight),"Disconnect")){
						Network.Disconnect();
						//Load default level
						Application.LoadLevel(0);
						//Set menu flags
						LevelSelect=false;
						Menu=false;
					}
					//Disconnects and quits the application
					if(GUI.Button(new Rect(Screen.width/2-ButtonWidth/2,Screen.height/2+(ButtonHeight*1+Padding),ButtonWidth,ButtonHeight),"Quit")){
						Network.Disconnect();
						Application.Quit();
					}
				//If in menu and in level select render that instead
				}else{
					DisplayLevelSet();
				}
			}
		}
		//Displays the level select screen
		void DisplayLevelSelect ()
		{
				//Set skin for rendering
				GUI.skin = UISkin;
				//Create a container for the UI
				GUI.BeginGroup (new Rect (Padding, Padding, Screen.width - Padding * 2, Screen.height - Padding * 2), UISkin.customStyles [0]);
					//Title label
					GUI.Label (new Rect (0, 0, 200, 75), "Level select");
					//Iterate through all level strings and create a button for each that starts a server with that level
					for (int i = 0; i < supportedNetworkLevels.Length; i++) {
						if (GUI.Button (new Rect (0, 100 + (110 * i), Screen.width - Padding * 2, 100), supportedNetworkLevels [i]))
								StartServer (supportedNetworkLevels [i]);
					}
				//End of container
				GUI.EndGroup ();
				//Reset UI drawing skin to default
				GUI.skin = null;
		}
		//Displays level select in game
		void DisplayLevelSet ()
		{
			//Set skin for rendering
			GUI.skin = UISkin;
			//Create a container for the UI
			GUI.BeginGroup (new Rect (Padding, Padding, Screen.width - Padding * 2, Screen.height - Padding * 2), UISkin.customStyles [0]);
				//Provide back button functionality by setting flag
				if(GUI.Button(new Rect(200,0,75,35),"Back"))
					levelSet=false;
				//Title
				GUI.Label (new Rect (0, 0, 200, 75), "Level select");
				//Iterate through all level strings and create a button for each that starts a server with that level, reset menu flags too
				for (int i = 0; i < supportedNetworkLevels.Length; i++) {
					if (GUI.Button (new Rect (0, 100 + (110 * i), Screen.width - Padding * 2, 100), supportedNetworkLevels [i])){
						setLevel (supportedNetworkLevels [i],lastLevelPrefix+1);
						levelSet=false;
						Menu=false;
					}
				}
			//End container
			GUI.EndGroup ();
			//Reset UI drawing skin to default
			GUI.skin = null;
		}
		//Displays a list of servers and buttons/UI to create/join servers
		void DisplayServers ()
		{
				//Set skin for rendering
				GUI.skin = UISkin;
				//Create a container for the UI
				GUI.BeginGroup (new Rect (Padding, Padding, Screen.width - Padding * 2, Screen.height - Padding * 2), UISkin.customStyles [0]);
					//Button for starting a non local server, sets flags to take user to level selection + mode
					if (GUI.Button (new Rect (ButtonWidth * 0, 5, ButtonWidth, ButtonHeight), "Start Server")) {
						LevelSelect = true;
						LAN = false;
					}
					//Button for starting a local server, sets flags to take user to level selection + mode
					if (GUI.Button (new Rect (ButtonWidth * 1, 5, ButtonWidth, ButtonHeight), "Start Local Server")) {
						LevelSelect = true;
						LAN = true;
					}
					//Causes list of servers to refresh
					if (GUI.Button (new Rect (ButtonWidth * 2, 5, ButtonWidth, ButtonHeight), "Refresh Hosts"))
						RefreshHostList ();
					//Create a container for the UI
					GUI.BeginGroup (new Rect (0, ButtonHeight + Padding * 2, Screen.width - Padding * 2, Screen.height - Padding * 4 - ButtonHeight * 2), UISkin.customStyles [0]);
						//Use cached list from RefreshHostList
						if (hostList != null) {
							for (int i = 0; i < hostList.Length; i++) {
								//Create a button for every found server to enable joining, displays some server info on it
								if (GUI.Button (new Rect (0, 100 + (110 * i), Screen.width - Padding * 2, 100), hostList [i].gameName+" "+hostList [i].connectedPlayers.ToString()+"/"+NumberOfConnections.ToString()))
									JoinServer (hostList [i]);
							}
						}
					GUI.EndGroup ();
					//Create a text field and get the text as the custom ip (set to localhost by default)
					stringToEdit = GUI.TextField (new Rect (0, Screen.height - Padding * 2 - ButtonHeight, ((Screen.width - Padding * 2) * 3) / 4, ButtonHeight), stringToEdit, 25);
					//Create a button for direct connecting using the custom ip
					if (GUI.Button (new Rect (((Screen.width - Padding * 2) * 3) / 4, Screen.height - Padding * 2 - ButtonHeight, ((Screen.width - Padding * 2) * 1) / 4, ButtonHeight), "Direct connect"))
						DirectConnect (stringToEdit);
					//Create a button for cancelling a connection
					if(Connecting)
						if (GUI.Button (new Rect (Padding+ButtonWidth/2, Padding+5+ButtonHeight, ButtonWidth/2, ButtonHeight), "Stop"))
							Connecting=false;
					GUI.EndGroup ();
					//Reset UI skin to default for next UI elements
					GUI.skin = null;
					//Debug info
					GUI.Label (new Rect (Padding, Padding+5+ButtonHeight, ButtonWidth/2, ButtonHeight), DebugString);
		
		}
		//Requests a list of hosts from the master server (of type ApexPropHunt)	
		private void RefreshHostList ()
		{
				MasterServer.RequestHostList (typeName);
		}
		//Handles recieving responses to requests from MS
		void OnMasterServerEvent (MasterServerEvent msEvent)
		{
				//If recieved a list of hosts
				if (msEvent == MasterServerEvent.HostListReceived)
						hostList = MasterServer.PollHostList ();
		}
		//Connects to a given server
		private void JoinServer (HostData hostData)
		{
			if(!Connecting){
				//Attempt to connect
				Network.Connect (hostData);
				//Allow for cancelling by setting flag
				Connecting=true;
				DebugString="Connecting..";
			}
		}
		//Handles failed connections
		void OnFailedToConnect(NetworkConnectionError error) {
			//Display feedback
			DebugString=("Could not connect to server: " + error);
			Connecting=false;
		}
		//Handles network disconnections, loads menu
		void OnDisconnectedFromServer ()
		{
				Application.LoadLevel (disconnectedLevel);
		}
		//Cleans up after a player disconnects from the server
		void OnPlayerDisconnected(NetworkPlayer player) {
			Debug.Log("Clean up after player " +  player.ToString());
			//Remove any leftover RPC calls and destroy objects owned by that player
			Network.RemoveRPCs(player);
			Network.DestroyPlayerObjects(player);
		}
	
}
