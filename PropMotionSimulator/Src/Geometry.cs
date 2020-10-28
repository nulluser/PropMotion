/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Geometry.cs
	
	Geometry Data
*/

using System;
using System.Collections.Generic;

using OpenTK;
using STL_Tools;

public class Geometry
{
	const string MODULE = "[Geometry]   ";
	
	private string name;			// Material name
	
	private TriangleMesh [] mesh = null;
	private VertexArrayObject vao = null;
	
	// Load Geometry from config file
	public bool load_from_config(Queue<string> items)
	{
		if (items.Count < 1) {Util.alert(MODULE + "Command needs 1 parameter"); return true; }
		
		name = items.Dequeue();
		
		Util.alert(MODULE + "Loading Geometry: " + name);

		string model_file = items.Dequeue();
		//Util.alert(MODULE + "  Model: " + model_file);

		if (load_stl(model_file)) 
		{
			Util.error(MODULE + "Unable to load geometry");
			return true;
		}
			
		return false;
	}
	
	
	// Load STL file
	public bool load_stl(string model_file)
	{
		//Util.alert(MODULE + "  Loading STL: " + model_file);
			
		Vector3 min, max;
			
		try
		{			
			STLReader stl_reader = new STLReader(model_file);;
			mesh = stl_reader.ReadFile();
			
			// Determine bounding box
			min = stl_reader.GetMinMeshPosition(mesh);
			max = stl_reader.GetMaxMeshPosition(mesh);
		}
		catch (Exception ex)
		{
			Util.error(MODULE + "Unable to load " + model_file + " " + ex.Message) ;
			return true;
		}
		
		//Util.alert(MODULE + "  Mesh triangles: " + mesh.Length);

		// ReCenter
		
		Vector3 center = new Vector3((min.X + max.X) / 2.0f,  (min.Y + max.Y) / 2.0f,  (min.Z + max.Z) / 2.0f);		

		for (int i = 0; i < mesh.Length; i++)
		{
			mesh[i].vert1.X -= center.X; mesh[i].vert1.Y -= center.Y; mesh[i].vert1.Z -= center.Z;
			mesh[i].vert2.X -= center.X; mesh[i].vert2.Y -= center.Y; mesh[i].vert2.Z -= center.Z;
			mesh[i].vert3.X -= center.X; mesh[i].vert3.Y -= center.Y; mesh[i].vert3.Z -= center.Z;
		}		
		
		return false;
	}
	
	
	// Create VAO for object
	public void create_buffer(Shader shader)
	{
		if (mesh == null)
		{
			Util.error(MODULE + " No mesh data " + name);
			return;
		}
			
		// Create VAO and load the mesh data into it
		vao = new VertexArrayObject();
		
		vao.load_mesh_data(mesh, shader);	
	}
	
	
	// Draw Geometry
	public void draw()
	{
		vao.draw();
	}
	
	
	// Return name
	public string get_name()
	{
		return name;
	}
	
}



