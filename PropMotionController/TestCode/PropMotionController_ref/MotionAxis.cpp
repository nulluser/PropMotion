/*
  Prop Controller
  
 (C) 2020 nulluser@gmail.com


  File: MotionAxis.cpp
*/

//#define DEBUG

#include <arduino.h>

#include "Utility.h"
#include "MotionAxis.h"
#include "MotionGenerator.h"

#include <stdint.h>

//MotionAxis::MotionAxis(uint16_t _min_pwm, uint16_t _max_pwm, float _home_pos,
//                       float max_vel, float max_accl, uint8_t _pwm_index)

//MotionAxis::MotionAxis(AxisConfig axis_config)
MotionAxis::MotionAxis(AxisConfig _axis_config) : axis_config(_axis_config)
{


  
  //min_pwm = axis_config.min_pwm;
  //max_pwm = axis_config.max_pwm;
  //home_pos = axis_config.home_pos;
  
  target = axis_config.home_pos;

  //profile.setMaxVelocity(axis_config.max_vel);
  //profile.setMaxAcceleration(axis_config.max_accl);
  //profile.setInitPosition(home_pos);   

  profile.setParameter(axis_config.max_vel, axis_config.max_accl, axis_config.home_pos);

  //profile = new MotionGenerator(max_vel, max_accl, home_pos);
}






void MotionAxis::update()
{
  float pos =  profile.update(target);

  #ifdef DEBUG
  Serial.print("Min:0,Max:255,");
  //Serial.print("T:"); Serial.print(target, 2); Serial.print(",");
  Serial.print("C:"); Serial.print(pos, 2); 
  Serial.println();
  #endif

  
  // Set pwm
  //uint8_t pwm_cmd = mapf(pos, 0, 255, axis_config.min_pwm, axis_config.max_pwm);
  // pwm.write(axis_config.pwm_index,  command_pos);
  
}


void MotionAxis::set_target(float _target)
{
  target = _target;
 
}


void MotionAxis::home()
{
  target = axis_config.home_pos;
 
}
