using UnityEngine;
using System.Collections;

public class YourPlayerController : BasicPlayerController {

	void Start()
	{
	}
	
	void Update()
	{
		UpdateBasicController(); //This void is in BasicPlayerController, and updates position/rotation of the object
		//your code goes here! :-)
	}
}
