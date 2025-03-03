﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void updateFrontRightEncoders(float x);

    [DllImport("__Internal")]
    private static extern void updateFrontLeftEncoders(float x);

    [DllImport("__Internal")]
    private static extern void updateBackRightEncoders(float x);

    [DllImport("__Internal")]
    private static extern void updateBackLeftEncoders(float x);

    PlayerControls controls;
    private float linearVelocityX;
    private float linearVelocityY;
    private float angularVelocity;

    public bool usingJoystick = true;

    private float frontLeftWheelCmd = 0f;
    private float frontRightWheelCmd = 0f;
    private float backLeftWheelCmd = 0f;
    private float backRightWheelCmd = 0f;

    private float frontLeftWheelEnc = 0f;
    private float frontRightWheelEnc = 0f;
    private float backLeftWheelEnc = 0f;
    private float backRightWheelEnc = 0f;

    private float motorPower5;
    private float motorPower6;
    private float motorPower7;
    public float motorPower8;
    public float motorPower9;

    public float drivetrainGearRatio = 20f;
    public float encoderTicksPerRev = 28f;
    public float wheelSeparationWidth = 0.4f;
    public float wheelSeparationLength = 0.4f;
    public float wheelRadius = 0.0508f;
    public float motorRPM = 340.0f;

    public float rightTrigger;

    private Rigidbody rb;

    private Transform posiiton;

    private float previousRealTime;

    [Header("Subsystem Controls")]
    public GameObject arm;
    public GameObject intake;

    private ArmControl armControl;
    private IntakeControl intakeControl;

    private AudioManager audioManager;
    private RobotSoundControl robotSoundControl;

    private void Awake()
    {
        if(!intake)intake = transform.Find("Intake").gameObject;
        if(!arm)arm = transform.Find("slide mount:1").gameObject;

        controls = new PlayerControls();

        //Arm
        controls.GamePlay.ArmExtend.performed += ctx => motorPower6 = 1.0f;
        controls.GamePlay.ArmExtend.canceled += ctx => motorPower6 = 0.0f;
        controls.GamePlay.ArmDecend.performed += ctx => motorPower7 = 1.0f;
        controls.GamePlay.ArmDecend.canceled += ctx => motorPower7 = 0.0f;
        controls.GamePlay.ArmRotateForward.performed += ctx => motorPower8 = 1.0f;
        controls.GamePlay.ArmRotateForward.canceled += ctx => motorPower8 = 0.0f;
        controls.GamePlay.ArmRotateBackward.performed += ctx => motorPower9 = 1.0f;
        controls.GamePlay.ArmRotateBackward.canceled += ctx => motorPower9 = 0.0f;
        //Intake
        controls.GamePlay.Intake.performed += ctx => motorPower5 = 1f;
        controls.GamePlay.Intake.canceled += ctx => motorPower5 = 0.0f;

        //Driving Controls
        controls.GamePlay.DriveForward.started += ctx => usingJoystick = true;
        controls.GamePlay.DriveForward.performed += ctx => linearVelocityX = -1.5f*ctx.ReadValue<float>();
        controls.GamePlay.DriveForward.canceled += ctx => linearVelocityX = 0f;

        controls.GamePlay.DriveBack.started += ctx => usingJoystick = true;
        controls.GamePlay.DriveBack.performed += ctx => linearVelocityX = 1.5f*ctx.ReadValue<float>();
        controls.GamePlay.DriveBack.canceled += ctx => linearVelocityX = 0f;

        controls.GamePlay.DriveLeft.started += ctx => usingJoystick = true;
        controls.GamePlay.DriveLeft.performed += ctx => linearVelocityY = 1.5f*ctx.ReadValue<float>();
        controls.GamePlay.DriveLeft.canceled += ctx => linearVelocityY = 0f;

        controls.GamePlay.DriveRight.started += ctx => usingJoystick = true;
        controls.GamePlay.DriveRight.performed += ctx => linearVelocityY = -1.5f*ctx.ReadValue<float>();
        controls.GamePlay.DriveRight.canceled += ctx => linearVelocityY = 0f;

        controls.GamePlay.TurnLeft.started += ctx => usingJoystick = true;
        controls.GamePlay.TurnLeft.performed += ctx => angularVelocity = 6*ctx.ReadValue<float>();
        controls.GamePlay.TurnLeft.canceled += ctx => angularVelocity = 0f;

        controls.GamePlay.TurnRight.started += ctx => usingJoystick = true;
        controls.GamePlay.TurnRight.performed += ctx => angularVelocity = -6*ctx.ReadValue<float>();
        controls.GamePlay.TurnRight.canceled += ctx => angularVelocity = 0f;

        
    }

    private void OnEnable()
    {
        controls.GamePlay.Enable();
    }

    private void OnDisable()
    {
        controls.GamePlay.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        robotSoundControl = GetComponent<RobotSoundControl>();

        audioManager = GameObject.Find("ScoreKeeper").GetComponent<AudioManager>();

        previousRealTime = Time.realtimeSinceStartup;
        Console.WriteLine("Started.....");

        intakeControl = intake.GetComponent<IntakeControl>();
        intakeControl.Commands.Add(() => motorPower5 != 0, () =>
        {
            //robotSoundControl.playIntakeRev(motorPower5);
            intakeControl.setVelocity(motorPower5 * 150);
            intakeControl.deployIntake();
        });
        intakeControl.Commands.Add(() => motorPower5 == 0, () =>
        {
            //robotSoundControl.playIntakeRev(motorPower5);
            intakeControl.retractIntake();
        });

        armControl = arm.GetComponent<ArmControl>();
        armControl.Commands.Add(() => motorPower6 != 0, () =>
        {
            armControl.ExtendArm();
        });
        armControl.Commands.Add(() => motorPower7 != 0, () =>
        {
            armControl.DecendArm();
        });
        armControl.Commands.Add(() => motorPower8 != 0, () =>
        {
            armControl.RotateArmForward();
        });
        armControl.Commands.Add(() => motorPower9 != 0, () =>
        {
            armControl.RotateArmBackward();
        });
        
    }

    private void OnDestroy()
    {

    }

    private void driveRobot()
    {
        // Strafer Drivetrain Control
        if (!usingJoystick)
        {
            linearVelocityX = ((frontLeftWheelCmd + frontRightWheelCmd + backLeftWheelCmd + backRightWheelCmd) / 4) * ((motorRPM / 60) * 2 * wheelRadius * Mathf.PI);
            linearVelocityY = ((-frontLeftWheelCmd + frontRightWheelCmd + backLeftWheelCmd - backRightWheelCmd) / 4) * ((motorRPM / 60) * 2 * wheelRadius * Mathf.PI);
            angularVelocity = (((-frontLeftWheelCmd + frontRightWheelCmd - backLeftWheelCmd + backRightWheelCmd) / 3) * ((motorRPM / 60) * 2 * wheelRadius * Mathf.PI) / (Mathf.PI * wheelSeparationWidth)) * 2 * Mathf.PI;
        }
        // Apply Local Velocity to Rigid Body        
        var locVel = transform.InverseTransformDirection(rb.velocity);
        locVel.x = -linearVelocityY;
        locVel.y = 0f;
        locVel.z = -linearVelocityX;
        rb.velocity = transform.TransformDirection(locVel);
        //Apply Angular Velocity to Rigid Body
        rb.angularVelocity = new Vector3(0f, -angularVelocity, 0f);
        //Encoder Calculations 
        frontLeftWheelEnc += (motorRPM / 60) * frontLeftWheelCmd * Time.deltaTime * encoderTicksPerRev * drivetrainGearRatio;
        frontRightWheelEnc += (motorRPM / 60) * frontRightWheelCmd * Time.deltaTime * encoderTicksPerRev * drivetrainGearRatio;
        backLeftWheelEnc += (motorRPM / 60) * backLeftWheelCmd * Time.deltaTime * encoderTicksPerRev * drivetrainGearRatio;
        backRightWheelEnc += (motorRPM / 60) * backRightWheelCmd * Time.deltaTime * encoderTicksPerRev * drivetrainGearRatio;

        try
        {
            updateFrontRightEncoders(frontRightWheelEnc);
            updateFrontLeftEncoders(frontLeftWheelEnc);
            updateBackRightEncoders(backRightWheelEnc);
            updateBackLeftEncoders(backLeftWheelEnc);
        }
        catch
        {
            //print("Can not find javascript functions");
        }

        //robotSoundControl.playRobotDrive((Mathf.Abs(linearVelocityX) + Mathf.Abs(linearVelocityY) + Mathf.Abs(angularVelocity)) / 4f);
    }

    public void resetEncoders()
    {
        frontLeftWheelEnc = 0f;
        frontRightWheelEnc = 0f;
        backLeftWheelEnc = 0f;
        backRightWheelEnc = 0f;
    }

    public void setFrontLeftVel(float x)
    {
        usingJoystick = false;
        frontLeftWheelCmd = x;
    }

    public void setFrontRightVel(float x)
    {
        usingJoystick = false;
        frontRightWheelCmd = x;
    }

    public void setBackLeftVel(float x)
    {
        usingJoystick = false;
        backLeftWheelCmd = x;
    }

    public void setBackRightVel(float x)
    {
        usingJoystick = false;
        backRightWheelCmd = x;
    }

    public void setMotor5(float x)
    {
        usingJoystick = false;
        motorPower5 = x;
    }

    public void setMotor6(float x)
    {
        usingJoystick = false;
        motorPower6 = x;
    }

    public void setMotor7(float x)
    {
        usingJoystick = false;
        motorPower7 = x;
    }

    public void setMotor8(float x)
    {
        usingJoystick = false;
        motorPower8 = x;
    }

 

    private void Update()
    {
        if (!Photon.Pun.PhotonNetwork.IsConnected)
        {
            driveRobot();
           
            armControl.Commands.Process();
            intakeControl.Commands.Process();
            
        }
        else if (GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            driveRobot();
           
            armControl.Commands.Process();
            intakeControl.Commands.Process();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Floor")
        {
            //robotSoundControl.playRobotImpact();
        }
    }

    public void setStartPosition(Transform t)
    {
        posiiton = t;
    }

    public Transform getStartPosition()
    {
        return posiiton;
    }
}
