﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TrafficSimulation{

    public class CreateCar : MonoBehaviour
    {   
        public float Y_Position = 1.5f;
        // 뽑을 Segment
        public GameObject segment;
        // 뽑을 트럭
        public GameObject Truck;
        public string selectedTruck;

        // Segment별 Position
        public Vector3 path_Position;
        public Quaternion path_Rotation;

        public string selectedPath;
        public string nameTag;

        List<string> pathName = new List<string>();
        List<string> truckName = new List<string>();

        Dictionary<string, string> tagsDic = new Dictionary<string, string>();
        Dictionary<string, Quaternion> rotationsDic = new Dictionary<string, Quaternion>();

        
        // Start is called before the first frame update
        void Start()
        {   
            AddPathName();
            AddTruckName();
            AddTags();
            AddRotation();
            GetPathName();
            GetPathPosition();
            GetPathRotation();
            GetTruck();
            Create();
        }

        public void AddPathName()
        {
            pathName.Add("path_0");
            pathName.Add("path_1");
            pathName.Add("path_2");
            pathName.Add("path_3");
        }
        
        public void AddTags()
        {
            //path에 tag는 string 값으로 추가
            tagsDic.Add(pathName[0], "place1");
            tagsDic.Add(pathName[1], "place2");
            tagsDic.Add(pathName[2], "place3");
            tagsDic.Add(pathName[3], "place4");
        }
    
        public void AddRotation()
        {
            //path종류는 String, 좌표는 V3 값으로 추가
            rotationsDic.Add(pathName[0], Quaternion.Euler(0, 0, 0));
            rotationsDic.Add(pathName[1], Quaternion.Euler(0, 180, 0));
            rotationsDic.Add(pathName[2], Quaternion.Euler(0, 180, 90));
            rotationsDic.Add(pathName[3], Quaternion.Euler(0, 0, 0));
        }

        public void AddTruckName()
        {
            truckName.Add("Truck1");
            truckName.Add("Truck2");
        }

        public void GetPathName()
        {
            int pathRandomNum = Random.Range(0, 4);
            selectedPath = pathName[pathRandomNum];
            Debug.Log("selected path : " + selectedPath);
        }

        // 뽑은 Segment position 얻기
        public void GetPathPosition()
        {   
            path_Position = GameObject.Find(selectedPath).transform.position;
            // Debug.Log("path_Position : "+ path_Position);
        }

        public void GetPathRotation()
        {
            path_Rotation = rotationsDic[selectedPath];
        }

        public void GetTag()
        {
            nameTag = tagsDic[selectedPath];
        }

        public void GetTruck()
        {   
            int truckRandomNum = Random.Range(0, 1);
            selectedTruck = truckName[truckRandomNum];
            
            Debug.Log("truckRandomNum : "+ truckRandomNum);
            // Debug.Log("truckName[truckRandomNum] : "+ truckName[truckRandomNum]);
            // Truck = Resources.Load<GameObject>(truckName[truckRandomNum]);
            Debug.Log(selectedTruck);
        }

        // Truck 인스턴스화 하기
        void Create()
        {   
            Truck = Resources.Load<GameObject>(selectedTruck);
            Truck.GetComponent<VehicleAI>().trafficSystem = FindObjectOfType<TrafficSystem>();
            Truck.AddComponent<SetNameTag>();
            Truck.GetComponent<SetNameTag>().truckNameTag = nameTag;
            Instantiate(Truck, path_Position, path_Rotation);
            // Instantiate(Truck, GameObject.Find("path_0").transform.position, Quaternion.Euler(0, 0, 0));
            // Debug.Log("Truck traffic system is " + Truck.GetComponent<VehicleAI>().trafficSystem);
        }


    }
}
