/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Material.cs
	
	Generic Textured Materials
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Material
{
	const string MODULE = "[Material]   ";
	
	private string name;			// Material name
	
	private int diffuse_map;		// Texture maps
	private int specular_map;
	private int normal_map;


	private float emissive_strength = 0.0f;		// Properties
	private float ambient_strength = 0.3f;		// Properties
	private float diffuse_strength = 1.3f;
	private float specular_strength = 0.7f;
	private float specular_exp = 32.0f;
	private Color4 color;
	private float tx_blend;
	
	//private float specular_exp

	
	// Load Material from config file
	public bool load_from_config(Queue<string> items)
	{
		name = items.Dequeue();

		Util.alert(MODULE + "Loading Material " + name);
	
		if (items.Count < 13)
		{
			Util.alert(MODULE + "Not enough material properties");
			return true;
		}
		
		string diffuse_file =  items.Dequeue();
		string specular_file =  items.Dequeue();
		string normal_file =  items.Dequeue();
		
		try
		{
			diffuse_map = load_texture(diffuse_file);
			specular_map = load_texture(specular_file);
			normal_map = load_texture(normal_file);
		}
		catch (Exception ex)
		{
			Util.error(MODULE + "Error loading textures: " + ex.Message);
			return true;
		}
				
		color             = Util.get_color(items); 
		tx_blend          = Util.get_float(items);
		emissive_strength  = Util.get_float(items);
		ambient_strength  = Util.get_float(items);
		diffuse_strength  = Util.get_float(items);
		specular_strength = Util.get_float(items);
		specular_exp      = Util.get_float(items);

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
	
	
	// Set material specific uniforms
	public void set_uniforms(Shader shader)
	{
		shader.set_uniform(shader.uEmissiveStrength, emissive_strength);		
		shader.set_uniform(shader.uAmbientStrength, ambient_strength);		
		shader.set_uniform(shader.uDiffuseStrength, diffuse_strength);		
		shader.set_uniform(shader.uSpecularStrength, specular_strength);		
		shader.set_uniform(shader.uSpecularExp, specular_exp);				
				
		shader.set_uniform(shader.uObjectColor, color);		
		shader.set_uniform(shader.uTextureBlend, tx_blend);	

		// Assign textures
		GL.ActiveTexture(TextureUnit.Texture0 + diffuse_map);
		GL.BindTexture(TextureTarget.Texture2D, diffuse_map);
		shader.set_uniform(shader.uDiffuseTexture, diffuse_map);	

		GL.ActiveTexture(TextureUnit.Texture0 + specular_map);
		GL.BindTexture(TextureTarget.Texture2D, specular_map);
		shader.set_uniform(shader.uSpecularTexture, specular_map);	

		//GL.ActiveTexture(TextureUnit.Texture0 + normal_map);
		//GL.BindTexture(TextureTarget.Texture2D, normal_map);
		//shader.set_uniform(shader.uNormalTexture, normal_map);	
	}
		
	public static int load_texture(string filename)
	{
		//Util.alert(MODULE + "  Loading Texture " + filename);
		
		TextureTarget Target = TextureTarget.Texture2D;

		int texture;
		GL.GenTextures(1, out texture);
		GL.BindTexture(Target, texture);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
		GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
		
		GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

		GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
		GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

		//Util.alert(MODULE + " Reading Data");

		try
		{
			Bitmap bitmap = new Bitmap(filename);
			BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			GL.Finish();
			bitmap.UnlockBits(data);
		}
		catch(Exception ex)
		{
			throw new FileNotFoundException("Texture Unable to read: " + ex.Message); //return;
		}
		
		if (GL.GetError() != ErrorCode.NoError)
			throw new Exception("Error loading texture " + filename);

		//Util.alert(MODULE + "  Texture ID " + texture);

		return texture;
	}
	
	public string get_name()
	{
		return name;
	}
	
}



