/*
  Prop Controller
  
 (C) 2020 nulluser@gmail.com

  File: Main.cpp
*/



#define MODULE_PROPCTRL

#include "Config.h"
#include "PropController.h"
#include "MotionGenerator.h"
#include "MotionAxis.h"

//#define SERIAL_BAUD 250000


// 318 bytes




const AxisConfig axis_config[] = { {min_pwm : 250,
                                  max_pwm : 350, 
                                  home_pos : 128,
                                  max_vel : 200,
                                  max_accl : 500,
                                  pwm_index : 0
                                 } } ;



int updates = 0;

//MotionAxis axis_1(axis_config[0]);//axis_1_min, axis_1_max, axis_1_home, axis_1_vel, axis_1_accl, axis_1_chan);


MotionAxis axis_list[] = {MotionAxis(axis_config[0])};





uint8_t num_axis = sizeof(axis_list) / sizeof(axis_list[0]);




void propcontroller_init()
{

  Serial.print("Size: ");
  Serial.println(sizeof(axis_list[0]));


  //for (int i = 0; i < num_axis; i++)
//    axis_list[i]->init();
}


void propcontroller_update()
{
  update_motion();
  update_stats();
}




 
void update_motion(void)
{
  static uint16_t count = 0;
  
  if (++count > 2000)
  {
      if (axis_list[0].get_target() < 128) axis_list[0].set_target(255); else axis_list[0].set_target(0); 
      count = 0;      
   }


  for (int i = 0; i < num_axis; i++)
    axis_list[i].update();

//  axis_1.update();
  updates++;
  
}



void update_stats()
{
  static unsigned long last_time = 0;
  unsigned long cur_time = millis();

  if (cur_time - last_time > 1000)
  {
    last_time = cur_time;
    
    Serial.print("Rate: ");
    Serial.println(updates);

    updates = 0;
    
    
    
  }

}
