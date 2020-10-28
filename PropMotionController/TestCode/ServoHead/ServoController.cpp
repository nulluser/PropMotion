/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#define MODULE_SERVOC

#include <Adafruit_PWMServoDriver.h>

#include "Utility.h"
#include "ServoController.h"

Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();


ServoController::ServoController( char * _name, float _home_pos, int _pwm_chan, int _pwm_min, int _pwm_max)
{
  strcpy(name, _name);
  
  mode = MODE_NONE;

  state = STATE_WAITING;
  state_t = 0;

  home_pos = _home_pos;

  pwm_chan = _pwm_chan;
  pwm_min = _pwm_min;
  pwm_max = _pwm_max;

  test_pos = CENTER;

  target = 0;
  cmd = 0;

}


void ServoController::init(void )
{
  Serial.print(name);
  Serial.println(" Init");

  // Center
  //pwm.setPWM(pwm_chan, 0, (pwm_min + pwm_max) / 2);

  
}




void ServoController::set_interval(float _rate, float _prob)
{
  Serial.print(name);
  Serial.println(" Set Interval");
  
  mode = MODE_INTERVAL;
  rate = _rate;
  prob = _prob;
   
}



void ServoController::set_target(float _rate, float _prob)
{
  Serial.print(name);
  Serial.println(" Set Target");
  
  mode = MODE_TARGET;
  rate = _rate;
  prob = _prob;
  
}


void ServoController::set_home(void)
{
  Serial.print(name);
  Serial.println(" Set Home");

  // Half rate for homing
  rate /= 2.0;
  target = home_pos;
  mode = MODE_HOME;
}

void ServoController::set_test(void)
{
  Serial.print(name);
  Serial.println(" Set Test");

  mode = MODE_TEST;
}


void ServoController::set_none(void)
{
  mode = MODE_NONE;
}


float ServoController::get_cmd() { return cmd; };


void ServoController::update(float dt)
{

  if (mode == MODE_NONE)
  {
    float val = 0.0;
    int pos = mapf(val, -1.0, 1.0, pwm_min, pwm_max);
    pwm.setPWM(pwm_chan, 0, pos);
  } else
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
        if (home_pos == 1) val = home_pos * cos(state_t)/2 + 0.5; else
        if (home_pos == -1) val = home_pos * cos(state_t)/2 - 0.5; else
        if (home_pos == 0) val =  sin(state_t)/2; 
        
      }
      else
      {
        if (home_pos == 1 || home_pos == -1)  val = home_pos * cos(state_t); else
        if (home_pos == 0) val = sin(state_t);
      }
      
      state_t += rate * dt;
  
      if (state_t > 2*M_PI)
      {
        state_t = 0;
        state = STATE_WAITING;
      }
    }
  
    //pos = mapf(val, -1.0, 1.0, pwm_min, pwm_max);
    cmd = val;

       int pos = mapf(val, -1.0, 1.0, pwm_min, pwm_max);
   pwm.setPWM(pwm_chan, 0, pos);
  }
  else
  if (mode == MODE_TARGET || mode == MODE_HOME)
  {
    float move_rate = 0;
    if (mode == MODE_TARGET)
    {
      if ((rand()/(float)(RAND_MAX)) < prob * dt)
      {
        target = 2*(rand() / (float)RAND_MAX) - 1;
      }
      move_rate = rate;
    }

    if (mode == MODE_HOME)
    {
     move_rate = 0.998;
    }
    
    
    // run once for each ms
    for (int i = 0; i < dt * 1000; i++)
      cmd = move_rate * cmd + (1-move_rate) * target;
    
    int pos = mapf(cmd, -1.0, 1.0, pwm_min, pwm_max);
    pwm.setPWM(pwm_chan, 0, pos);
   
  } else
  if (mode == MODE_TEST)
  {
    static float test_pos = CENTER;
  
    // Manual control
    if (Serial.available())
    {
      char ch = Serial.read();
  
      if (ch == 'q') test_pos--;
      if (ch == 'w') test_pos++;
      
      if (ch == 'a') test_pos-=10;
      if (ch == 's') test_pos+=10;
      
      Serial.print(" Pos: ");
      Serial.println(test_pos);
    }

    pwm.setPWM(pwm_chan, 0, test_pos);
  }
}


void servo_init()
{
  Serial.println("Servo init");
  
  Serial.print("Freq: ");
  Serial.println(SERVO_FREQ);

 pwm.begin();
   
  pwm.setOscillatorFrequency(27000000);
  
  pwm.setPWMFreq(SERVO_FREQ); 

  //delay(10);
}
