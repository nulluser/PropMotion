/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#define MODULE_LIGHTC

#include <FastLED.h>

#include "Config.h"
#include "Utility.h"
#include "LightController.h"



LightController::LightController()
{


  
}

void LightController::init()
{
  //Serial.println("Servo init");
  
  FastLED.addLeds<NEOPIXEL, LED_PIN>(leds, NUM_LEDS);

  //pixels.begin(); 
  //pixels.clear(); // Set all pixel colors to 'off'

  setall(0, 255, 255);
 
  delay(2000);

  
}


void LightController::update()
{
  
  static uint8_t count = 0;

  if (++count < 3) return;

  count = 0;
  
  for (int i = 0; i < NUM_LEDS; i++)
  {
      if ((rand() % 100) <250)
      leds[i].setRGB( rand() % 255, rand() % 255, rand() % 255);
  }
  
  show();
}


void LightController::show()
{
   FastLED.show(); 

  
}


void LightController::setled(uint8_t index, uint8_t r, uint8_t g, uint8_t b )
{
  leds[index].setRGB(r, g, b);
}


void LightController::setall(uint8_t r, uint8_t g, uint8_t b )
{
  for (int i = 0; i < NUM_LEDS; i++)
  {
    leds[i].setRGB( r, g, b);
  }

   FastLED.show();
}
