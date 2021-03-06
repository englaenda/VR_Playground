using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SharpOSC;


using UnityEngine;


public class WandController_v2 : MonoBehaviour {
	// Grip Button
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	// Trigger Button
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	// Controller
	private SteamVR_Controller.Device controller{ get { return SteamVR_Controller.Input((int)trackedObj.index); } }
	private SteamVR_TrackedObject trackedObj;

	// Pickup variable
	private GameObject pickup;

	//UDP shit
	private static string IP_add = "192.168.83.108";
	private static int port = 5510;

	// Simple mapping function
	float map2range(float val, float min_old, float max_old, float min_new, float max_new){
		return min_new + (val - min_old) * (max_new - min_new) / (max_old - min_old);
	}

	// Function to send Msg to Linux
	void sendPos(GameObject pos_arr){
		
		// x and z for plane, y is height
		float pos_x = pos_arr.GetComponent<Transform> ().position.x;
		float pos_y = pos_arr.GetComponent<Transform> ().position.y;
		float pos_z = pos_arr.GetComponent<Transform> ().position.z;

		float azi;
		float ele;
		//Debug.Log(pos_x);
		//Debug.Log(pos_y);
		//Debug.Log(pos_z);

		azi = Mathf.Atan2 (pos_x, pos_z)*180/Mathf.PI;
		ele = Mathf.Atan2(pos_y - 1.6f, Mathf.Sqrt(pos_x*pos_x + pos_z*pos_z))*180/Mathf.PI;

		var msg_azi = new SharpOSC.OscMessage ("/0x00/azi", azi);
		var msg_ele = new SharpOSC.OscMessage ("/0x00/ele", ele);
		var sender = new SharpOSC.UDPSender (IP_add, port);
		sender.Send(msg_azi);
		sender.Send(msg_ele);
	}

	// Use this for initialization
	void Start () {
		// Get Device ID
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
		// Controller Connected?
		if (controller == null) {
			Debug.Log("Controller not initialized");
			return;
		}

		// Pick and drag things
		// Press Down to pick up
		if(controller.GetPressDown(triggerButton) && pickup != null){
			pickup.transform.parent = this.transform;
			// pickup.GetComponent<Rigidbody> ().isKinematic = true;
		}
		// Hold Down to send pos
		if(controller.GetPress(triggerButton) && pickup != null){
			sendPos(pickup);
		}
		// Press Up to send final pos and then detatch 
		if(controller.GetPressUp(triggerButton) && pickup != null){
			sendPos(pickup);
			pickup.transform.parent = null;
			// pickup.GetComponent<Rigidbody> ().isKinematic = false;
		}

		// 


	}
		
	// Trigger activation when interacting with Cube/Sphere
	private void OnTriggerEnter(Collider collider) {
		pickup = collider.gameObject;
	}
	// Trigger deactivation when interacting with Cube/Sphere
	private void OnTriggerExit(Collider collider) {
		pickup = null;
	}


}
