using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
/// <summary>Script for a button to change camera transfor according to given target, direction and distance</summary>
public class FlyCamViewButton : MonoBehaviour
{
    public enum CoordinateFrame
    {
        GlobalFrame,
        TargetFrame
    }
    public FlyCam flyCam;
    public Transform target;
    public float distance;
    public CoordinateFrame coordinateFrame;
    public Vector3 directionFromTarget = Vector3.forward;
    public Vector3 cameraUpDirection = Vector3.up;
    public bool isInitial {get; private set;} = false;

    private Button button;

    void Awake()
    {
        isInitial = Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isInitial)
            isInitial = Initialize();
    }

    bool Initialize()
    {
        button = GetComponent<Button>();
        if(!flyCam)
            flyCam = FindObjectOfType<FlyCam>();
        if(!button || !flyCam || !target)
            return false;
        switch(coordinateFrame)
        {
            case CoordinateFrame.GlobalFrame:
                button.onClick.AddListener(delegate{ flyCam.target = target; flyCam.LookAtTargetFrom(directionFromTarget, cameraUpDirection, distance); });
                break;
            case CoordinateFrame.TargetFrame:
                button.onClick.AddListener(delegate{ flyCam.target = target; flyCam.LookAtTargetFromLocal(directionFromTarget, cameraUpDirection, distance); });
                break;
            default:
                return false;
        }
        return true;
    }
}
