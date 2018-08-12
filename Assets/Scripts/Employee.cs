using System;
using UnityEngine;
using UnityEngine.AI;

public class Employee : MonoBehaviour
{

    public enum SatisfactionStatus
    {
        WORK,
        IDLE,
        RESTROOM,
        ENTERTAINMENT,
        COFFEE
    }

    private enum MovementStatus
    {
        IDLE,
        MOVING,
        SATISFYING,
        CANT_REACH
    }

    // When satisfaction drops below this threshold, the employee will try to satisfy this need.
    [Header("Satisfaction Threshold")]    
    public float EntertainmentThreshold;
    public float FoodSatisfactionThreshold;
    public float BathroomSatisfactionThreshold;

    private float Anger = 0;

    [Header("Satisfaction Factors")]
    public float EntertainmentFactor = 4f;
    public float FoodSatisfactionFactor = 6f;
    public float BathroomSatisfactionFactor = 6f;
    public float AngerManagementFactor = 0.25f;

    private float EntertainmentSatisfaction;
    private float FoodSatisfaction;
    private float BathroomSatisfaction;
    public NeedSatisfier Workstation { get; private set; }
    public float AngerLimit;

    private NavMeshAgent agent;
    private SatisfactionStatus currentSatisfactionStatus;
    private MovementStatus currentMovementStatus;
    private NeedSatisfier currentSatisfier;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentSatisfactionStatus = SatisfactionStatus.IDLE;
        currentMovementStatus = MovementStatus.IDLE;
        currentSatisfier = null;

        EntertainmentSatisfaction = 100f;
        FoodSatisfaction = 100f;
        BathroomSatisfaction = 100f;
    }

    private void Update()
    {        
        UpdateNeeds();
        DecideStatus();
        UpdateSatisfiers();
        UpdateAnger();
    }

    private void UpdateAnger()
    {
        if(Anger >= AngerLimit)
        {
            FindObjectOfType<UIUpdater>().ShowHint("An employee just quit... \n Consider having all needs covered");
            Destroy(Workstation.gameObject);
            Destroy(this.gameObject);
        }
        Anger -= AngerManagementFactor * Time.deltaTime;
    }

    private void UpdateSatisfiers()
    {
        if(currentSatisfier == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, currentSatisfier.waitingQueue.position);

        // TODO: Move threhsold to local variable
        if (distance <= 0.1f)
        {
            currentSatisfier.QueueEmployee(this);
            currentMovementStatus = MovementStatus.SATISFYING;            
        }
    }

    // THIS IS BAD CODE... THIS NEEDS TO BE REPLACED WITH GENERIC MODEL FOR EACH NEED IF 
    // I DECIDE TO MAKE MORE NEEDS FOR THE EMPLOYEE

    private void UpdateNeeds()
    {
        // Only get bored while working
        if(currentSatisfactionStatus == SatisfactionStatus.WORK && currentMovementStatus == MovementStatus.SATISFYING)
        {
            EntertainmentSatisfaction -= EntertainmentFactor * Time.deltaTime;

            // Are we below threshold?
            if(EntertainmentSatisfaction <= EntertainmentThreshold)
            {
                Need(SatisfactionStatus.ENTERTAINMENT);
            }
            
            // Force satisfaction to be positive
            if (EntertainmentSatisfaction <= 0)
            {
                EntertainmentSatisfaction = 0;
            }
        }        

        FoodSatisfaction -= FoodSatisfactionFactor * Time.deltaTime;

        if (FoodSatisfaction <= FoodSatisfactionThreshold)
        {
            Need(SatisfactionStatus.COFFEE);
        }

        if (FoodSatisfaction <= 0)
        {
            FoodSatisfaction = 0;
        }

        BathroomSatisfaction -= BathroomSatisfactionFactor * Time.deltaTime;

        if( BathroomSatisfaction <= BathroomSatisfactionThreshold)
        {
            Need(SatisfactionStatus.RESTROOM);
        }

        if (BathroomSatisfaction <= 0)
        {
            BathroomSatisfaction = 0;
        }
    }

    private void Need(SatisfactionStatus need)
    {
        // Are we moving towards something? (AKA: Are we trying to satisfy a need?)
        if(currentMovementStatus == MovementStatus.MOVING || currentMovementStatus == MovementStatus.SATISFYING)
        {
            // Don't update anything.
            return;
        }        

        // Set need, and move to the nearest satisfier
        currentSatisfactionStatus = need;

        // Do we need to work? if so, go to our station, no need to find nearest one.
        if ( currentSatisfactionStatus == SatisfactionStatus.WORK)
        {            
            if (Workstation == null)
            {
                // We didn't set workstation or it was removed...
                Debug.LogError("Workstation not found");
                currentMovementStatus = MovementStatus.CANT_REACH;
                return;
            }
            currentMovementStatus = MovementStatus.MOVING;
            currentSatisfier = Workstation;
            MoveTo(Workstation.satisfierStandPlace.position);
            return;
        }

        // Find the nearest need satisfaction and go there
        NeedSatisfier satisfier = SatisfierLocationDictionary.Instance.GetNearestSatisfier(need, transform.position);
        if(satisfier == null)
        {
            // No satisfier available
            currentMovementStatus = MovementStatus.CANT_REACH;
            Debug.LogError("No Satisfier Available");
            FindObjectOfType<UIUpdater>().ShowHint("An employee can't reach location for " + need.ToString());
            Anger += 15;
            return;
        }

        currentMovementStatus = MovementStatus.MOVING;
        currentSatisfier = satisfier;        
        MoveTo(satisfier.waitingQueue.position);
    }

    void DecideStatus()
    {

        if(currentMovementStatus == MovementStatus.CANT_REACH)
        {            
            // retry movement
            Need(currentSatisfactionStatus);
        }

        // TODO: Check other status?
        switch (currentSatisfactionStatus)
        {
            case SatisfactionStatus.IDLE:
                currentSatisfactionStatus = SatisfactionStatus.WORK;
                Need(SatisfactionStatus.WORK);
                break;            
        }
    }

    public void SatisfiedNeed(SatisfactionStatus need)
    {
        switch(need)
        {
            case SatisfactionStatus.WORK:
                GameManager.Instance.UpdateProgress(5f);
                break;
            case SatisfactionStatus.ENTERTAINMENT:
                EntertainmentSatisfaction = 100;
                break;
            case SatisfactionStatus.COFFEE:
                FoodSatisfaction = 100;
                break;
            case SatisfactionStatus.RESTROOM:
                BathroomSatisfaction = 100;
                break;            
        }
        currentSatisfactionStatus = SatisfactionStatus.IDLE;
        currentMovementStatus = MovementStatus.IDLE;
    }

    public void MoveTo(Vector3 destination)
    {
        Debug.Log("Moving to " + destination + " to satisfy: " + currentSatisfactionStatus.ToString());
        agent.SetDestination(destination);
    }

    public void SetWorkstation(NeedSatisfier Workstation)
    {
        if (Workstation.Need != SatisfactionStatus.WORK)
        {
            Debug.LogError("Wrong workstation");
            return;
        }
        this.Workstation = Workstation;
    }
}

