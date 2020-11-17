using System.Collections.Generic;
using UnityEngine;

namespace Freelance
{
    public class WorksData : ScriptableObject
    {
        public List<Day> days;
        public float hourlyWage;
        public float totalRevenue;
        public int extraTime;
    }

    [System.Serializable]
    public class Day
    {
        public string date;
        public List<string> startTime = new List<string>();
        public List<string> endTime = new List<string>();
        public List<int> minutes = new List<int>();
    }
}
