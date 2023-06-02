﻿using System.Collections;
using System.Collections.Generic;
// using System.Diagnostics;
using UnityEngine;

namespace TrafficSimulation{
    public class TruckInfo : MonoBehaviour
    {

        public List<Vector3> truckWorkStations;
        public Vector3 truckOrigin;
        public Vector3 truckDestination;
        public string truckRouteName;
        public int truckStatus;

        
        private GameObject vehicle;
        private VehicleAI thisVehicleAI;

        
        public float short_slowingTime = 2f;
        public float long_slowingTime = 3f;

        public float moveDelay = 1f;
        public float processTime = 10f;


        private Vector3 originalPos;
        private float toRightNum = 15f;
        private float toLeftNum = 30f;
        private float checkRange_1 = 20f;
        private float checkRange_2 = 1f;


        private GameObject nowStation;
        private StationsInfo nowStationInfo;
        private int nowStation_FinishedVehicle_toLeft;
        private int nowStation_FinishedVehicle_toRight;

        // To check Station's Status
        private float checkDelay = 0.1f; 

        private Timer truckTimer;

        private ExitPlayMode exitPlayMode;

        // Start is called before the first frame update
        void Start()
        {
            truckStatus = 0;
            vehicle = this.gameObject;
            thisVehicleAI = vehicle.GetComponent<VehicleAI>();
            truckTimer = vehicle.GetComponent<Timer>();
            exitPlayMode = GameObject.Find("Roads").GetComponent<ExitPlayMode>();

            WheelDrive thisVehicleDr = vehicle.GetComponent<WheelDrive>();
            Debug.Log(vehicle.name + " streeing Speed Max : " + thisVehicleDr.steeringSpeedMax);
            Debug.Log(vehicle.name + " minSpeed : " + thisVehicleDr.minSpeed);
        }

        void OnTriggerEnter(Collider _other)
        {   
            if(_other.gameObject.tag == "Station")
            {
                // Get Station's Information
                nowStation = _other.gameObject;
                nowStationInfo = nowStation.GetComponent<StationsInfo>();

                nowStation_FinishedVehicle_toLeft = nowStationInfo.finishedVehicle_toLeft_Count;
                nowStation_FinishedVehicle_toRight = nowStationInfo.finishedVehicle_toRight_Count;

                // vehicle이 어느 방향으로 가는지 확인
                // 오른쪽 방향으로 가는 경우
                if(CheckRotation_IsToRight(vehicle))
                {
                    // 작업이 완료된 트럭이 있는지 확인
                    if(nowStation_FinishedVehicle_toRight > 0)
                    {   
                        UnityEngine.Debug.Log("there is finished vehicle to right");
                        // 작업이 완료된 트럭이 있다면 도착한 vehicle 감속 및 멈춤
                        StartCoroutine(ReduceSpeed(vehicle, short_slowingTime));
                        thisVehicleAI.vehicleStatus = Status.STOP;
                    }
                }

                // 왼쪽 방향으로 가는 경우
                else
                {
                    if(nowStation_FinishedVehicle_toLeft > 0)
                    {
                        UnityEngine.Debug.Log("there is finished vehicle to left");
                        // 작업이 완료된 트럭이 있다면 도착한 vehicle 감속 및 멈춤
                        StartCoroutine(ReduceSpeed(vehicle, short_slowingTime));
                        thisVehicleAI.vehicleStatus = Status.STOP;
                    }
                }

                // 작업하는 곳인지 확인
                string toWorkStation_Name = truckWorkStations[truckStatus].ToString();

                // 트럭이 작업해야하는 곳인 경우
                if(toWorkStation_Name == nowStation.name)
                {
                    truckStatus += 1;

                    // 도착지인 경우
                    if(IsDestination(toWorkStation_Name, truckDestination))
                    {
                        Debug.Log(vehicle.name +" arrives destination!!!");
                        StartCoroutine(LastWorkingProcess(vehicle, thisVehicleAI, nowStation, nowStationInfo, long_slowingTime, toRightNum, toLeftNum, 
                                                            moveDelay, checkDelay, checkRange_1, checkRange_2, processTime, truckTimer, exitPlayMode, truckRouteName, truckOrigin, truckDestination));
                    }
                    
                    // 도착지가 아닌 경우
                    else
                    {  
                        StartCoroutine(WorkingProcess(vehicle, thisVehicleAI, nowStation, nowStationInfo, long_slowingTime, toRightNum, toLeftNum, 
                                                                moveDelay, checkDelay, checkRange_1, checkRange_2, processTime, truckTimer));
                    }

                    // Debug.Log(vehicle.name + " truckTimer.stationWatchList.Count : " + truckTimer.stationWatchList.Count);
                }
            }
        }

        
        private IEnumerator WorkingProcess(GameObject _vehicle, VehicleAI _vehicleAI, GameObject _station, StationsInfo _stationInfo, float _slowingTime, 
                                            float _toRigthNum, float _toLeftNum, float _moveDelay, float _checkDelay, float _checkRange_1, float _checkRange_2, float _processTime, Timer _truckTimer)
        {
            // 감속
            Debug.Log(_vehicle.name + " SLOW_DOWN");
            _vehicleAI.vehicleStatus = Status.SLOW_DOWN;
            yield return StartCoroutine(ReduceSpeed(_vehicle, _slowingTime));

            // yield return new WaitForSeconds(3f);
            Debug.Log(_vehicle.name + " STOP");
            _vehicleAI.vehicleStatus = Status.STOP;

            if(_truckTimer != null)
            {
                if(_truckTimer.stationWatch != null)
                {
                    float stationArrivalTime = _truckTimer.TimerStop(_truckTimer.stationWatch);
                    _truckTimer.stationWatchList.Add(stationArrivalTime);
                }
                else
                {
                    Debug.LogError("_truckTimer.stationWatch 컴포넌트를 찾을 수 없습니다.");
                }
        
            }

            else
            {
                Debug.LogError("_truckTimer 컴포넌트를 찾을 수 없습니다.");
            }

            originalPos = _vehicle.transform.position;

            yield return StartCoroutine(MoveToProcess(_vehicle, _station, _stationInfo, _moveDelay, _toRigthNum, _toLeftNum, originalPos));

            yield return StartCoroutine(Processing(_processTime, _station, _stationInfo, _vehicle, _checkDelay));

            yield return StartCoroutine(MoveToOriginalPos(_vehicle, _vehicleAI, _station, _stationInfo, originalPos, _checkRange_1, _checkRange_2, _checkDelay));

            if(_truckTimer != null)
            {
                _truckTimer.stationWatch.Reset();
                Debug.Log( vehicle.name + " stationWatch reset");
                _truckTimer.stationWatch.Start();
            }
        }

        private IEnumerator LastWorkingProcess(GameObject _vehicle, VehicleAI _vehicleAI, GameObject _station, StationsInfo _stationInfo, float _slowingTime, float _toRigthNum, float _toLeftNum, float _moveDelay, float _checkDelay, 
                                                        float _checkRange_1, float _checkRange_2, float _processTime, Timer _truckTimer, ExitPlayMode _exitPlayMode, string _routeName, Vector3 _origin, Vector3 _destination)
        {
            // 감속
            yield return StartCoroutine(ReduceSpeed(_vehicle, _slowingTime));
            _vehicleAI.vehicleStatus = Status.STOP;

            if(_truckTimer != null)
            {
                if(_truckTimer.stationWatch != null)
                {   
                    float stationArrivalTime = _truckTimer.TimerStop(_truckTimer.stationWatch);
                    Debug.Log(_vehicle.name + " stationArrivalTime : " + stationArrivalTime);
                    _truckTimer.stationWatchList.Add(stationArrivalTime);
                }
                else
                {
                    Debug.LogError("Timer 컴포넌트를 찾을 수 없습니다.");
                }
        
            }

            else
            {
                Debug.LogError("Timer 컴포넌트를 찾을 수 없습니다.");
            }

            originalPos = _vehicle.transform.position;

            yield return StartCoroutine(MoveToProcess(_vehicle, _station, _stationInfo, _moveDelay, _toRigthNum, _toLeftNum, originalPos));

            yield return StartCoroutine(LastProcessing(_processTime, _station, _stationInfo, _vehicle, _checkDelay, _truckTimer, _exitPlayMode, _routeName, _origin, _destination));
        }

        private IEnumerator MoveToProcess(GameObject _vehicle, GameObject _station, StationsInfo _stationInfo, float _delay, float _toRigthNum, float _toLeftNum, Vector3 _originalPos)
        {
            yield return new WaitForSeconds(_delay);

            // UnityEngine.Debug.Log(_vehicle.name + "Move to Process");
            // After _delay seconds, move to process

            _vehicle.GetComponent<Collider>().enabled = false;

            if(CheckRotation_IsToRight(_vehicle))
            {
                _vehicle.transform.position = _originalPos + new Vector3(0, 0, _toRigthNum);
            }

            else
            {
                _vehicle.transform.position = _originalPos + new Vector3(0, 0, _toLeftNum);
            }

            // _station.GetComponent<StationsInfo>().queueList.Add(_vehicle);
            _stationInfo.queueList.Add(_vehicle);

        }
        

        private IEnumerator Processing(float _processTime, GameObject _station, StationsInfo _stationInfo, GameObject _vehicle, float _checkDelay)
        {   
            // station이 작업 처리 할 수 있는 지 확인
            while(!IsStationAvailable(_station))
            {
                yield return new WaitForSeconds(_checkDelay);
            }

            // Get Station information
            _stationInfo.stationStatus += 1;

            _stationInfo.queueList.Remove(_vehicle);

            yield return new WaitForSeconds(_processTime);

            PlusFinishedVehicle(_stationInfo, _vehicle);
        }


        private IEnumerator LastProcessing(float _processTime, GameObject _station, StationsInfo _stationInfo, GameObject _vehicle, float _checkDelay, 
                                                        Timer _truckTimer, ExitPlayMode _exitPlayMode, string _routeName, Vector3 _origin, Vector3 _destination)
        {   
            // station이 작업 처리 할 수 있는 지 확인
            while(!IsStationAvailable(_station))
            {
                yield return new WaitForSeconds(_checkDelay);
            }

            // Get Station information
            _stationInfo.stationStatus += 1;

            _stationInfo.queueList.Remove(_vehicle);

            yield return new WaitForSeconds(_processTime);

            _stationInfo.stationStatus -= 1;

            if(_truckTimer != null)
            {
                float totalTime = _truckTimer.TimerStop(_truckTimer.totalWatch);
                Debug.Log( _vehicle.name + " totalTime is " + totalTime);
                _exitPlayMode.nowTruckCount += 1;
                _truckTimer.SaveToCSV(_truckTimer.filePath, _vehicle.name, _routeName, _origin, _destination, totalTime, _truckTimer.stationWatchList);
            }

            else
            {
                Debug.LogError("Timer 컴포넌트를 찾을 수 없습니다.");
            }

            _vehicle.SetActive(false);
        }


        private IEnumerator MoveToOriginalPos(GameObject _vehicle, VehicleAI _vehicleAI, GameObject _station, StationsInfo _stationInfo, Vector3 _originalPos, float _checkRange_1, float _checkRange_2, float _checkDelay)
        {
            // 작업이 끝나면 주변에 트럭이 있는지 확인
            while(ExistAnyTruck(_originalPos, _checkRange_1, _checkRange_2))
            {   
                // UnityEngine.Debug.Log("there is vehicle near original position");
                yield return new WaitForSeconds(_checkDelay);
            }

            // UnityEngine.Debug.Log("Now You can go!");

            // Move to original position
            // UnityEngine.Debug.Log(_vehicle.name + "Move to original position");
            _vehicle.transform.position = _originalPos;

            _stationInfo.stationStatus -= 1;
            // _station.GetComponent<StationsInfo>().stationStatus -= 1;
            

            // Update finishedVehicle_Count
            // MinusFinishedVehicle(_station, _vehicle);
            MinusFinishedVehicle(_stationInfo, _vehicle);


            _vehicle.GetComponent<Collider>().enabled = true;
            _vehicleAI.vehicleStatus = Status.GO;

        }


        private bool CheckRotation_IsToRight(GameObject _vehicle)
        {
            bool isToRight = false;

            if(_vehicle.transform.rotation.y == 90)
            {
                isToRight = true;
            }

            return isToRight;
        }


        private bool IsStationAvailable(GameObject _station)
        {
            bool isAvailable = false;
            
            // Get Station's Status
            StationsInfo _stationInfo = _station.GetComponent<StationsInfo>();
            int _stationStatus = _stationInfo.stationStatus;
            int _stationCapacity = _stationInfo.stationCapacity;

            if(_stationStatus <= _stationCapacity)
            {
                isAvailable = true;
            }

            return isAvailable;
        }


        private IEnumerator ReduceSpeed(GameObject _vehicle, float _slowingTime)
        {   
            // UnityEngine.Debug.Log(vehicle.name + " reduces Speed");
            Rigidbody rb = _vehicle.GetComponent<Rigidbody>();
            
            Vector3 initialVelocity = rb.velocity;
            float elapsedTime = 0f;

            while (elapsedTime < _slowingTime)
            {
                rb.velocity = Vector3.Lerp(initialVelocity, Vector3.zero, elapsedTime / _slowingTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rb.velocity = Vector3.zero; // Ensure velocity is set to zero
            // UnityEngine.Debug.Log(vehicle.name + " rb.velocity is 0!!");
        }


        private bool ExistAnyTruck(Vector3 _position, float _checkRange_1, float _checkRange_2)
        {   
            // // Perform a raycast to check for vehicles on both sides
            // bool rightSide = CheckRaycast(_position, Vector3.right, _checkRange);
            // bool leftSide = CheckRaycast(_position, Vector3.left, _checkRange);

            // Perform a raycast to check for vehicles on both sides
            bool rightSide = CheckRaycast(_position, new Vector3(1f, 0f, 0f), _checkRange_1);
            bool leftSide = CheckRaycast(_position, new Vector3(-1f, 0f, 0f), _checkRange_1);
            bool forwardSide = CheckRaycast(_position, new Vector3(0f, 0f, 1f), _checkRange_2);
            bool backwardSide = CheckRaycast(_position, new Vector3(0f, 0f, -1f), _checkRange_2);

            return rightSide || leftSide || forwardSide || backwardSide;

            // return rightSide || leftSide;
        }


        private bool CheckRaycast(Vector3 _position, Vector3 _direction, float _range)
        {
            // Perform a raycast in the specified direction
            RaycastHit hit;
            if (Physics.Raycast(_position, _direction, out hit, _range))
            {   
                return true;
            }

            return false;
        }


        private void PlusFinishedVehicle(StationsInfo _stationInfo, GameObject _vehicle)
        {   
            // Debug.Log(_vehicle.name + " PlusFinishedVehicle");
            
            if(CheckRotation_IsToRight(_vehicle))
            {
                _stationInfo.finishedVehicle_toRight_Count += 1;
                // _station.GetComponent<StationsInfo>().finishedVehicle_toRight_Count += 1;
                // return _stationInfo.finishedVehicle_toRight_Count;
            }

            else
            {
                _stationInfo.finishedVehicle_toLeft_Count += 1;
                // _station.GetComponent<StationsInfo>().finishedVehicle_toLeft_Count += 1;
                // return _stationInfo.finishedVehicle_toLeft_Count;
            }
        }


        private void MinusFinishedVehicle(StationsInfo _stationInfo, GameObject _vehicle)
        {   
            // Debug.Log(_vehicle.name + " MinusFinishedVehicle");
            if(CheckRotation_IsToRight(_vehicle))
            {
                // _station.GetComponent<StationsInfo>().finishedVehicle_toRight_Count -= 1;
                _stationInfo.finishedVehicle_toRight_Count -= 1;
                // return _stationInfo.finishedVehicle_toRight_Count;
            }

            else
            {
                // _station.GetComponent<StationsInfo>().finishedVehicle_toLeft_Count -= 1;
                _stationInfo.finishedVehicle_toLeft_Count -= 1;
                // return _stationInfo.finishedVehicle_toLeft_Count;
            }
        }


        private bool IsDestination(string _stationName, Vector3 _destination)
        {
            return _stationName == _destination.ToString();
        }
 
    }
}