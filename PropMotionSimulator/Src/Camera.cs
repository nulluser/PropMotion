/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Camera.cs
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Camera
{
	const string MODULE = "[Camera]     ";
		
	private Vector3 position = new Vector3(0.0f, 0.0f, 30.0f);		// Default Position
	private Vector3 direction = new Vector3((float)-Math.PI, 0f, 0f);
	private Matrix4 view_matrix = Matrix4.Identity;
	
	// Move camera by a delta
	public void move(float turn, float forward)
	{
		float turn_speed = 0.025f;
		direction.X += turn_speed * turn;
			
		float forward_speed = 1.0f;
		position.Z += forward * forward_speed *(float) Math.Cos(direction.X);
		position.X += forward * forward_speed *(float) Math.Sin(direction.X);
		update_view_matrix();
	}
	
	// Strafe camera by a delta
	public void strafe(float strafe)
	{
		float strafe_speed  = 1.0f;
		position.Z += strafe * strafe_speed * (float)Math.Cos(direction.X + Math.PI/2.0f);
		position.X += strafe * strafe_speed * (float) Math.Sin(direction.X + Math.PI/2.0f);
		update_view_matrix();
	}	
	
	
	// Update view matrix
	private void update_view_matrix()
	{
		Vector3 lookat = new Vector3();

		lookat.X = (float)(Math.Sin((float)direction.X) * Math.Cos((float)direction.Y));
		lookat.Y = (float)Math.Sin((float)direction.Y);
		lookat.Z = (float)(Math.Cos((float)direction.X) * Math.Cos((float)direction.Y));
		//Util.alert("Dir: " + camera_direction.X + " " + camera_direction.Y + " " + camera_direction.Z +  "Pos: " + camera_position.X + " " + camera_position.Y + " " + camera_position.Z + " " + " Look: " + lookat.X + " " + lookat.Y + " " + lookat.Z);
		view_matrix = Matrix4.LookAt(position, position + lookat, Vector3.UnitY);
	}
		
		
	// Set Current Position
	public void set_position(float x, float y, float z)
	{
		position.X = x;
		position.Y  = y;
		position.Z  = z;
		update_view_matrix();
		
	}		
		
	// Return view matrix
	public Matrix4 get_view_matrix()
	{
		return view_matrix;
	}
	
	// Return Current Position
	public Vector3 get_position()
	{
		return position;
	}
}

