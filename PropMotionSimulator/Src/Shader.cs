/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Shader.cs
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

// Shader Data
public class Shader
{
	const string MODULE = "[Shader]     ";	
	
	// TODO, should use accessors
	
	public const int MAX_LIGHTS = 16;
	
	// Shader programs
	public int vertex_shader_id, fragment_shader_id, shader_program;

	// Vertx Attr positions
	public	int aVertexPos, aVertexNormal, aVertexTxPos;
	
	// Uniform Locations
	public int uProjectionMatrix;
	public int uProjectionViewMatrix;
	public int uViewMatrix;
	public int uModelMatrix;

	// Uniform Locations
	public int uNormalMatrix;
	public int uCameraPosition;
	public int uEmissiveStrength, uAmbientStrength, uDiffuseStrength, uSpecularStrength, uSpecularExp;
	public int uObjectColor;
	public int uDrawMode;
	public int uTextureBlend;
	public int uDiffuseTexture;
	public int uSpecularTexture;
	public int uNormalTexture;
	public int uTestMode;	
	
	public int uTxOffset;
	public int uTyOffset;
	
	
	public int uNumLights = 0;									// Uniform Current number of light
	public int [] uLightSourcePos = new int[MAX_LIGHTS];		// 
	public int [] uLightSourceColor = new int[MAX_LIGHTS];		//
	

	// Load shaders from file
	public bool load_shaders(string vs_file, string fs_file)
	{
		Util.alert(MODULE + "Loading Shaders " + vs_file + " " + fs_file);

		string vs_prog = String.Empty, fs_prog = String.Empty;
	
		try
		{
			using (StreamReader vs = new StreamReader(vs_file))
				vs_prog = vs.ReadToEnd();

			using (StreamReader fs = new StreamReader(fs_file))
				fs_prog = fs.ReadToEnd();
		}
		catch (Exception ex)
		{
			Util.error("Unable to load shaders " + ex.Message);
			return true;
		}
		
		
		if (create_shaders(vs_prog, fs_prog))
		{
			Util.error("Unable to create shaders");
			return true;
		}		
		
		// Get attribute locations
		aVertexPos				= get_attribute("in_position");
		aVertexNormal			= get_attribute( "in_normal");
		aVertexTxPos			= get_attribute("in_tx_cord");
		
		// Get uniform locations
		//uProjectionMatrix		= get_uniform("projection_matrix");
		uProjectionViewMatrix	= get_uniform("projection_view_matrix");
		//uViewMatrix				= get_uniform("view_matrix");
		uModelMatrix			= get_uniform("model_matrix");
				
		uNormalMatrix			= get_uniform("normal_matrix");
		uCameraPosition			= get_uniform("camera_position");
		uObjectColor			= get_uniform("object_color");
		
		uEmissiveStrength		= get_uniform("emissive_strength");
		uAmbientStrength		= get_uniform("ambient_strength");
		uDiffuseStrength		= get_uniform("diffuse_strength");
		uSpecularStrength		= get_uniform("specular_strength");
		uSpecularExp			= get_uniform("specular_exp");
		//uDrawMode				= get_uniform("draw_mode");
		uTextureBlend			= get_uniform("texture_blend");
		
		uDiffuseTexture			= get_uniform("diffuse_map");
		uSpecularTexture		= get_uniform("specular_map");
		//uNormalTexture			= get_uniform("normal_map");
		
		//uTestMode				= get_uniform("test_mode");
		
		uTxOffset				= get_uniform("tx_offset");
		uTyOffset				= get_uniform("ty_offset");

		uNumLights				= get_uniform("num_lights");
		
		// Get uniforms for lights
		for (int i = 0; i < MAX_LIGHTS; i++)
		{
			uLightSourcePos[i]				= get_uniform("light_src["+i+"].position");
			uLightSourceColor[i]			= get_uniform("light_src["+i+"].color");
		}

		return false;
	}

	// Create shaders from shader program strings
	private bool create_shaders(string vs, string fs)
	{
		int status_code;
		string info;

		vertex_shader_id = GL.CreateShader(ShaderType.VertexShader);
		fragment_shader_id = GL.CreateShader(ShaderType.FragmentShader);

		// Compile vertex shader
		GL.ShaderSource(vertex_shader_id, vs);
		GL.CompileShader(vertex_shader_id);
		GL.GetShaderInfoLog(vertex_shader_id, out info);
		GL.GetShader(vertex_shader_id, ShaderParameter.CompileStatus, out status_code);

		if (status_code != 1)
		{
			Util.error(MODULE + "Unable to create vertex shader " + info);
			return true;
		}	

		// Compile vertex shader
		GL.ShaderSource(fragment_shader_id, fs);
		GL.CompileShader(fragment_shader_id);
		GL.GetShaderInfoLog(fragment_shader_id, out info);
		GL.GetShader(fragment_shader_id, ShaderParameter.CompileStatus, out status_code);
		
		if (status_code != 1)
		{
			Util.error(MODULE + "Unable to create fragment shader " + info);
			return true;
		}	

		shader_program = GL.CreateProgram();
		GL.AttachShader(shader_program, fragment_shader_id);
		GL.AttachShader(shader_program, vertex_shader_id);

		GL.LinkProgram(shader_program);
		GL.UseProgram(shader_program);
		
		return false;
	}

	public void use_shader()
	{
		GL.UseProgram(shader_program);
	}

	public void set_uniform(int index, Matrix3 m) { GL.UniformMatrix3(index, false, ref m); }
	public void set_uniform(int index, Matrix4 m) { GL.UniformMatrix4(index, false, ref m); }
	public void set_uniform(int index, Vector3 v) { GL.Uniform3(index, v.X, v.Y, v.Z ); }
	public void set_uniform(int index, Color4 v)  { GL.Uniform4(index, v.R, v.G, v.B, v.A ); }
	public void set_uniform(int index, float v)   { GL.Uniform1(index, v); }
	public void set_uniform(int index, int v)     { GL.Uniform1(index, v); }
	
	// Get uniform location
	public int get_uniform(string item)
	{
		int location = GL.GetUniformLocation(shader_program, item);
		
		if (location < 0)
		{
			Util.error(MODULE + "Unable to find position of uniform " + item + " " + location);
			return -1;
		}
		
		//Util.alert(MODULE + "Located " + item + " at " + location);
		
		return location;
	}	
		
	// Get attribute location
	public int get_attribute(string item)
	{
		int location = GL.GetAttribLocation(shader_program, item);
		
		if (location < 0)
		{
			Util.error(MODULE + "Unable to find position of attirbute " + item + " " + location);
			return -1;
		}
		
		//Util.alert(MODULE + "Located " + item + " at " + location);
		
		return location;
	}	
}
