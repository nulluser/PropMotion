/*
  Prop Controller
  
 (C) 2020 nulluser@gmail.com


  File: PropController.ino
*/


#include "Config.h"
#include "PropController.h"

void setup()
{
  Serial.begin(SERIAL_BAUD);
  delay(1000);
  Serial.println("\n\n\n\n");
  
  propcontroller_init();
}

void loop ()
{
  propcontroller_update();
}
