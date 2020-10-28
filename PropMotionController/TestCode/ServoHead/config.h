/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#ifndef CONFIG_H
#define CONFIG_H


// Eye color
//float EYE_COLOR_MAX = 200;
//float EYE_COLOR_MIN = 30;
//float EYE_COLOR_STEP = 0.015;
#define EYE_RGB_ALPHA   0.995
#define EYE_RGB_PROB      200

// Axis Config
#define EYE_PAN_CHAN  0
#define EYE_PAN_MIN   236
#define EYE_PAN_MAX   360
#define EYE_PAN_PROB  0.750
#define EYE_PAN_RATE  0.995

// Eye Tilt
#define EYE_TILT_CHAN 1
#define EYE_TILT_MIN  146
#define EYE_TILT_MAX  470
#define EYE_TILT_PROB 0.750
#define EYE_TILT_RATE 0.995

// Eyelid
#define EYELID_CHAN   2
#define EYELID_MIN    208
#define EYELID_MAX    395
#define EYELID_PROB   0.300
#define EYELID_RATE   7.000

// Nose
#define NOSE_CHAN     3
#define NOSE_MIN      300
#define NOSE_MAX      345
#define NOSE_PROB     0.300
#define NOSE_RATE     3.00

// Brain
#define BRAIN_CHAN    4
#define BRAIN_MIN     252
#define BRAIN_MAX     320
#define BRAIN_PROB    0.800
#define BRAIN_RATE    3.500

// Mouth
#define MOUTH_CHAN    5
#define MOUTH_MIN     240
#define MOUTH_MAX     357
#define MOUTH_PROB    0.200
#define MOUTH_RATE    1.500


// Neck Pan
#define NECK_PAN_CHAN    6
#define NECK_PAN_MIN     242
#define NECK_PAN_MAX     355
#define NECK_PAN_PROB    0.800
#define NECK_PAN_RATE    0.999


// Nexk Tilt
#define NECK_TILT_CHAN    7
#define NECK_TILT_MIN     231
#define NECK_TILT_MAX     370
#define NECK_TILT_PROB    0.800
#define NECK_TILT_RATE    0.999


// Chan 15 Servo test
#define TEST_CHAN     15
#define TEST_MIN      200
#define TEST_MAX      500


#endif
