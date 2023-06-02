﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// #if UNITY_EDITOR
using UnityEditor;
// #endif

namespace TrafficSimulation{
    public class ExitPlayMode : MonoBehaviour
    {
        public int nowTruckCount;
        [SerializeField] private int totalTruckCount;

        // private CreateTruckAndStation createTruckAndStation;

        void Start()
        {
            nowTruckCount = 0;
            totalTruckCount = CreateTruckAndStation.truckDataList.Count;
            
        }

        // Update is called once per frame
        private void Update()
        {
    // #if UNITY_EDITOR
            if(CompareTruckCount(nowTruckCount, totalTruckCount))
            {
                Debug.Log("Exit Play Mode");
                EditorApplication.ExitPlaymode();
            }
    // #endif
        }

        private bool CompareTruckCount(int _nowTruckCount, int _totalTruckCount)
        {
            return _nowTruckCount == _totalTruckCount;
        }

        
    }
}