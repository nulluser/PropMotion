/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
*/

using System;

// Motion axis
public class Axis
{
	public const int AXIS_N = 0; // TODO, enum
	public const int AXIS_X = 1;
	public const int AXIS_Y = 2;
	public const int AXIS_Z = 3;
	
	
	// Movement
	private int motion_idx;			// 
	private float min;
	private float max;
	private int axis_idx;		// 0 off, 1 X, 2 Y, 3 Z

	// Motion
	private float target;
	private float current;
	private MotionGenerator motion = new MotionGenerator();
	
	public Axis()
	{
	}

	public void set_paremeter(int _motion_idx, int _axis_idx, float _min, float _max,
							float _max_vel, float _max_acc, float _home_pos)
	{
		motion_idx = _motion_idx;
		axis_idx = _axis_idx;
		min = _min;
		max = _max;
		
		motion.setParameter(_max_vel, _max_acc, _home_pos);
	}

	// Update axis data
	public float update(byte [] motion_data, int test, double time)
	{
		// Test enabled will override
		if (test == axis_idx)
		{
			byte val = (byte)(128 + 127 * (float)Math.Sin(1 * time));
			
			target = Util.mapf(val, 0, 255, min, max);
		} else
		// Otherwise apply axis motion if configured
		if (motion_idx != -1)
		{
			byte val = motion_data[motion_idx];
			
			// Ignore 0s from sequencer
			if (val != 0)
				target = Util.mapf(val, 0, 255, min, max);
		}
		
		current = motion.update(target, time); 
		
		return current;
	}

public int get_motion_idx () { return motion_idx; }
}


