/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: LightSource.cs
*/


using System.Collections.Generic;

using OpenTK;


public class LightSrc
{
	const string MODULE = "[LightSrc]   ";
	
	private string name;			// Material name
	
	private Vector3 position;		// Position of light source
	private Vector3 color;			// Color of light source
	private int light_idx;			// Contorl channel for light
	
	private float strength = 0;		// Intensity of light

	// Load Material from config file
	public bool load_from_config(Queue<string> items)
	{
		light_idx = -1;
		
		name = items.Dequeue();

		Util.alert(MODULE + "Loading Light Source " + name);
	
		if (items.Count < 8)
		{
			Util.alert(MODULE + "Not enoughlight properties");
			return true;
		}

		position	= Util.get_vector(items); 
		strength	= Util.get_float(items);
		color		= Util.get_color3(items); 
		light_idx	= Util.get_int(items); 
		
		/*Util.alert(MODULE + "  Name: " + name);
		Util.alert(MODULE + "  Position " + position.X + " "+  position.Y + " " + position.Z);
		Util.alert(MODULE + "  Strength " + strength);
		Util.alert(MODULE + "  Color " + color.X + " "+  color.Y + " " + color.Z);
		Util.alert(MODULE + "  Index " + light_idx);*/

		/*Util.alert(MODULE + "  Name: " + name);
		Util.alert(MODULE + "  Diffuse Texture: " + diffuse_file);
		Util.alert(MODULE + "  Specular Texture: " + specular_file);
		Util.alert(MODULE + "  Normal Texture: " + normal_file);
		Util.alert(MODULE + "  Color " + color.R + " "+  color.G + " " + color.B + " " + color.A);
		Util.alert(MODULE + "  Ambient_strength: " + ambient_strength);
		Util.alert(MODULE + "  Diffuse_strength: " + diffuse_strength );
		Util.alert(MODULE + "  Specular strength: " + specular_strength);
		Util.alert(MODULE + "  Specular exponent: " + specular_exp );*/

		return false;
	}
	
	// Update light source color from control data
	public void update(byte [] control_data, float ref_time)
	{
		if (light_idx >= 0)
		{
			
			color.X = strength * control_data[light_idx+0] / 255.0f;;
			color.Y = strength * control_data[light_idx+1] / 255.0f;;
			color.Z = strength * control_data[light_idx+2] / 255.0f;;
		}
	}
	
	// Set light specific uniforms
	public void set_uniforms(int i, Shader shader)
	{
		shader.set_uniform(shader.uLightSourcePos[i], position);	
		shader.set_uniform(shader.uLightSourceColor[i], color);	
	}
		
	// Return name
	public string get_name()
	{
		return name;
	}
	
}



