#include <Adafruit_PWMServoDriver.h>

#include "MotionGenerator.h"


#define LED LED_BUILTIN


#define POWER_ON     7

#define SERVO_FREQ 50 // Analog servos run at ~50 Hz updates

#define TEST_SERVO      7


Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();

MotionGenerator profile;// Motion profile

bool manual = true;

unsigned int servo_min = 240; // TestFirst !!!
unsigned int servo_max = 370;

uint16_t manual_pos = (servo_min  + servo_max) / 2;


float max_vel = 100;
float max_acc = 100;
float home_pos = 50;
float target = 128;

void setup()
{
  pinMode(POWER_ON , OUTPUT);

  // Turn on power
  digitalWrite(POWER_ON, 1);

  delay(3000);

  
  Serial.begin(115200);

  Serial.println("Start");

  pinMode(LED, OUTPUT);
 
  pwm.begin();


    for (int i = 0; i < 16; i++)
    pwm.setPWM(i, 0, 0);


  pwm.setOscillatorFrequency(27000000);
    
  pwm.setPWMFreq(SERVO_FREQ);  // Analog servos run at ~50 Hz updates






  delay(10);

  profile.setParameter(max_vel, max_acc, home_pos);
}


double mapf(double x, double in_min, double in_max, double out_min, double out_max)
{
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}






void update_motion()
{
  float pos =  profile.update(target);
  
  float pwm_pos = mapf(pos, 0, 255, servo_min, servo_max);
  
  if (manual)
    pwm.setPWM(TEST_SERVO, 0, manual_pos);
   else
   pwm.setPWM(TEST_SERVO, 0, pwm_pos);
  //Serial.print("PWM Pos");
  
}







float t = 0;
int count = 0;


void loop() 
{
  if (Serial.available())
  {
      char ch = Serial.read();
    //Serial.println(ch);

    if (ch == 'q') manual_pos--;
    if (ch == 'w') manual_pos++;

    if (ch == 'e') target = 255;
    if (ch == 'r') target = 128;
    if (ch == 't') target = 0;



    //if(ch == 'a') servo_min -= 1;
    //if(ch == 'q') servo_min += 1;
    //if(ch == 's') servo_max -= 1;
    //if(ch == 'w') servo_max += 1;

    if(ch == 'a') max_vel *= 0.95;
    if(ch == 'q') max_vel *= 1.05;
    if(ch == 's') max_acc *= 0.95;
    if(ch == 'w') max_acc *= 1.05;

    //profile.setParameter(max_vel, max_acc, home_pos);
    profile.setMaxVelocity(max_vel);
    profile.setMaxAcceleration(max_acc);

    //Serial.print("Min PWM: "); Serial.print(servo_min);
    //Serial.print("Max PWM: "); Serial.print(servo_max);

    Serial.print("man_pos "); Serial.print(manual_pos);
    //Serial.print("Max vel: "); Serial.print(max_vel);
    //Serial.print(" Max acc: "); Serial.print(max_acc);
        
    Serial.print("Target: "); Serial.print(target, 3);
    Serial.println();
  }
  
  update_motion();

}
