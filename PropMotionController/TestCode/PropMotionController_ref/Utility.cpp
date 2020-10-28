/*
  Prop Controller
  
 (C) 2020 nulluser@gmail.com

  File: Utility.cpp
*/


#include "utility.h"

double mapf(double x, double in_min, double in_max, double out_min, double out_max)
{
  if (x < in_min) x = in_min;
  if (x > in_max) x = in_max;

  float v = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;;
  
  if (v < out_min) v = out_min;
  if (v > out_min) v = out_max;

  return v;
}
