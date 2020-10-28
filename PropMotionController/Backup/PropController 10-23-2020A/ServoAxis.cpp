/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#define MODULE_SERVOAXIS


#include "Utility.h"
#include "ServoAxis.h"


ServoAxis::ServoAxis( bool _enabled, float _home_pos, uint8_t _pwm_chan, 
                    uint16_t _pwm_min, uint16_t _pwm_max, float _max_vel, float _max_acc)
{
  enabled = _enabled;
  home_pos = _home_pos;
  pwm_chan = _pwm_chan;
  pwm_min = _pwm_min;
  pwm_max = _pwm_max;
  max_vel = _max_vel;
  max_acc = _max_acc;
  
  mode = MODE_HOLD;

  state = STATE_WAITING;
  state_t = 0;
  
  target = home_pos;
  cmd = home_pos;

  profile.setParameter(max_vel, max_acc, home_pos);
  profile.update(home_pos);

  pwm_pos = map_pwm(home_pos);
}


void ServoAxis::init(void )
{
  //Serial.print(name);
  //Serial.println(" Init");

  // Center
  //pwm.setPWM(pwm_chan, 0, (pwm_min + pwm_max) / 2);

  
}




void ServoAxis::set_interval(float _rate, float _prob)
{
  //Serial.print(name);
  //Serial.println(" Set Interval");
  
  mode = MODE_INTERVAL;
  rate = _rate;
  prob = _prob;
   
}



void ServoAxis::set_rand(float _rate, float _prob)
{
  //Serial.print(name);
  //Serial.println(" Set Target");
  
  mode = MODE_RAND;
  rate = _rate;
  prob = _prob;
  
}


void ServoAxis::set_target_pos(uint8_t _target)
{
  target = _target;
  mode = MODE_HOLD;
}


void ServoAxis::set_home(void)
{
  //Serial.print(name);
  //Serial.println(" Set Home");


   target = home_pos;
}





void ServoAxis::set_hold(void)
{
  mode = MODE_HOLD;
}


float ServoAxis::get_cmd() { return cmd; };


void ServoAxis::update(float dt)
{

  // Disable output
  if (enabled == false)
  {
      pwm_pos = 0;
      //pwm.setPWM(pwm_chan, 0, 0);
      return;
  }
 
  if (mode == MODE_INTERVAL)
  {
    float val = home_pos;
  
    if (state == STATE_WAITING)
    {
      if ((rand()/(float)(RAND_MAX)) < prob * dt)
      {
        state_t = 0;
        state = STATE_MOVING;
        half_travel = rand() % 2;
        
      }
      
    }else
    if (state == STATE_MOVING)
    {
      
      // Only move half way    
      if (half_travel)
      {
        if (home_pos == 0)  val = 0.5*255*sin(state_t/2.0);// + 127
        if (home_pos == 127) val = 0.5*127*sin(state_t) + 127;
        if (home_pos == 255)  val = 255 - 0.5*255*sin(state_t/2.0);
      }
      else
      {
        if (home_pos == 0)  val = 255*sin(state_t/2.0);// + 127
        if (home_pos == 127) val = 127*sin(state_t) + 127;
        if (home_pos == 255)  val = 255 - 255*sin(state_t/2.0);
      }
      
      state_t += rate * dt;
  
      if (state_t > 2*M_PI)
      {
        state_t = 0;
        state = STATE_WAITING;
      }
    }
  
    //pos = mapf(val, -1.0, 1.0, pwm_min, pwm_max);
   // cmd = val;
   target = val;
   

       //int pos = mapf(val, -1.0, 1.0, pwm_min, pwm_max);
   //pwm.setPWM(pwm_chan, 0, pos);
  }
  else
  if (mode == MODE_RAND)
  {
    float move_rate = 0;
    
    
    
      if ((rand()/(float)(RAND_MAX)) < prob * dt)
      {
        target = rand() % 255;
      }
      
    

   
  } 


  if (target < 0) target = 0;
  if (target > 255) target = 255;

  cmd = profile.update(target);

  pwm_pos = map_pwm(cmd);
}


uint16_t ServoAxis::map_pwm(float pos_cmd)
{
  return mapf(pos_cmd, 0, 255, pwm_min, pwm_max);
}
