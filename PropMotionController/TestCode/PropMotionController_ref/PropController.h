/*
  Prop Controller
  
 (C) 2020 nulluser@gmail.com

  File: PropController.h
*/


#ifndef PROPCTRL_H
#define PROPCTRL_H

void propcontroller_init();
void propcontroller_update();

#ifdef MODULE_PROPCTRL

void update_motion(void);
void update_stats(void);

#endif

#endif
