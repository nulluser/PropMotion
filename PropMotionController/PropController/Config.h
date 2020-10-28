/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#ifndef CONFIG_H
#define CONFIG_H

#include "PropController.h"

#define RUN_MODE  MODE_VIXEN
//#define RUN_MODE  MODE_RAND


//// Main Comm
#define SERIAL_BAUD     115200   // 96 updates / second
#define NUM_ELEMENTS    80

//// Intervals
#define VIXEN_TIMEOUT   1000
#define HOME_BOUNCE     1000
#define HOME_TIME       5000


//// IO
#define HOME_CMD        6
#define POWER_ON        7
#define LED             LED_BUILTIN
#define LED_PIN         4

//// Motion 

#define I2C_SPEED         400000L


// Axis Config
#define EYE_PAN_CHAN      0
#define EYE_PAN_MIN       236
#define EYE_PAN_MAX       360
#define EYE_PAN_MAX_VEL   1500
#define EYE_PAN_MAX_ACC   3000
#define EYE_PAN_PROB      0.750
#define EYE_PAN_RATE      0.995

// Eye Tilt
#define EYE_TILT_CHAN     1
#define EYE_TILT_MIN      146
#define EYE_TILT_MAX      470
#define EYE_TILT_MAX_VEL  1500
#define EYE_TILT_MAX_ACC  3000
#define EYE_TILT_PROB     0.750
#define EYE_TILT_RATE     0.995

// Eyelid
#define EYELID_CHAN       2
#define EYELID_MIN        208
#define EYELID_MAX        395
#define EYELID_MAX_VEL    1500
#define EYELID_MAX_ACC    3000
#define EYELID_PROB       0.300
#define EYELID_RATE       7.000

// Nose
#define NOSE_CHAN         3
#define NOSE_MIN          300
#define NOSE_MAX          345
#define NOSE_MAX_VEL      1000
#define NOSE_MAX_ACC      1500
#define NOSE_PROB         0.300
#define NOSE_RATE         3.00

// Brain
#define BRAIN_CHAN        4
#define BRAIN_MIN         252
#define BRAIN_MAX         320
#define BRAIN_MAX_VEL     1000
#define BRAIN_MAX_ACC     1000
#define BRAIN_PROB        0.900
#define BRAIN_RATE        3.500

// Mouth
#define MOUTH_CHAN        5
#define MOUTH_MIN         240
#define MOUTH_MAX         357
#define MOUTH_MAX_VEL     1500
#define MOUTH_MAX_ACC     3000
#define MOUTH_PROB        0.200
#define MOUTH_RATE        1.500

// Neck Pan
#define NECK_PAN_CHAN     6
#define NECK_PAN_MIN      242
#define NECK_PAN_MAX      355
#define NECK_PAN_MAX_VEL  800
#define NECK_PAN_MAX_ACC  800
#define NECK_PAN_PROB     0.800
#define NECK_PAN_RATE     0.999

// Nexk Tilt
#define NECK_TILT_CHAN    7
#define NECK_TILT_MIN     265
#define NECK_TILT_MAX     365
#define NECK_TILT_MAX_VEL 800
#define NECK_TILT_MAX_ACC 800
#define NECK_TILT_PROB    0.800
#define NECK_TILT_RATE    0.999

// Chan 15 Servo test
#define TEST_CHAN         15
#define TEST_MIN          100
#define TEST_MAX          450
#define TEST_MAX_VEL      300
#define TEST_MAX_ACC      600

////Lights

#define NUM_LEDS    24

// Eye color
//float EYE_COLOR_MAX = 200;
//float EYE_COLOR_MIN = 30;
//float EYE_COLOR_STEP = 0.015;
#define EYE_RGB_ALPHA     0.995
#define EYE_RGB_PROB      200


#define LINK1_LED_IDX       0
#define LINK2_LED_IDX       1

#define LEDBAR1_LED_IDX       2
#define LEDBAR2_LED_IDX       3
#define LEDBAR3_LED_IDX       4
#define LEDBAR4_LED_IDX       5
#define LEDBAR5_LED_IDX       6
#define LEDBAR6_LED_IDX       7
#define LEDBAR7_LED_IDX       8

#define REACTOR1_LED_IDX    9
#define REACTOR2_LED_IDX    10
#define REACTOR3_LED_IDX    11

#define POWERBOX1_LED_IDX    12
#define POWERBOX2_LED_IDX    13

#define TUBE1_LED_IDX       14
#define TUBE2_LED_IDX       15
#define TUBE3_LED_IDX       16
#define TUBE4_LED_IDX       17

#define LOGO1_LED_IDX       18
#define LOGO2_LED_IDX       19
#define LOGO3_LED_IDX       20
#define LOGO4_LED_IDX       21
#define LOGO5_LED_IDX       22

#define EYE_LED_IDX         23 // 10



#endif
