using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedSatisfier : MonoBehaviour
{
    public string ObjectName;
    // The need that satisfies this object

    public Employee.SatisfactionStatus Need;
    public float TimeToSatisfy = 1f;
    public Transform satisfierStandPlace;
    public Transform waitingQueue;
    public float Value;

    [HideInInspector]
    public bool isAvailable = true;

    // If it's a workstation, we have an owner.
    [HideInInspector]
    public Employee owner;

    private Queue<Employee> waitingEmployees;
    private Employee currentUser;
    private float elapsedTimeSatisfying = 0;
    private MeshRenderer[] mrs;
    
    private void Start()
    {
        waitingEmployees = new Queue<Employee>();
        currentUser = null;
        mrs = GetComponentsInChildren<MeshRenderer>();
    }

    private void Update()
    {        
        if (currentUser != null)
        {            
            elapsedTimeSatisfying += Time.deltaTime;
            if(elapsedTimeSatisfying >= TimeToSatisfy)
            {
                SatisfyUser();
            }
            return;
        }

        isAvailable = true;

        if( waitingEmployees.Count > 0 )
        {
            SatisfyNextEmployee();
        }
    }

    private void SatisfyUser()
    {
        currentUser.SatisfiedNeed(Need);
        elapsedTimeSatisfying = 0;
        currentUser = null;
        isAvailable = true;
    }

    private void SatisfyNextEmployee()
    {
        currentUser = waitingEmployees.Dequeue();
        currentUser.MoveTo(satisfierStandPlace.position);
        currentUser.transform.rotation = satisfierStandPlace.rotation;
        isAvailable = false;
    }    

    public void QueueEmployee(Employee employee)
    {
        if(waitingEmployees.Contains(employee))
        {
            return;
        }
        waitingEmployees.Enqueue(employee);
    }
    

    public void BadlyPlaced()
    {
        foreach (MeshRenderer mr in mrs)
        {
            mr.material.color = Color.red;
        }
    }

    public void OkPlaced()
    {
        foreach (MeshRenderer mr in mrs)
        {
            mr.material.color = Color.white;
        }
    }    
}

