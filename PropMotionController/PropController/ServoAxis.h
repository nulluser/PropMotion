/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/


#ifndef SERVOAXIS_C
#define SERVOAXIS_C

#include "MotionGenerator.h"

#define DISABLED 0
#define ENABLED  1

#ifdef MODULE_SERVOAXIS

// Servos
#define SERVO_FREQ 50 // Analog servos run at ~50 Hz updates



// Modes
#define MODE_HOLD  0     
#define MODE_INTERVAL 1
#define MODE_RAND   2 
//#define MODE_HOME     3 
//#define MODE_TEST     100


// States
#define STATE_WAITING  0
#define STATE_MOVING  1

#endif

// Server Controller
class ServoAxis
{
  public:

    ServoAxis( bool _enabled, float _home_pos, uint8_t pwm_chan, 
                    uint16_t pwm_min, uint16_t pwm_max, float max_vel, float max_acc);
    void init(void);
  
    void set_interval(float _rate, float _prob);
    void set_rand(float _rate, float _prob);
    void set_test(void);
    void set_hold();
    void set_home();
    //void set_home_pos(void);

    void set_target_pos(uint8_t target);
    
    float get_cmd();
    void update(float dt);

    uint16_t get_pwm_pos() { return pwm_pos; }
    uint16_t get_pwm_chan() { return pwm_chan; }
    
    uint16_t map_pwm(float pos_cmd);

  private:

    MotionGenerator profile;// Motion profile

    uint8_t home_pos;

    float max_vel;
    float max_acc;
    
    uint8_t pwm_chan;
    uint16_t pwm_min;
    uint16_t pwm_max;
    
    uint8_t mode;
    uint8_t state;
    float state_t;
  
    bool half_travel;

    float rate;
    float prob;

    
    float target;   // For target control
    float cmd;

    uint16_t pwm_pos;
    
    bool enabled;

};

//void servo_init();


#endif
