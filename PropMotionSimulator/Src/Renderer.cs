/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Renderer.cs
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

// OpgnGL
public class Renderer
{
	const string MODULE = "[Render]     ";

	private Matrix4 projection_matrix;	// Matrix
	
	private List<LightSrc> light_sources = new List<LightSrc>();// Light Sources
	private List<Texture> textures = new List<Texture>();		// Textures
	private List<Material> materials = new List<Material>();	// Materials 
	private List<Geometry> geometry = new List<Geometry>(); 	// Geometry Data

	Shader shader = new Shader();		// Shader Program
	Camera camera = new Camera();		// Camera
		
	private int test_mode = 0;			// Debugging
	private const int TEST_MODES = 3;
	private int count = 0;
	
	public Renderer()
	{
	}
	
	// Setup
	public bool init()
	{
		Util.alert(MODULE + "Init");
		Util.alert(MODULE + "GL Version: " + GL.GetString(StringName.Version));
		
		string vertex_shader_file = Util.get_config_string(MODULE, "vertex_shader");
		string fragment_shader_file =  Util.get_config_string(MODULE, "fragment_shader");
	
		if (load_shaders(vertex_shader_file, fragment_shader_file))
		{
			Util.alert("Unable to load shaders");
			return true;			
		}
		
		//
		//GL.ClearColor(bg_color);
		GL.Enable(EnableCap.DepthTest);
		GL.CullFace(CullFaceMode.Front);
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
		GL.Enable(EnableCap.Normalize); 
		return false;
		
	}

	
	// Link objects by name
	public void resolve_links(List<MotionObject> objects)
	{
		Util.alert(MODULE + "Resolving Links. Objects: " + objects.Count);		
		
		// Resolve Material objects by name
		List<string> mat_name_list = new List<string>();

		for (var i = 0; i < materials.Count; i++)
			mat_name_list.Add(materials[i].get_name());
		
		// Resolve parent index
		for (var i = 0; i < objects.Count; i++)
			objects[i].resolve_material(mat_name_list);
		
		/*for (var i = 0; i < objects.Count; i++)
		{
			Util.alert(MODULE + objects[i].get_name() + " " + objects[i].get_material());
		}*/
		
		// Create VBOs for loaded geometry
		for (int i = 0; i < geometry.Count; i++)
			geometry[i].create_buffer(shader);
		
		// Resolve Link Geometry to models
		List<string> geo_name_list = new List<string>();

		for (var i = 0; i < geometry.Count; i++)
			geo_name_list.Add(geometry[i].get_name());
		
		// Resolve parent index
		for (var i = 0; i < objects.Count; i++)
			objects[i].resolve_geometry(geo_name_list);
	}


	// Unload objects
	public void unload()
	{
		if (shader.shader_program != 0)			GL.DeleteProgram(shader.shader_program);
		if (shader.fragment_shader_id != 0)		GL.DeleteShader(shader.fragment_shader_id);
		if (shader.vertex_shader_id != 0)		GL.DeleteShader(shader.vertex_shader_id);
	}
	
	public void resize(int width, int height, float aspect)
	{
		GL.Viewport(0, 0, width, height);

		// Create projection Matrix
		Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1, 250, out projection_matrix);
	}
	
	// Process updated motion data
	public void update(byte [] control_data, float ref_time)
	{
		
		for (int i = 0; i < light_sources.Count; i++)
			light_sources[i].update(control_data, ref_time);
	}

	// Render Frame
	public void render(List<MotionObject> objects)
	{
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		shader.set_uniform(shader.uCameraPosition, camera.get_position());		

		// Precompute projection view matrix so we don't have to do it in the shader
		Matrix4 projectionViewMatrix;
		Matrix4 viewMatrix = camera.get_view_matrix();

		Matrix4.Mult(ref viewMatrix, ref projection_matrix, out projectionViewMatrix);
		GL.UniformMatrix4(shader.uProjectionViewMatrix, false, ref projectionViewMatrix);
		
		// Set view matrix 
		//shader.set_uniform(shader.uViewMatrix, viewMatrix);

		// Set uniforms
		//shader.set_uniform(shader.uLightPosition1, light_position1);		
		
		
		for (int i = 0; i < light_sources.Count; i++)
			light_sources[i].set_uniforms(i, shader);
		
		shader.set_uniform(shader.uNumLights, light_sources.Count);		
				
		//shader.set_uniform(shader.uLightColor1, light_color1);		
	
		draw_objects(shader, objects);
	}
	
		
	// Draw all objects
	private void draw_objects(Shader shader, List<MotionObject> objects)
	{
		// Draw root objects
		for (int i = 0; i < objects.Count; i++)
		{
			// Start model matrix stack with identity, others are multiplied as draw level increases
			var root_model = Matrix4.Identity;
			
			int parent = objects[i].get_parent();
	
			// Start drawing at root level only
			if (parent == -1)
				draw_object(ref root_model, objects[i], objects);
		}
		
		count++;
	}
	
	// Draw object and all child objects, recursive
	private void draw_object(ref Matrix4 root_model, MotionObject o, List<MotionObject> objects)
	{
		// Need to save the root model matrix because we multiply by local model matrix
		// This root_model will be modified locally by the model matrix and passed down as modified to child objects
		// Todo, figure out how to make clone correctly 
		Matrix4 oldModelMatrix = root_model.ClearScale();
		
		//model_matrix_stack.Push(modelMatrix);
		Matrix4 object_model_matrix = o.get_matrix();
		
		// Copy current model matrix to shader
		Matrix4.Mult(ref object_model_matrix, ref root_model, out root_model);

		// Compute scaled model matrix. Need to do this so we don't chain the scale to children
		Matrix4 scaled = Matrix4.Identity;

		Matrix4 scale_mat = Matrix4.CreateScale(o.get_scale().X, o.get_scale().Y, o.get_scale().Z);			
		Matrix4.Mult(ref scale_mat, ref root_model, out scaled);		
	
		GL.UniformMatrix4(shader.uModelMatrix, false, ref scaled);	

		// Copmute normal natrix (inverse transpose of upper left 3x3 of model matrix)
		Matrix3 normal_matrix = Matrix3.Invert(Matrix3.Transpose(new Matrix3(root_model)));
		shader.set_uniform(shader.uNormalMatrix, normal_matrix);			
		
		// Set drawing parameters
		shader.set_uniform(shader.uTestMode, test_mode);	
		
		// Test scrolling
		//shader.set_uniform(shader.uTxOffset, count * 0.001f);	
		//if (o.get_name() == "skull" || o.get_name() == "jaw")
		shader.set_uniform(shader.uTyOffset, -count * 0.001f);	
		
		// Set matrial uniforms
		int material_idx = o.get_material();
		
		if (material_idx >= 0)
			materials[material_idx].set_uniforms(shader);

		// Override object color if it is a light
		if (o.get_light_enabled())
			shader.set_uniform(shader.uObjectColor, o.get_light_color());	

		// Draw the mesh
		int geometry_idx = o.get_geometry();
		
		if (geometry_idx >= 0)
			geometry[geometry_idx].draw();
				
		// Draw children
		List<int> list = o.get_child_list();
		
		for (int i = 0; i < list.Count; i++)
		{
			int idx = list[i];
			draw_object(ref root_model, objects[idx], objects);
		}
		
		// Restore previous so next is draw with correct starting model matrix
		root_model = oldModelMatrix;
	}
	
	
	public bool load_shaders(string vertex, string fragment)
	{
		return shader.load_shaders(vertex, fragment);
	}
	

	public void toggle_test_mode()
	{
		test_mode = (test_mode + 1) % TEST_MODES; Util.alert(MODULE + "Test Mode: " + test_mode); 
	}
	
	public void set_camera_pos(Vector3 pos)
	{
		camera.set_position(pos.X, pos.Y, pos.Z);
	}
	
	public void move_camera(float turn, float forward)
	{
		camera.move(turn, forward);
	}
	
	public void strafe_camera(float strafe)
	{
		camera.strafe(strafe);
	}

	public void set_background(Color4 color)
	{
		GL.ClearColor(color);
	}

	public void add_material(Material m) {materials.Add(m);}
	public void add_geometry(Geometry g) {geometry.Add(g);}
	public void add_texture(Texture t)   {textures.Add(t);}
	public void add_light_src(LightSrc l)   {light_sources.Add(l);}
		
	
}

