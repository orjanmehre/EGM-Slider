### EGM_Slider
Simulates a sensor for use with EGM on ABB robot

The program makes a square in the xy-plane that it sends over to a ABB-robot using UdPUc.
It also contains a slider that enables the user to adjust the height of the robot in the path. 

This program is made for testing the concept of EGM.

The code is built on the example code provided by ABB. 

In RAPID there is a waiting program, the robot stand still in one point and wait for data from the sensor.

_____________________________________________________________________________________________________________________________
####The RAPID code 

```rapid

MODULE EGM_test_UDP
    
    VAR egmident egmID1;
    VAR egmstate egmSt1;
    
    CONST egm_minmax egm_minmax_lin1:=[-1,1]; !in mm
    CONST egm_minmax egm_minmax_rot1:=[-2,2];! in degees
    CONST robtarget p20:=[[34,-499,123],[0.000494947,0.662278052,-0.749217059,-0.007831731],[0,0,-1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata UISpenholder:=[TRUE,[[0,0,114.25],[1,0,0,0]],[1,[-0.095984607,0.082520613,38.69176324],[1,0,0,0],0,0,0]];
    TASK PERS wobjdata wobjBordN:=[FALSE,TRUE,"",[[0,0,0],[1,0,0,0]],[[150,-500,8],[0.707106781,0,0,-0.707106781]]];
    
    VAR pose posecorTable:=[[150,-500,8],[1,0,0,0]];
    VAR pose posesenTable:=[[0,0,0],[1,0,0,0]];
  
    PROC main()
        ! Move to start position. Fine point is demanded.
        MoveJ p20,v100,fine,UISpenholder;
        testuc_UDP; 
    ENDPROC
  
    
    PROC testuc_UDP()
        EGMReset egmID1;
        EGMGetId egmID1;
        egmSt1 := EGMGetState(egmID1);
        TPWrite "EGM state: "\Num := egmSt1;
        
        IF egmSt1 <= EGM_STATE_CONNECTED THEN
            ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
            EGMSetupUC ROB_1, egmID1, "default", "EGMSensor:" \pose;
        ENDIF
        
        !Which program to run
        runEGM;

        IF egmSt1 = EGM_STATE_CONNECTED THEN
            TPWrite "Reset EGM instance egmID1";
            EGMReset egmID1;
        ENDIF
    ENDPROC
        
        
        PROC runEGM()
            EGMActPose egmID1\Tool:=UISpenholder \WObj:=wobj0, posecorTable,EGM_FRAME_WOBJ, posesenTable, EGM_FRAME_WOBJ 
            \x:=egm_minmax_lin1 \y:=egm_minmax_lin1 \z:=egm_minmax_lin1
            \rx:=egm_minmax_rot1 \ry:=egm_minmax_rot1 \rz:=egm_minmax_rot1\LpFilter:=3\Samplerate:=4\MaxSpeedDeviation:= 1000;
                
            EGMRunPose egmID1, EGM_STOP_RAMP_DOWN\x \y \z\CondTime:=20 \RampInTime:=0.05\RampOutTime:=0.5;
            egmSt1:=EGMGetState(egmID1);
            
        ENDPROC
    ENDMODULE
 ```
