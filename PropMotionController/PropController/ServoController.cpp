/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#define MODULE_SERVOCTRL

//#include <Adafruit_PWMServoDriver.h>
#include "PWMServoDriver.h"

#include "Config.h"
#include "Utility.h"
#include "ServoController.h"
#include "ServoAxis.h"

/*
ServoAxis eye_pan_axis   (ENABLED,  127, EYE_PAN_CHAN,   EYE_PAN_MIN,   EYE_PAN_MAX, EYE_PAN_MAX_VEL, EYE_PAN_MAX_ACC);
ServoAxis eye_tilt_axis  (ENABLED,  127, EYE_TILT_CHAN,  EYE_TILT_MIN,  EYE_TILT_MAX, EYE_TILT_MAX_VEL, EYE_TILT_MAX_ACC);
ServoAxis eyelid_axis    (ENABLED,    0, EYELID_CHAN,    EYELID_MIN,    EYELID_MAX, EYELID_MAX_VEL, EYELID_MAX_ACC);
ServoAxis nose_axis      (ENABLED,  255, NOSE_CHAN,      NOSE_MIN,      NOSE_MAX, NOSE_MAX_VEL, NOSE_MAX_ACC);
ServoAxis brain_axis     (ENABLED,  127, BRAIN_CHAN,     BRAIN_MIN,     BRAIN_MAX, BRAIN_MAX_VEL, BRAIN_MAX_ACC);
ServoAxis mouth_axis     (ENABLED,    0, MOUTH_CHAN,     MOUTH_MIN,     MOUTH_MAX, MOUTH_MAX_VEL, MOUTH_MAX_ACC);
ServoAxis neck_pan_axis  (ENABLED,  127, NECK_PAN_CHAN,  NECK_PAN_MIN,  NECK_PAN_MAX, NECK_PAN_MAX_VEL, NECK_PAN_MAX_ACC);
ServoAxis neck_tilt_axis (ENABLED,  127, NECK_TILT_CHAN, NECK_TILT_MIN, NECK_TILT_MAX, NECK_TILT_MAX_VEL, NECK_TILT_MAX_ACC);
ServoAxis test_axis      (DISABLED, 127, TEST_CHAN,      TEST_MIN,      TEST_MAX, TEST_MAX_VEL, TEST_MAX_ACC);
*/



ServoAxis eye_pan_axis   (ENABLED,  127, EYE_PAN_CHAN,   EYE_PAN_MIN,   EYE_PAN_MAX, EYE_PAN_MAX_VEL, EYE_PAN_MAX_ACC);
ServoAxis eye_tilt_axis  (ENABLED,  127, EYE_TILT_CHAN,  EYE_TILT_MIN,  EYE_TILT_MAX, EYE_TILT_MAX_VEL, EYE_TILT_MAX_ACC);
ServoAxis eyelid_axis    (ENABLED,    0, EYELID_CHAN,    EYELID_MIN,    EYELID_MAX, EYELID_MAX_VEL, EYELID_MAX_ACC);
ServoAxis nose_axis      (ENABLED,  255, NOSE_CHAN,      NOSE_MIN,      NOSE_MAX, NOSE_MAX_VEL, NOSE_MAX_ACC);
ServoAxis brain_axis     (ENABLED,  127, BRAIN_CHAN,     BRAIN_MIN,     BRAIN_MAX, BRAIN_MAX_VEL, BRAIN_MAX_ACC);
ServoAxis mouth_axis     (ENABLED,    0, MOUTH_CHAN,     MOUTH_MIN,     MOUTH_MAX, MOUTH_MAX_VEL, MOUTH_MAX_ACC);
ServoAxis neck_pan_axis  (ENABLED,  127, NECK_PAN_CHAN,  NECK_PAN_MIN,  NECK_PAN_MAX, NECK_PAN_MAX_VEL, NECK_PAN_MAX_ACC);
ServoAxis neck_tilt_axis (ENABLED,  127, NECK_TILT_CHAN, NECK_TILT_MIN, NECK_TILT_MAX, NECK_TILT_MAX_VEL, NECK_TILT_MAX_ACC);
ServoAxis test_axis      (ENABLED,  127, TEST_CHAN,      TEST_MIN,      TEST_MAX, TEST_MAX_VEL, TEST_MAX_ACC);


// Setup Axis list
ServoAxis *axis_list[] = 
  {
    &eye_pan_axis, &eye_tilt_axis, 
    &eyelid_axis, &nose_axis,
    &brain_axis, &mouth_axis, 
    &neck_pan_axis, &neck_tilt_axis,
    &test_axis 
  };

#define NUM_AXIS (sizeof(axis_list) / sizeof(axis_list[0]))

// Default constructor
ServoController::ServoController()
{
}

// Init 
void ServoController::init(void )
{
  //Serial.print(name);
  //Serial.println(" Init");
  //Serial.print(" axis");
  //Serial.println(NUM_AXIS);
  

  pwm.begin();

  // Disable all
  for (int i = 0; i < 16; i++)
    pwm.setPWM(i, 0, 0);
    
  pwm.setOscillatorFrequency(27000000);
  
  pwm.setPWMFreq(SERVO_FREQ); 

  delay(10);

  // init all axis
  for (int i = 0; i < NUM_AXIS; i++)
    axis_list[i]->init();
}

// Update all axis
void ServoController::update(float dt)
{
  uint16_t chan_data[NUM_AXIS];
  
  for (int i = 0; i < NUM_AXIS; i++)
  {
    axis_list[i]->update(dt);

    //if (i == 8) {Serial.print("Pos: "); Serial.println(axis_list[i]->get_pwm_pos());}
    chan_data[axis_list[i]->get_pwm_chan()] = axis_list[i]->get_pwm_pos();
    
    //pwm.setPWM(axis_list[i]->get_pwm_chan(), 0, axis_list[i]->get_pwm_pos());
  }

  pwm.setPWMArray(chan_data, NUM_AXIS);
}


// External target command
void ServoController::set_servo_target(uint8_t index, uint8_t target)
{
   axis_list[index]->set_target_pos(target);
}

// Home all axis
void ServoController::home_axis()
{
  for (int i = 0; i < NUM_AXIS; i++)
    axis_list[i]->set_home();
}

// Hold mode for all axis
void ServoController::set_hold()
{
  for (int i = 0; i < NUM_AXIS; i++)
    axis_list[i]->set_hold();
}


// Rand more for all axis
void ServoController::set_rand()
{
  eye_pan_axis.set_rand    (EYE_PAN_RATE, EYE_PAN_PROB);
  eye_tilt_axis.set_rand   (EYE_TILT_RATE, EYE_TILT_PROB);
  eyelid_axis.set_interval (EYELID_RATE, EYELID_PROB);
  nose_axis.set_interval   (NOSE_RATE, NOSE_PROB);
  brain_axis.set_interval  (BRAIN_RATE, BRAIN_PROB);
  mouth_axis.set_interval  (MOUTH_RATE, MOUTH_PROB);
  neck_pan_axis.set_rand   (NECK_PAN_RATE, NECK_PAN_PROB);
  neck_tilt_axis.set_rand  (NECK_TILT_RATE, NECK_TILT_PROB);
  test_axis.set_rand       (NECK_PAN_RATE, NECK_PAN_PROB);
}

// Return axis count
uint8_t ServoController::get_num_axis()
{
  return NUM_AXIS;
}
