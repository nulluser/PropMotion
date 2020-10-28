
#ifndef MOTIONAXIS_H
#define MOTIONAXIS_H

#include <stdint.h>

#include "MotionGenerator.h"

struct AxisConfig
{
  uint16_t min_pwm ;
  uint16_t max_pwm ;
  float home_pos ;
  float max_vel ;
  float max_accl ;
  uint8_t pwm_index;
};


class MotionAxis
{
  public:

    MotionAxis(AxisConfig axis_config);

    void update();

    void set_target(float _target);
    float get_target(void) { return target; };
    void home();
   
  private:

    const AxisConfig axis_config;
    
    MotionGenerator profile;// Motion profile

    float target;
    
};



#endif
