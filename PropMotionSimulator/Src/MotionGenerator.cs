/**
	* Generates the analytical solution for the trapezoidal motion.
	*
	* <p>
	* Usage:
	* // Includes
	* #include "MotionGenerator.h"
	*
	* Initialization
	*
	* @param int aVelMax maximum velocity (units/s)
	* @param int aAccMax maximum acceleration (units/s^2)
	* @param int aInitPos initial position (units)
	*

	// Define the MotionGenerator object
	MotionGenerator *trapezoidalProfile = new MotionGenerator(100, 400, 0);

	// Retrieve calculated position
	float positionRef = 1000;
	float position = trapezoidalProfile->update(positionRef)

	// Retrieve current velocity
	float velocity = trapezoidalProfile->getVelocity();

	// Retrieve current acceleration
	float acceleration = trapezoidalProfile->getAcceleration();

	// Check if profile is finished
	if (trapezoidalProfile->getFinished()) {};

	// Reset internal state
	trapezoidalProfile->reset();

	*
	* @author      AerDronix <aerdronix@gmail.com>
	* @web		https://aerdronix.wordpress.com/
	* @version     1.0 
	* @since       2016-12-22


	nulluser 10-20-2020: Ported C#
*/


using System;


public class MotionGenerator 
{		
	private float maxVel;
	private float maxAcc;
	private float initPos;
	private float pos;
	private float vel;
	private float acc;
	private float oldPos;
	private float oldPosRef;
	private float oldVel;

	private float dBrk;
	private float dAcc;
	private float dVel;
	private float dDec;
	private float dTot;

	private float tBrk;
	private float tAcc;
	private float tVel;
	private float tDec;
		
	private float velSt;
		
	private double oldTime;
	private double lastTime;
	private double deltaTime;
		
	private int signM;      	// 1 = positive change, -1 = negative change
	private bool shape;      	// true = trapezoidal, false = triangular
		
	//bool isFinished;	

	/**	
	 * Constructor
	 * 
	 * @param int aVelocityMax maximum velocity
	 * @param int aAccelerationMax maximum acceleration
	 */

	public MotionGenerator()
	{
		//Util.alert("MG base const");
		maxVel = 0;
		maxAcc = 0;
		initPos = 0;

		init();
	}


	public MotionGenerator(float aMaxVel, float aMaxAcc, float aInitPos)
	{
		//Util.alert("MG param const");
		maxVel = aMaxVel;
		maxAcc = aMaxAcc;
		initPos = aInitPos;
		
		init();
	}

	public void init() 
	{	
		//Util.alert("MG Init " + maxVel  + " " + maxAcc );
	
		// Time variables
		oldTime = 0;//Util.get_time();
		lastTime = oldTime;
		deltaTime = 0;	
		
		// State variables
		reset();	
		
		// Misc
		signM = 1;		// 1 = positive change, -1 = negative change
		shape = true;   // true = trapezoidal, false = triangular
		//isFinished = false;
	}


	/**	
	 * Updates the state, generating new setpoints
	 *
	 * @param aSetpoint The current setpoint.
	 */

	public float update(float posRef, double time) 
	{	
		if (oldPosRef != posRef)  // reference changed
		{
			//isFinished = false;
			// Shift state variables
			oldPosRef = posRef;
			oldPos = pos;
			oldVel = vel;
			oldTime = lastTime;
			
			// Calculate braking time and distance (in case is neeeded)
			tBrk = Math.Abs(oldVel) / maxAcc;
			dBrk = tBrk * Math.Abs(oldVel) / 2.0f;
			
			// Caculate Sign of motion
			signM = sign(posRef - (oldPos + sign(oldVel)*dBrk));
			
			if (signM != sign(oldVel))  // means brake is needed
			{
				tAcc = (maxVel / maxAcc);
				dAcc = tAcc * (maxVel / 2.0f);
			}
			else
			{
				tBrk = 0;
				dBrk = 0;
				tAcc = (maxVel - Math.Abs(oldVel)) / maxAcc;
				dAcc = tAcc * (maxVel + Math.Abs(oldVel)) / 2.0f;
			}
			
			// Calculate total distance to go after braking
			dTot = Math.Abs(posRef - oldPos + signM*dBrk);
			
			tDec = maxVel / maxAcc;
			dDec = tDec * (maxVel) / 2.0f;
			dVel = dTot - (dAcc + dDec);
			tVel = dVel / maxVel;
			
			if (tVel > 0)    // trapezoidal shape
				shape = true;
			else             // triangular shape
			{
				shape = false;
				// Recalculate distances and periods
				if (signM != sign(oldVel))  // means brake is needed
				{
					velSt = (float)Math.Sqrt(maxAcc*(dTot));
					tAcc = (velSt / maxAcc);
					dAcc = tAcc * (velSt / 2.0f);
				}
				else
				{
					tBrk = 0;
					dBrk = 0;
					dTot = Math.Abs(posRef - oldPos);      // recalculate total distance
					velSt = (float)Math.Sqrt(0.5f*oldVel*oldVel + maxAcc*dTot);
					tAcc = (velSt - Math.Abs(oldVel)) / maxAcc;
					dAcc = tAcc * (velSt + Math.Abs(oldVel)) / 2.0f;
				}
				tDec = velSt / maxAcc;
				dDec = tDec * (velSt) / 2.0f;
			}
		}
		
		// Calculate time since last set-point change
		deltaTime = (time - oldTime);
		
		// Calculate new setpoint
		calculateTrapezoidalProfile(posRef);

		// Update last time
		lastTime = time;
		
		return pos;
	}


	public void calculateTrapezoidalProfile(float posRef) 
	{
		float t = (float)deltaTime;
		
		if (shape)   // trapezoidal shape
		{
			if (t <= (tBrk+tAcc))
			{
				pos = (float)(oldPos + oldVel*t + signM * 0.5*maxAcc*t*t);
				vel = oldVel + signM * maxAcc*t;
				acc = signM * maxAcc;
			}
			else if (t > (tBrk+tAcc) && t < (tBrk+tAcc+tVel))
			{
				pos = oldPos + signM * (-dBrk + dAcc + maxVel*(t-tBrk-tAcc));
				vel = signM * maxVel;
				acc = 0;
			}
			else if (t >= (tBrk+tAcc+tVel) && t < (tBrk+tAcc+tVel+tDec))
			{
				pos = oldPos + signM * (-dBrk + dAcc + dVel + maxVel*(t-tBrk-tAcc-tVel) - 0.5f*maxAcc*(t-tBrk-tAcc-tVel)*(t-tBrk-tAcc-tVel));
				vel = signM * (maxVel - maxAcc*(t-tBrk-tAcc-tVel));
				acc = - signM * maxAcc;
			}
			else
			{
				pos = posRef;
				vel = 0;
				acc = 0;
				//isFinished = true;
			}
		}
		else            // triangular shape
		{
			if (t <= (tBrk+tAcc))
			{
				pos = oldPos + oldVel*t + signM * 0.5f*maxAcc*t*t;
				vel = oldVel + signM * maxAcc*t;
				acc = signM * maxAcc;
			}
			else if (t > (tBrk+tAcc) && t < (tBrk+tAcc+tDec))
			{
				pos = oldPos + signM * (-dBrk + dAcc + velSt*(t-tBrk-tAcc) - 0.5f*maxAcc*(t-tBrk-tAcc)*(t-tBrk-tAcc));
				vel = signM * (velSt - maxAcc*(t-tBrk-tAcc));
				acc = - signM * maxAcc;
			}
			else
			{
				pos = posRef;
				vel = 0;
				acc = 0;
				//isFinished = true;
			}
			
		}

	} 

	// Getters and setters
	/*public bool getFinished()
	{
		return true;//isFinished;
	}*/

	public void setParameter(float aMaxVel, float aMaxAcc, float aInitPos)
	{
		//Util.alert(" setParameter ");
		
		maxVel = aMaxVel;
		maxAcc = aMaxAcc;
		initPos = aInitPos;
		//Util.alert(" VA: maxVel  " + maxVel);
		//Util.alert(" VB: maxAel  " + maxAcc);

	
		init();
	}

	public float getVelocity() 
	{
		return vel;
	}

	public float getAcceleration() 
	{
		return acc;
	}

	public void setMaxVelocity(float aMaxVel) 
	{
		maxVel = aMaxVel;
	}

	public void setMaxAcceleration(float aMaxAcc) 
	{
		maxAcc = aMaxAcc;
	}

	public void setInitPosition(float aInitPos) 
	{
		initPos 	= aInitPos;
		pos 		= aInitPos;
		oldPos 		= aInitPos;
	}

	public int sign(float aVal) 
	{
		if (aVal < 0)
			return -1;
		else if (aVal > 0)
			return 1;
		else
			return 0;
	}

	public void reset() 
	{
		// Reset all state variables	

		pos 		= initPos;
		oldPos 		= initPos;
		oldPosRef 	= 0;
		vel 		= 0;
		acc 		= 0;
		oldVel 		= 0;
		
		dBrk 		= 0;
		dAcc 		= 0;
		dVel 		= 0;
		dDec 		= 0;
		dTot 		= 0;
		
		tBrk 		= 0;
		tAcc 		= 0;
		tVel 		= 0;
		tDec 		= 0;
		
		velSt 		= 0;
	}


}