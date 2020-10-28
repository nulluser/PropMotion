
#include <Adafruit_NeoPixel.h>

#define EYE_LED_PIN   4
#define DELAYVAL      1 
#define NUMPIXELS     1 

Adafruit_NeoPixel pixels(NUMPIXELS, EYE_LED_PIN, NEO_GRB + NEO_KHZ800);

void setup() 
{
  pixels.begin(); 
  pixels.clear(); // Set all pixel colors to 'off'

}

float eye_max = 200;
float eye_min = 30;
float eye_step = 0.005;


double mapf(double x, double in_min, double in_max, double out_min, double out_max)
{
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}



void update_eyes()
{
  static float t = 0;

  t += eye_step;

  float val = sin(t);

  uint8_t eye_r = mapf(val, -1, 1, eye_min, eye_max);

  pixels.setPixelColor(0, pixels.Color(eye_r, 10, 0));

  pixels.show();   // Send the updated pixel colors to the hardware.

  
}

void loop() 
{
  update_eyes();

    delay(DELAYVAL); // Pause before next pass through loop
}
