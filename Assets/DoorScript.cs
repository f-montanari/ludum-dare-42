using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {        
        if(other.CompareTag("Employee"))
        {
            Open();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Employee"))
        {
            Close();
        }
    }

    private void Close()
    {
        this.transform.rotation = Quaternion.Euler(0, 90, 0);
    }

    private void Open()
    {
        this.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}
