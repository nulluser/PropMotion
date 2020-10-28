/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/


#ifndef SERVOCTRL_C
#define SERVOCTRL_C

#include <Adafruit_PWMServoDriver.h>

#include "MotionGenerator.h"

#define DISABLED 0
#define ENABLED  1

#ifdef MODULE_SERVOCTRL

// Servos
#define SERVO_FREQ 50 // Analog servos run at ~50 Hz updates

// States
#define STATE_WAITING  0
#define STATE_MOVING  1

#endif


// Server Controller
class ServoController
{
  public:

    ServoController( );
    void init(void);
    void update(float dt);

    void set_servo_target(uint8_t index, uint8_t target);
    void home_axis();

    uint8_t get_num_axis();

    void set_hold();
    void set_rand();
  
  private:


  Adafruit_PWMServoDriver pwm;// = Adafruit_PWMServoDriver();
 
};


#endif
