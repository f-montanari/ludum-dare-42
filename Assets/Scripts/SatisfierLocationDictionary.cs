using System;
using System.Collections.Generic;
using UnityEngine;

public class SatisfierLocationDictionary
{
    #region SINGLETON
    private static SatisfierLocationDictionary m_Instance;
    public static SatisfierLocationDictionary Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = new SatisfierLocationDictionary();                
            }
            return m_Instance;            
        }
    }
    #endregion

    private Dictionary<Employee.SatisfactionStatus, List<NeedSatisfier>> Satisfiers;

    protected SatisfierLocationDictionary()
    {
        Satisfiers = new Dictionary<Employee.SatisfactionStatus, List<NeedSatisfier>>();
    }

    public List<NeedSatisfier> GetSatisfierList(Employee.SatisfactionStatus key)
    {
        if (Satisfiers.ContainsKey(key))
            return Satisfiers[key];
        else
            return null;
    }

    public void AddSatisfier(Employee.SatisfactionStatus key, NeedSatisfier satisfier)
    {
        if (!Satisfiers.ContainsKey(key))
        {
            Satisfiers[key] = new List<NeedSatisfier>();            
        }
        Satisfiers[key].Add(satisfier);
    }

    public void RemoveSatisfier(Employee.SatisfactionStatus key, NeedSatisfier satisfier)
    {
        if(!Satisfiers.ContainsKey(key))
        {
            return;
        }
        Satisfiers[key].Remove(satisfier);
    }

    public NeedSatisfier GetNearestSatisfier(Employee.SatisfactionStatus key, Vector3 position)
    {
        if(!Satisfiers.ContainsKey(key))
        {
            return null;
        }

        List<NeedSatisfier> satisfiers = Satisfiers[key];
        // If there's no satisfier available (this is, if they're all being used or there aren't any) return null.
        NeedSatisfier nearestSatisfier = null;

        float lastDistance = Mathf.Infinity;

        foreach (NeedSatisfier satisfier in satisfiers)
        {
            if(!satisfier.isAvailable)
            {
                // Can't use this guy... next!
                continue;
            }

            float distance = Vector3.Distance(satisfier.transform.position, position);
            if (distance < lastDistance)
            {
                lastDistance = distance;
                nearestSatisfier = satisfier;
            }
        }
        return nearestSatisfier;
    }
}

