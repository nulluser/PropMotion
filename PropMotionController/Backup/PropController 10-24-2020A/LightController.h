/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/


#ifndef LIGHTCTRL_C
#define LIGHTCTRL_C

#include <FastLED.h>
#include <stdint.h>

#ifdef MODULE_LIGHTC

#endif

// Server Controller
class LightController
{
  public:

    LightController();
    void init(void);
    void update(void);
    
    void show();

    void setall(uint8_t r, uint8_t g, uint8_t b );
    void setled(uint8_t index, uint8_t r, uint8_t g, uint8_t b );

  private:

  // Define the array of leds
  CRGB leds[NUM_LEDS];
};

#endif
