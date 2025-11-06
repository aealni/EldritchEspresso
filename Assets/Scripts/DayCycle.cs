using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class HourlySpawnConfig
{
    public DayCycle.DayTime hour; 
    [Tooltip("The value to assign to CustomerManager.spawn_delay during this hour.")]
    public float delaySeconds = 5f;
}

public class DayCycle : MonoBehaviour
{
    // Defines the 13 hourly segments from 9 AM to 9 PM
    public enum DayTime 
    { 
        NineAM = 0, TenAM = 1, ElevenAM = 2, Noon = 3, 
        OnePM = 4, TwoPM = 5, ThreePM = 6, FourPM = 7, 
        FivePM = 8, SixPM = 9, SevenPM = 10, EightPM = 11, 
        NinePM = 12 
    }

    [SerializeField] private int activeDayLengthSeconds = 300; 
    
    [Header("Spawn Rate Configuration")]
    [Tooltip("Customize the spawn_delay value for each hour segment.")]
    [SerializeField] private List<HourlySpawnConfig> spawnDelayConfigs = new List<HourlySpawnConfig>();
    
    public DayTime currentDayTime { get; private set; }

    public event Action<DayTime> OnTimeSegmentChanged;

    private int segmentLength;

    void Awake()
    {
        
        const int FIXED_SEGMENT_COUNT = 13;
        segmentLength = Mathf.Max(1, activeDayLengthSeconds / FIXED_SEGMENT_COUNT);
        currentDayTime = DayTime.NineAM;
        UpdateCustomerSpawnDelay(currentDayTime);
    }

    void Update()
    {
        if (activeDayLengthSeconds <= 0) return;

        int rawSeconds = Mathf.FloorToInt(GameTime.gameTimeSeconds); 
        int withinActiveDay = rawSeconds % activeDayLengthSeconds; 

        // Determine which hour we are currently in
        int index = Mathf.Clamp(withinActiveDay / segmentLength, 0, 12);

        DayTime newTime = (DayTime)index;
        
        if (currentDayTime != DayTime.NinePM && newTime == DayTime.NinePM)
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.Result);
        }

        if (newTime != currentDayTime)
        {
            currentDayTime = newTime;
            OnTimeSegmentChanged?.Invoke(currentDayTime);
            UpdateCustomerSpawnDelay(newTime);
        }
    }
    
    private void UpdateCustomerSpawnDelay(DayTime newTime)
    {
        HourlySpawnConfig config = spawnDelayConfigs.FirstOrDefault(c => c.hour == newTime);
        if (newTime >= DayTime.NinePM) 
        {
            CustomerManager.spawn_delay = float.MaxValue; 
            return; 
        }
        if (config != null)
        {
            CustomerManager.spawn_delay = config.delaySeconds; 
            Debug.Log($"DayCycle set CustomerManager.spawn_delay to {config.delaySeconds} for {newTime}.");
        }
        else
        {
            CustomerManager.spawn_delay = 10f; 
            Debug.LogWarning($"No config found for {newTime}. CustomerManager.spawn_delay defaulted to 10s.");
        }
    }
}
