using UnityEngine;
using System.Collections;
/* Author: 	Glynn Taylor (Apex5, Crazedfish, RUI)
 * Date: 	September 2014
 * Purpose: Helper base class for GUI functions, atm only does drawing a status bar on the GUI
 * Usage: 	Zero attribution required on builds, please attribute in code when releasing source
 *			code (when not heavily modified).
 */
public class GUIExtender : MonoBehaviour
{	
	//Draws a status bar on the GUI
	public void DrawStatusBar (float startx, float starty, float width, float height, float percent)
	{
		int WIDTH = Screen.width;
		int HEIGHT = Screen.height;
		//Draw outline
		GUI.Box (new Rect (WIDTH * startx,  HEIGHT * starty, width*WIDTH, height*HEIGHT), "");
		//Draw percentage bar
		GUI.Box (new Rect (WIDTH * startx + 5,  HEIGHT * starty + 5, (width*WIDTH - 10) * percent, height*HEIGHT - 10), "");
	}
}
