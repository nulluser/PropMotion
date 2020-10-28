/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#include <Adafruit_NeoPixel.h>

#include "ServoController.h"
#include "config.h"

#define HOME_CMD     6
#define POWER_ON     7
#define LED          LED_BUILTIN
#define EYE_LED_PIN  4

// Main loop delay
#define DELAYVAL     1 

// Total of pixels
#define NUMPIXELS    24

#define HOME_BOUNCE  1000
#define HOME_TIME    5000



// RGB Chain Assignments
// LEDS are 40mA each full white. 440mA total for 11 LEDS
#define LINK1_LED_IDX       0
#define LINK2_LED_IDX       1

#define LEDBAR1_LED_IDX       2
#define LEDBAR2_LED_IDX       3
#define LEDBAR3_LED_IDX       4
#define LEDBAR4_LED_IDX       5
#define LEDBAR5_LED_IDX       6
#define LEDBAR6_LED_IDX       7
#define LEDBAR7_LED_IDX       8


#define TUBE1_LED_IDX       2
#define TUBE2_LED_IDX       3
#define TUBE3_LED_IDX       4
#define TUBE4_LED_IDX       5

#define REACTOR1_LED_IDX    6
#define REACTOR2_LED_IDX    7
#define REACTOR3_LED_IDX    8
#define REACTOR4_LED_IDX    9

#define EYE_LED_IDX         9 // 10



Adafruit_NeoPixel pixels(NUMPIXELS, EYE_LED_PIN, NEO_GRB + NEO_KHZ800);

ServoController eye_pan_axis   ("EyeP",  0, EYE_PAN_CHAN,   EYE_PAN_MIN,   EYE_PAN_MAX);
ServoController eye_tilt_axis  ("EyeT",  0, EYE_TILT_CHAN,  EYE_TILT_MIN,  EYE_TILT_MAX);
ServoController eyelid_axis    ("EyeL", -1, EYELID_CHAN,    EYELID_MIN,    EYELID_MAX);
ServoController nose_axis      ("Nose",  1, NOSE_CHAN,      NOSE_MIN,      NOSE_MAX);
ServoController brain_axis     ("Brne",  0, BRAIN_CHAN,     BRAIN_MIN,     BRAIN_MAX);
ServoController mouth_axis     ("Mout", -1, MOUTH_CHAN,     MOUTH_MIN,     MOUTH_MAX);
ServoController neck_pan_axis  ("NckP",  0, NECK_PAN_CHAN,  NECK_PAN_MIN,  NECK_PAN_MAX);
ServoController neck_tilt_axis ("NckT",  0, NECK_TILT_CHAN, NECK_TILT_MIN, NECK_TILT_MAX);
ServoController test_axis      ("Test",  0, TEST_CHAN,      TEST_MIN,      TEST_MAX);

// Setup Axis list
ServoController *axis_list[] = 
  {
    &eye_pan_axis, &eye_tilt_axis, 
    &eyelid_axis, &nose_axis,
    &brain_axis , &mouth_axis, 
    &neck_pan_axis, &neck_tilt_axis,
    &test_axis 
  };

#define NUM_AXIS (sizeof(axis_list) / sizeof(axis_list[0]))


bool homeing_mode = false;


void setup()
{
  Serial.begin(250000);
  Serial.println("Start");

  Serial.println("Num axis: ");
  Serial.println(NUM_AXIS);  

  // Setup IO
  pinMode(HOME_CMD, INPUT_PULLUP);
  pinMode(POWER_ON , OUTPUT);

  // Turn on power
  digitalWrite(POWER_ON, 1);

  pixels.begin(); 
  pixels.clear(); // Set all pixel colors to 'off'
  
  pinMode(LED, OUTPUT);

  servo_init();

  // init all
  for (int i = 0; i < NUM_AXIS; i++)
    axis_list[i]->init();

  eye_pan_axis.set_target   (EYE_PAN_RATE, EYE_PAN_PROB);
  eye_tilt_axis.set_target  (EYE_TILT_RATE, EYE_TILT_PROB);
  eyelid_axis.set_interval  (EYELID_RATE, EYELID_PROB);
  nose_axis.set_interval    (NOSE_RATE, NOSE_PROB);
  brain_axis.set_interval   (BRAIN_RATE, BRAIN_PROB);
  mouth_axis.set_interval   (MOUTH_RATE, MOUTH_PROB);
  neck_pan_axis.set_target  (NECK_PAN_RATE, NECK_PAN_PROB);
  neck_tilt_axis.set_target (NECK_TILT_RATE, NECK_TILT_PROB);
  test_axis.set_test();

  // Disable
  //eye_pan_axis.set_none();
  //eye_tilt_axis.set_none();
  //nose_axis.set_none();
  //brain_axis.set_none();
  //eyelid_axis.set_none();
  //mouth_axis.set_none();
  //neck_pan_axis.set_none();  
  //neck_tilt_axis.set_none();
  //test_axis.set_none();
}

void loop() 
{
  // Stats
  static int count = 0;
  static unsigned long last_update = 0;
  static unsigned long prev = 0;
  unsigned long current = millis();
  float dt = (current - prev) / 1000.0;

  if (current - last_update > 1000)
  {
    Serial.print("Update rate: ");
    Serial.print(count);
    Serial.print(" dt: ");
    Serial.println(dt, 6);
    last_update = current;
    count = 0;
  }

  prev = current;

  // Core udpate
  update_io();
  update_servos(dt);
  update_leds();

  count++;

  delay(DELAYVAL); // Pause before next pass through loop
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

  Serial.print("H: ");
  Serial.println(home_active);
  

  // Waiting for switch
  if (mode == HOME_IDLE)
  {
    if (home_active)
    {
      Serial.println("Home Requested");
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
        homeing_mode = true;
        Serial.println("Home Started");
        home_start = millis();
        mode = HOME_HOMING;

        for (int i = 0; i < NUM_AXIS; i++)
          axis_list[i]->set_home();
      }
    }
    else
    {
      Serial.println("Home Aborted");
      mode = HOME_IDLE;
    }
  }


  if (mode == HOME_HOMING)
  {
    if (millis() - home_start > HOME_TIME)
    {
      Serial.println("Shutting Down");
      delay(1000);
      digitalWrite(POWER_ON, 0);
      while(1); 
    }
  }


}




// LEDs inside tubes
void update_ledbar_leds()
{

  static float r_target = 0;
  static float g_target = 0;
  static float b_target = 0;
   
  static int count2 = 0;
  if (++count2 > 20)
  {
    count2 = 0;

    pixels.setPixelColor(LEDBAR1_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR2_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR3_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR4_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR5_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR6_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 


pixels.setPixelColor(LEDBAR7_LED_IDX+1, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+2, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+3, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+4, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+5, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+6, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+7, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+8, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 



    pixels.setPixelColor(LEDBAR7_LED_IDX+9, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+10, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+11, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+12, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+13, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+14, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+15, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    pixels.setPixelColor(LEDBAR7_LED_IDX+16, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); 
    
    
 }
  
}






// LEDs inside tubes
void update_tube1_leds()
{
  float tube_a = 0.8;
  
  // First in tube 1
  static float r_target1 = 0, g_target1 = 255, b_target1 = 0;
  static float r_cmd1 = 0,  g_cmd1 = 255, b_cmd1 = 0;

  static int count1 = 0;
  if (++count1 > 30)
  {
    r_target1= rand() % 255;
    g_target1 = rand() % 255;
    b_target1 = rand() % 255;
    count1 = 0;
  }

  r_cmd1 = tube_a *  r_cmd1  + (1-tube_a) * r_target1;
  g_cmd1 = tube_a *  g_cmd1  + (1-tube_a) * g_target1;
  b_cmd1 = tube_a *  b_cmd1  + (1-tube_a) * b_target1;

  // Second in tube 1
  static float r_target2 = 0, g_target2 =255, b_target2 = 0;
  static float r_cmd2 = 0,  g_cmd2 = 255, b_cmd2 = 0;
   
  static int count2 = 0;
  if (++count2 > 30)
  {
    r_target2 = rand() % 255;
    g_target2 = rand() % 255;
    b_target2 = rand() % 255;
    count2 = 0;
  }

  r_cmd2 = tube_a *  r_cmd2  + (1-tube_a) * r_target2;
  g_cmd2 = tube_a *  g_cmd2  + (1-tube_a) * g_target2;
  b_cmd2 = tube_a *  b_cmd2  + (1-tube_a) * b_target2;

  pixels.setPixelColor(TUBE1_LED_IDX, pixels.Color(r_cmd1, g_cmd1, b_cmd1)); 
  pixels.setPixelColor(TUBE2_LED_IDX, pixels.Color(r_cmd2, g_cmd2, b_cmd2)); 
}

// LEDs inside tubes
void update_tube2_leds()
{
  float tube_a = 0.9;
  
  // First in tube 1
  static float r_target1 = 0, g_target1 = 255, b_target1 = 0;
  static float r_cmd1 = 0,  g_cmd1 = 255, b_cmd1 = 0;

  static int count1 = 0;
  if (++count1 > 50)
  {
    r_target1= rand() % 255;
    g_target1 = rand() % 255;
    b_target1 = rand() % 255;
    count1 = 0;
  }

  r_cmd1 = tube_a *  r_cmd1  + (1-tube_a) * r_target1;
  g_cmd1 = tube_a *  g_cmd1  + (1-tube_a) * g_target1;
  b_cmd1 = tube_a *  b_cmd1  + (1-tube_a) * b_target1;

  // Second in tube 1
  static float r_target2 = 0, g_target2 = 255, b_target2 = 0;
  static float r_cmd2 = 0,  g_cmd2 = 255, b_cmd2 = 0;
   
  static int count2 = 0;
  if (++count2 > 50)
  {
    r_target2 = rand() % 255;
    g_target2 = rand() % 255;
    b_target2 = rand() % 255;
    count2 = 0;
  }

  r_cmd2 = tube_a *  r_cmd2  + (1-tube_a) * r_target2;
  g_cmd2 = tube_a *  g_cmd2  + (1-tube_a) * g_target2;
  b_cmd2 = tube_a *  b_cmd2  + (1-tube_a) * b_target2;

  pixels.setPixelColor(TUBE3_LED_IDX, pixels.Color(r_cmd1, g_cmd1, b_cmd1)); 
  pixels.setPixelColor(TUBE4_LED_IDX, pixels.Color(r_cmd2, g_cmd2, b_cmd2)); 
}



// LEDs inside tubes
void update_reactor_leds()
{
  float tube_a = 0.9;
  
  // First in tube 1
  static float r_target1 = 0, g_target1 = 255, b_target1 = 0;
  static float r_cmd1 = 0,  g_cmd1 = 255, b_cmd1 = 0;

  static int count1 = 0;
  if (++count1 > 50)
  {
    r_target1= rand() % 255;
    g_target1 = rand() % 255;
    b_target1 = rand() % 255;
    count1 = 0;
  }

  r_cmd1 = tube_a *  r_cmd1  + (1-tube_a) * r_target1;
  g_cmd1 = tube_a *  g_cmd1  + (1-tube_a) * g_target1;
  b_cmd1 = tube_a *  b_cmd1  + (1-tube_a) * b_target1;

  // Second in tube 1
  static float r_target2 = 0, g_target2 =255, b_target2 = 0;
  static float r_cmd2 = 0,  g_cmd2 = 255, b_cmd2 = 0;
   
  static int count2 = 0;
  if (++count2 > 50)
  {
    r_target2 = rand() % 255;
    g_target2 = rand() % 255;
    b_target2 = rand() % 255;
    count2 = 0;
  }

  r_cmd2 = tube_a *  r_cmd2  + (1-tube_a) * r_target2;
  g_cmd2 = tube_a *  g_cmd2  + (1-tube_a) * g_target2;
  b_cmd2 = tube_a *  b_cmd2  + (1-tube_a) * b_target2;

  pixels.setPixelColor(REACTOR1_LED_IDX, pixels.Color(r_cmd1, g_cmd1, b_cmd1)); 
  pixels.setPixelColor(REACTOR2_LED_IDX, pixels.Color(r_cmd2, g_cmd2, b_cmd2)); 
  pixels.setPixelColor(REACTOR3_LED_IDX, pixels.Color(r_cmd1, g_cmd1, b_cmd1)); 
  pixels.setPixelColor(REACTOR4_LED_IDX, pixels.Color(r_cmd2, g_cmd2, b_cmd2)); 
 
  
}















void update_link_leds()
{
  // Link LEDS
  static int count = 0;
  if (++count > 6)
  {
    count = 0;
    pixels.setPixelColor(LINK1_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255));   // Link led 1
    pixels.setPixelColor(LINK2_LED_IDX, pixels.Color(rand() % 255, rand() % 255, rand() % 255)); // Link led 2
  
  }
   
}


// RGB Eye color
void update_eye_leds()
{
  // static float t = 0;
  //t += EYE_COLOR_STEP;
  //float val = sin(t);

  //uint8_t eye_r = 0;//mapf(val, -1, 1, EYE_COLOR_MIN, EYE_COLOR_MAX);
  //uint8_t eye_g = 0;//255;
  //uint8_t eye_b = 200;

  static float eye_r_target = 128;
  static float eye_g_target = 128;
  static float eye_b_target = 128;

  static float eye_r_cmd = 128;
  static float eye_g_cmd = 128;
  static float eye_b_cmd = 128;

  eye_r_cmd = 0;
  eye_g_cmd = 0;
  eye_b_cmd = 50;

  // override for shutdown
  if (homeing_mode)
  {
    eye_r_cmd = 50;
    eye_g_cmd = 0;
    eye_b_cmd = 0;
  }


  //eye_r_cmd = 255;
  //eye_g_cmd = 255;
  //eye_b_cmd = 255;

  /*if ((rand() % EYE_RGB_PROB) == 0)
  {
    eye_r_target = rand() % 255;
    eye_g_target = rand() % 255;
    eye_b_target = rand() % 255;
    
    //Serial.print("Eye RGB Target R: ");Serial.println(eye_r_target);
    //Serial.print("Eye RGB Target G: ");Serial.println(eye_g_target);
    //Serial.print("Eye RGB Target B: ");Serial.println(eye_b_target);
  }*/

  eye_r_cmd = EYE_RGB_ALPHA * eye_r_cmd + (1-EYE_RGB_ALPHA) * eye_r_target;
  eye_g_cmd = EYE_RGB_ALPHA * eye_g_cmd + (1-EYE_RGB_ALPHA) * eye_g_target;
  eye_b_cmd = EYE_RGB_ALPHA * eye_b_cmd + (1-EYE_RGB_ALPHA) * eye_b_target;

  // Turn off eyes when eyelids are closed
  if ( eyelid_axis.get_cmd() > 0.8) { eye_r_cmd = 0;  eye_g_cmd = 0;  eye_b_cmd = 0; };

  pixels.setPixelColor(EYE_LED_IDX, pixels.Color(eye_r_cmd, eye_g_cmd, eye_b_cmd));






  
}


void update_leds(void)
{
  update_link_leds();
  update_ledbar_leds();

//  update_tube1_leds();
//  update_tube2_leds();

//  update_reactor_leds();




//  update_eye_leds();

  pixels.show();   // Send the updated pixel colors to the hardware.

}




void update_servos(float dt)
{
  for (int i = 0; i < NUM_AXIS; i++)
    axis_list[i]->update(dt);
    
}
