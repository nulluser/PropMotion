/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: MotionObject.cs
*/

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using STL_Tools;


public class MotionObject
{
	private const string MODULE = "[MotionObj]  ";
	
	public string name;								// Object name
	
	// Positioning
	private Vector3 position;						// Draw location relative to parent
	private Vector3 position_offset;				// Pre-move before rotation. Used to recenter
	private Vector3	rotation;						// Current rotation in addition to static
	private Vector3 rotation_offset;				// Static rotation
	private Vector3 scale;

	// Movement axis
	private Axis x_axis, y_axis, z_axis;

	// Parent / Children
	private List<int> child_list = new List<int>();	// Index of children
	private string parent_name;						// Name of parent. Used to resolve parent index
	private string material_name;					// Name of material
	private string geometry_name;					// Name of geometry

	private int parent_idx = -1;					// Index of parent 
	private int geometry_idx = -1;					// Indec of geometry 
	private int material_idx = -1;						// Index of material to apply 
	private int light_idx = -1;						// motion axis index for light override

	// Graphics Data
	private Matrix4 model_matrix = Matrix4.Identity;					// Current model matrix.
	
	private Color4 light_color;

	// Testing	
	//private double ref_time;						// Time since start
	private int test_axis;								// true if testing
	
	public MotionObject()
	{
	}
	
	// Update Motion
	// Todo add motion profiles
	public void update(byte [] control_data, float ref_time)
	{ 
		// Update motion profiles
		rotation.X = x_axis.update(control_data, test_axis, ref_time);
		rotation.Y = y_axis.update(control_data, test_axis, ref_time);
		rotation.Z = z_axis.update(control_data, test_axis, ref_time);
	
		// Recompute model matrix
		update_matrix();
	
		// Update object color
		if (light_idx != -1)
		{
			float r = control_data[light_idx+0] / 255.0f;
			float g = control_data[light_idx+1] / 255.0f;
			float b = control_data[light_idx+2] / 255.0f;
			
			light_color = new Color4(r, g, b, 1.0f);  // 0tells shader to ignore shading and just produce light
		}
	}

	
	// Update model matrix from position and rotation
	public void update_matrix()
	{
		model_matrix = Matrix4.Identity;
	
		// Tramsforms need to happen in reverse order
		// Move model into position, Happens last
		var translation = Matrix4.CreateTranslation(position.X, position.Y, position.Z);			
		Matrix4.Mult(ref translation, ref model_matrix, out model_matrix);

		// Rotate Around center of model
		var rotationX = Matrix4.CreateRotationX(Util.degtorad(rotation.X));
		var rotationY = Matrix4.CreateRotationY(Util.degtorad(rotation.Y));
		var rotationZ = Matrix4.CreateRotationZ(Util.degtorad(rotation.Z));
		
		Matrix4.Mult(ref rotationX, ref model_matrix, out model_matrix);
		Matrix4.Mult(ref rotationY, ref model_matrix, out model_matrix);
		Matrix4.Mult(ref rotationZ, ref model_matrix, out model_matrix);
		
		// Apply Rotation offset
		rotationX = Matrix4.CreateRotationX(Util.degtorad(rotation_offset.X));
		rotationY = Matrix4.CreateRotationY(Util.degtorad(rotation_offset.Y));
		rotationZ = Matrix4.CreateRotationZ(Util.degtorad(rotation_offset.Z));
		
		Matrix4.Mult(ref rotationX, ref model_matrix, out model_matrix);
		Matrix4.Mult(ref rotationY, ref model_matrix, out model_matrix);
		Matrix4.Mult(ref rotationZ, ref model_matrix, out model_matrix);

		// Apply position happens first
		translation = Matrix4.CreateTranslation(position_offset.X, position_offset.Y, position_offset.Z);			
		Matrix4.Mult(ref translation, ref model_matrix, out model_matrix);
	}
	
	// Draw mesh or sub objects
	public void draw()
	{ 
	}
	
	// Load motion option from model config information
	// Todo Merge group and model
	public bool load_from_model_config(Queue<string> items)
	{
		name = items.Dequeue();

		Util.alert(MODULE + "Loading Model: " + name);

		if (items.Count < 32) { Util.alert("Model command expects 32 parameters " + items.Count); return true; }
		
		parent_name = items.Dequeue();
		//Util.alert(MODULE + "  Parent: " + parent_name);			

		geometry_name = items.Dequeue();
		//Util.alert(MODULE + "  Geometry: " + geometry_name);
		
		material_name = items.Dequeue();
		//Util.alert(MODULE + "  Material: " + material_name);
		
		test_axis = Util.get_int(items);
		//Util.alert(MODULE + "  Test Axis: " + test_axis);
		
		// Load position and orientation data
		position        = Util.get_vector(items); 
		//Util.alert(MODULE + "  Pos: " + position.X + " " + position.Y + " " + position.Z);
		
		position_offset = Util.get_vector(items); 
		//Util.alert(MODULE + "  PosOfs: " + position_offset.X + " " + position_offset.Y + " " + position_offset.Z);
		
		rotation_offset = Util.get_vector(items); 
		//Util.alert(MODULE + "  RotOfs: " + rotation_offset.X + " " + rotation_offset.Y + " " + rotation_offset.Z);
		
		scale   = Util.get_vector(items); 
		//Util.alert(MODULE + "  Scale: " + scale.X + " " + scale.Y + " " + scale.Z);
		
		// Load axis data
		x_axis = Util.get_axis(items, Axis.AXIS_X);	// x axis config
		//Util.alert(MODULE + "  X Axis: " + x_axis.motion_idx + " " + x_axis.min + " " + x_axis.max);
		
		y_axis = Util.get_axis(items, Axis.AXIS_Y);	// y axis config
		//Util.alert(MODULE + "  Y Axis: " + y_axis.motion_idx + " " + y_axis.min + " " + y_axis.max);		
		
		z_axis = Util.get_axis(items, Axis.AXIS_Z);	// z axis config
		//Util.alert(MODULE + "  Z Axis: " + z_axis.motion_idx + " " + z_axis.min + " " + z_axis.max);
		
		light_idx = Util.get_int(items);
		light_idx--;
		
		//Util.alert(MODULE + "  light_idx: " + light_idx);

		//Util.alert(MODULE + "  x axis idx: " + x_axis.get_motion_idx());
		//Util.alert(MODULE + "  y axis idx: " + y_axis.get_motion_idx());
		//Util.alert(MODULE + "  z axis idx: " + z_axis.get_motion_idx());
		
		/*Util.alert(MODULE + "  Object: " + name);
		Util.alert(MODULE + "  Parent: " + parent_name);		
		Util.alert(MODULE + "  Model: " +model_file);
		Util.alert(MODULE + "  test_axis: " + test_axis);
		Util.alert(MODULE + "  Pos: " + position.X + " " + position.Y + " " + position.Z);
		Util.alert(MODULE + "  PosOfs: " + position_offset.X + " " + position_offset.Y + " " + position_offset.Z);
		Util.alert(MODULE + "  RotOfs: " + rotation_offset.X + " " + rotation_offset.Y + " " + rotation_offset.Z);
		Util.alert(MODULE + "  Scale: " + scale.X + " " + scale.Y + " " + scale.Z);*/
		
		return false;
	}
	
	
	
	//// Parent / Child related

	// Return index of parent object
	public int get_parent()
	{
		return parent_idx;
	}
	
	// Add index of a child object
	public void add_child(int idx)
	{
		child_list.Add(idx);
	}
	
	// Return list of child indexs
	public List<int> get_child_list() 
	{
		return child_list;
	}

	// Resolve parent id
	public bool resolve_parent(List<string> name_list)
	{
		parent_idx = Util.find_item(parent_name, name_list);
		
		if (parent_idx < 0 && parent_name != "none")
			Util.error(MODULE + "Unable to resolve parent of " + name + " (" + parent_name + ")");
		
		return true;		
	}
	
	// Resolve material id
	public bool resolve_material(List<string> name_list)
	{
		material_idx = Util.find_item(material_name, name_list);
		
		if (material_idx < 0 && material_name != "none")
			Util.error(MODULE + "Unable to resolve material of " + name + " (" + material_name + ")");
		
		return true;		
	}	
	
	// Resolve material id
	public bool resolve_geometry(List<string> name_list)
	{
		geometry_idx = Util.find_item(geometry_name, name_list);
		
		if (geometry_idx < 0 && geometry_name != "none")
			Util.error(MODULE + "Unable to resolve geometry of " + name + " (" + geometry_name + ")");
		
		return true;
	}	
		
	public string get_name()		{ return name;}
	public bool get_light_enabled()	{ return  light_idx != -1; }
	public Color4 get_light_color()	{ return light_color;}
	public Vector3 get_scale()		{ return scale;	}
	public Matrix4 get_matrix()		{ return model_matrix;}
	public int get_material()		{ return material_idx;}
	public int get_geometry()		{ return geometry_idx;}
};

