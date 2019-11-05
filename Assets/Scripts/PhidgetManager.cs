using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phidget22;
using Phidget22.Events;
using System;

public class PhidgetManager : MonoBehaviour
{

    [Header("Phidget")]
    public float multiplier = 150000;
    public float overallWeight;
    public Vector4 sensorVals;
    public List<VoltageRatioInput> sensors = new List<VoltageRatioInput>();

    private Vector4 sensorValsTarg, sensorValsCal;
    private float dataInt = 16, timer;
    private bool callibrated;

    [Header("OneEuroFilter Settings")]
    public bool filterOn = true;
    public float filterFrequency = 120.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;
    OneEuroFilter<Vector4> PhidgetFilter;

    float frontFoot, deltaFrontFoot, backFoot;

    // Start is called before the first frame update
    void Awake()
    {
        callibrated = true;
        PhidgetFilter = new OneEuroFilter<Vector4>(filterFrequency);

        for (int i = 0; i < 4; i++)
        {
            sensors.Add(new VoltageRatioInput());
            sensors[i].Channel = i;
            sensors[i].VoltageRatioChange += WeightChange;
            sensors[i].Attach += IsAttached;
            sensors[i].Open();
        }
        Invoke("Callibrate", 0.2f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        overallWeight = 0;

        //OneEuroFilter
        if (filterOn)
        {
            PhidgetFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            sensorVals = PhidgetFilter.Filter(sensorValsTarg);
        }
        else
        {
            sensorVals = sensorValsTarg;
        }


        for (int i = 0; i < 4; i++)
        {
            overallWeight += sensorVals[i];
        }

        if (overallWeight < 1 && !callibrated)
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                Callibrate();
            }
        }

        if (overallWeight > 2)
        {
            callibrated = false;
        }

        deltaFrontFoot = (sensorVals[1] + sensorVals[2]) - frontFoot;
        frontFoot = sensorVals[1] + sensorVals[2];
        backFoot = sensorVals[0] + sensorVals[3];


        //if (GameManager.instance != null)
        //{
        //    GameManager.instance.balanceBoardVals = sensorVals;
        //    GameManager.instance.totalWeight = overallWeight;
        //    GameManager.instance.frontFoot = frontFoot;
        //    GameManager.instance.backFoot = backFoot;
        //    GameManager.instance.deltaFrontFoot = deltaFrontFoot;
        //    GameManager.instance.moveYBalance = (sensorVals[0] + sensorVals[3]) - (sensorVals[1] + sensorVals[2]);
        //    GameManager.instance.moveXBalance = (sensorVals[2] + sensorVals[3]) - (sensorVals[0] + sensorVals[1]);
        //}

    }

    void IsAttached(object sender, AttachEventArgs e)
    {
        VoltageRatioInput ch = (VoltageRatioInput)sender;
        //print("channel attached: "+ch.Channel +"  Data Interval: "+dataInt+" ms");
        sensors[ch.Channel].DataInterval = (int)dataInt;
    }

    void WeightChange(object sender, VoltageRatioInputVoltageRatioChangeEventArgs e)
    {
        try
        {
            VoltageRatioInput axis = (VoltageRatioInput)sender;
            sensorValsTarg[axis.Channel] = (float)(sensorValsCal[axis.Channel] - sensors[axis.Channel].VoltageRatio * multiplier);
        }
        catch (Exception)
        {

            //
        }
        
    }

    public void Callibrate()
    {
        for (int i = 0; i < 4; i++)
        {
            try
            {
                sensorValsCal[i] = (float)(sensors[i].VoltageRatio * multiplier);
            }
            catch (Exception)
            {

               //
            }

            
        }
        print("callibrated");
        callibrated = true;
        timer = 0;
    }


    private void OnApplicationQuit()
    {
        for (int i = 0; i < 4; i++)
        {
            sensors[i].VoltageRatioChange -= WeightChange;
            sensors[i].Close();
            sensors[i] = null;
        }

        if (Application.isEditor)
        {
            Phidget.ResetLibrary();
        }
        else
        {
            Phidget.FinalizeLibrary(0);
        }

    }

}