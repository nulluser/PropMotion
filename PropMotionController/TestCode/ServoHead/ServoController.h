/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#ifndef SERVER_C
#define SERVER_C

#ifdef MODULE_SERVOC

// Servos
#define SERVO_FREQ 50 // Analog servos run at ~50 Hz updates

#define TEST_MIN 145
#define TEST_MAX 459
#define CENTER  ((TEST_MIN+TEST_MAX) / 2.0)

// Modes
#define MODE_NONE     0
#define MODE_INTERVAL 1
#define MODE_TARGET   2 
#define MODE_HOME     3 
#define MODE_TEST     100


// States
#define STATE_WAITING  0
#define STATE_MOVING  1

#endif

// Server Controller
class ServoController
{
  public:

    ServoController(char * name, float _home_pos, int pwm_chan, int pwm_min, int pwm_max);
    void init(void);
  
    void set_interval(float _rate, float _prob);
    void set_target(float _rate, float _prob);
    void set_test(void);
    void set_none();
    void set_home();
    
    float get_cmd();
    void update(float dt);

  private:
    char name[16];
    float home_pos;
    int pwm_chan;
    int pwm_min;
    int pwm_max;
    
    int mode;
    int state;
    float state_t;
  
    int half_travel;

    float rate;
    float prob;

    int test_pos;
    float target;   // For target control
    float cmd;
};

void servo_init();


#endif
