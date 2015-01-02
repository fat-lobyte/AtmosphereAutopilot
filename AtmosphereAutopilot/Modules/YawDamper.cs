﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AtmosphereAutopilot
{
    /// <summary>
    /// Simple yaw damper on PID
    /// </summary>
    class YawDamper: PIDAngularVelDampener
    {
        public YawDamper(Vessel cur_vessel)
            : base(cur_vessel, "Yaw dampener", 752348) 
        {
            pid = new PIDController();
            pid.KP = 1.0;
            pid.KI = 0.0;
            pid.IntegralClamp = 1.0;
            pid.KD = 0.01;
        }

        double time = 0.0;

        protected override void apply_module(FlightCtrlState cntrl)
        {
            // vector to right wing
            angular_velocity = -currentVessel.angularVelocity.z;
            time = time + TimeWarp.fixedDeltaTime;
            output = pid.Control(angular_velocity, 0.0, time);
            
            // check if user is inputing control
            if (cntrl.killRot)                          // when sas works just back off
                return;
            if (cntrl.yaw == cntrl.yawTrim)             // when user doesn't use control, pitch is on the same level as trim
            {
                cntrl.yaw = (float)Common.Clamp(output, 1.0);
            }
            else
                pid.clear();
            if (currentVessel.checkLanded())
                pid.clear();
        }
    }
}
