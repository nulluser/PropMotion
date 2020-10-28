/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/
#include "Config.h"
#include "PropController.h"
#include "LightController.h"
#include "ServoController.h"

// Serial States
#define SS_WAIT1      0
#define SS_WAIT2      1
#define SS_WAIT3      2
#define SS_WAIT4      3
#define SS_DATA       4

LightController light_ctrl;
ServoController servo_ctrl;

bool homing_mode = false;
unsigned long last_vixen_update = 0;

uint8_t mode = RUN_MODE;

// Serial data
static uint8_t serial_state = SS_WAIT1;
static uint8_t rx_buffer[NUM_ELEMENTS];

void setup()
{
  io_init();

  light_ctrl.init();
  servo_ctrl.init();

  if (mode == MODE_RAND) servo_ctrl.set_rand();
  if (mode == MODE_VIXEN) servo_ctrl.set_hold();

  last_vixen_update = millis();
}


void loop() 
{
  check_serial();
  
  // Stats
  static int count = 0;
  static unsigned long last_update = millis();
  static unsigned long prev = millis();
  unsigned long current = millis();
  float dt = (current - prev) / 1000.0;

  if (current - last_update > 1000)
  {
    //Serial.print("Update rate: ");
    //Serial.print(count);
    //Serial.print(" dt: ");
    //Serial.println(dt, 6);
    //Serial.println();
    last_update = current;
    count = 0;
  }

  prev = current;

  // Core udpate
  update_io();
  
  servo_ctrl.update(dt);
  
  // Do not auto update in vixen mode
  if (mode == MODE_RAND && homing_mode == false)
    light_ctrl.update();
    
  count++;


  if (mode == MODE_VIXEN && millis() - last_vixen_update > VIXEN_TIMEOUT) 
  {
    servo_ctrl.home_axis();
  
    //servo_ctrl.set_mode_rand();

    mode = MODE_NONE;
      
    light_ctrl.setall(255, 0, 0);
  }
 
  
}




void io_init()
{
  Serial.begin(SERIAL_BAUD);
  delay(2500);
  
  //Serial.println("Start");
  //Serial.println("Num axis: ");
  //Serial.println(NUM_AXIS);  

  pinMode(LED, OUTPUT);

  // Setup IO
  pinMode(HOME_CMD, INPUT_PULLUP);
  pinMode(POWER_ON , OUTPUT);

  // Turn on power
  digitalWrite(POWER_ON, 1);

  delay(2000);
}

// Get IO. Implements homing state machine
void update_io()
{
  // Homing Modes
  #define HOME_IDLE  0
  #define HOME_WAIT  1
  #define HOME_HOMING  2

  static uint8_t mode = false;
  static unsigned long home_start = 0;
     
  bool home_active = !digitalRead(HOME_CMD);

  //Serial.print("H: ");
  //Serial.println(home_active);
  
  // Waiting for switch
  if (mode == HOME_IDLE)
  {
    if (home_active)
    {
      //Serial.println("Home Requested");
      home_start = millis();
      mode = HOME_WAIT;
    }
  }

  // Waiting for dwell time to complete
  if (mode == HOME_WAIT)
  {
    //See if switch is still time and bounce period passed
    if (home_active)
    {
      if (millis() - home_start > HOME_BOUNCE)
      {
        homing_mode = true;
        //Serial.println("Home Started");
        home_start = millis();
        mode = HOME_HOMING;

        servo_ctrl.set_hold();
        servo_ctrl.home_axis();

        light_ctrl.setall(0, 255, 0);
        
      }
    }
    else
    {
      //Serial.println("Home Aborted");
      mode = HOME_IDLE;
    }
  }


  if (mode == HOME_HOMING)
  {
    if (millis() - home_start > HOME_TIME)
    {
      //Serial.println("Shutting Down");
      delay(1000);
      digitalWrite(POWER_ON, 0);
      while(1); 
    }
  }


}







void process_buffer()
{
  servo_ctrl.set_hold();
  mode = MODE_VIXEN;

  digitalWrite(LED, !digitalRead(LED));
    
  uint8_t index = 0;

  for (uint8_t i = 0; i < NUM_LEDS; i++)
  {
    uint8_t r = rx_buffer[index++];
    uint8_t g = rx_buffer[index++];
    uint8_t b = rx_buffer[index++];

    //leds[i].setRGB( r, g, b);

    light_ctrl.setled(i, r, g, b);
    
    //pixels.setPixelColor(i, r, g, b); 
  }

  light_ctrl.show();


  
//  pixels.show();  
  
  // Ignore test axis
  for (uint8_t i = 0; i < servo_ctrl.get_num_axis()-1; i++)
  {
    uint8_t v = rx_buffer[index++];
  
    if (v != 0)
    {
      servo_ctrl.set_servo_target(i, v);
    }
      //axis_list[i]->set_target_pos(v); 
  }



  last_vixen_update = millis();
}


void process_serial(uint8_t c)
{
  static uint8_t rx_index = 0;
  
  if (serial_state == SS_WAIT1)
  {
    if (c == '<') 
    {
      serial_state = SS_WAIT2; 
      rx_index = 0;
    }
  } else 

  if (serial_state == SS_WAIT2)
  {
    if (c == '!') serial_state = SS_WAIT3; else serial_state = SS_WAIT1;
  } else

  if (serial_state == SS_WAIT3)
  {
    if (c == '*') serial_state = SS_WAIT4; else serial_state = SS_WAIT1;
  } else
  if (serial_state == SS_WAIT4)
  {
    if (c == '$') serial_state = SS_DATA; else serial_state = SS_WAIT1;
  } else  
  if (serial_state == SS_DATA)
  {
    rx_buffer[rx_index++] = c;

    if (rx_index >= NUM_ELEMENTS)
    {
      process_buffer();
      serial_state = SS_WAIT1;
    }

  }


  
}



void check_serial()
{

  while (Serial.available())
  {
    uint8_t ch = Serial.read();
    process_serial(ch);
  }
  
}
