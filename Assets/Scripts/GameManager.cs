using System;

public class GameManager
{
    #region SINGLETON
    private static GameManager m_Instance;
    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new GameManager();
            }
            return m_Instance;
        }
    }
    #endregion

    public float Money { get; protected set; }
    public float CurrentJobProgress { get; protected set; }
    public float CurrentJobTotal { get; protected set; }
    public float CurrentJobTime { get; protected set; }
    public float CurrentJobTotalTime { get; protected set; }
    public float CurrentJobPayment { get; protected set; }
    private int numJobs = 0;    

    private GameManager()
    {
        m_Instance = this;
        Money = 15000f;
        NewJob();
    }

    public void Update(float Time)
    {
        CurrentJobTime += Time;        
        if(CurrentJobProgress >= CurrentJobTotal)
        {
            Pay(3000f);
            NewJob();
        }
    }

    private void Pay(float amount)
    {
        Money += amount;
    }

    public void RemoveMoney(float amount)
    {
        if((Money - amount) < 0)
        {
            throw new Exception();
        }
        Money -= amount;
    }
    public void AddMoney(float amount)
    {
        Money += amount;
    }

    public void UpdateProgress(float progressMade)
    {
        CurrentJobProgress += progressMade;
    }

    private void NewJob()
    {
        CurrentJobProgress = 0;
        CurrentJobTime = 0;
        CurrentJobTotal = numJobs * 50 + UnityEngine.Random.Range(20,50);
        CurrentJobTotalTime = UnityEngine.Random.Range(120, 300);
        CurrentJobPayment = UnityEngine.Random.Range(3000, 6000);
    }

}

