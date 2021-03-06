/* 
   ServoHead
   2020 nulluser@gmail.com

   Servos can easily break themselves 
*/

#include "Utility.h"

// Floating Point Map
double mapf(double x, double in_min, double in_max, double out_min, double out_max)
{
    if (x < in_min) x = in_min;
    if (x > in_max) x = in_max;
 
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}
