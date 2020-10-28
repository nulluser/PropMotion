
#include <Adafruit_NeoPixel.h>

#define HOME_CMD     6
#define POWER_ON     7


#define LED          LED_BUILTIN



#define EYE_LED_PIN  4

// Main loop delay
// Total of pixels
#define NUMLEDS    24

Adafruit_NeoPixel pixels(NUMLEDS, EYE_LED_PIN, NEO_GRB + NEO_KHZ800);




void setup()
{
  Serial.begin(115200);
  
  // Setup IO
  pinMode(HOME_CMD, INPUT_PULLUP);
  pinMode(POWER_ON, OUTPUT);

  // Turn on power
  digitalWrite(POWER_ON, 1);

  pixels.begin(); 
  pixels.clear(); // Set all pixel colors to 'off'

  delay(1000);

  test_leds();
  
}




#define NUM_ELEMENTS    80

#define SS_WAIT1    0
#define SS_WAIT2    1
#define SS_DATA      2


uint8_t serial_state = SS_WAIT1;

int rx_index = 0;
uint8_t rx_buffer[NUM_ELEMENTS];



void clear_leds()
{



  
}


void test_leds()
{

  for (int i = 0; i < NUMLEDS; i++)
  {
    uint8_t r = 0;//255;
    uint8_t g = 255;
    uint8_t b = 255;
 
    pixels.setPixelColor(i, r, g, b); 
  }

  pixels.show();  

  delay(4000);
  
}






void process_buffer()
{
/*
  static int count = 0;
  static int state = 0;

  if (count++ > 10)
  {
    count = 0;


    if (state == 0)
    {
      pixels.setPixelColor(0, 1, 1, 2); 
      state = 1;
    } else
    {
      pixels.setPixelColor(0, 255, 255, 255); 
      state = 0;
    }

  pixels.show();  
  }
  
  */

















  

  int index = 0;

  for (int i = 0; i < NUMLEDS; i++)
  {
    uint8_t r = rx_buffer[index++];
    uint8_t g = rx_buffer[index++];
    uint8_t b = rx_buffer[index++];
 
    pixels.setPixelColor(i, r, g, b); 
  }


    pixels.show();  
 
}


void process_serial(uint8_t c)
{

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
    if (c == '!') serial_state = SS_DATA; else serial_state = SS_WAIT1;
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








void loop ( void )
{

  check_serial();

  
  //pixels.setPixelColor(0, r, g, b); 
  //pixels.setPixelColor(1, r, g, b); 
      
    
  //pixels.show();   // Send the updated pixel colors to the hardware.



}
