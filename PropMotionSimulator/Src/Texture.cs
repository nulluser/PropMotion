/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Texture.cs
	
	Generic Textures
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class Texture
{
	const string MODULE = "[Texture]    ";
	
	private string name;			// TExture name
	private int texture_id;			// GL Texture ID

	
	// Load Material from config file
	public bool load_from_config(Queue<string> items)
	{
		name = items.Dequeue();

		Util.alert(MODULE + "Loading Texture " + name);
	
		if (items.Count < 1)
		{
			Util.alert(MODULE + "Not enough texture properties " + items.Count);
			return true;
		}
		
		string texture_file =  items.Dequeue();
		
		try
		{
			texture_id = load_texture(texture_file);
		}
		catch (Exception ex)
		{
			Util.error(MODULE + "Error loading textures: " + ex.Message);
			return true;
		}

		return false;
	}
	
	
	// Set material specific uniforms
	public void set_uniforms(Shader shader)
	{
		// Assign textures
		GL.ActiveTexture(TextureUnit.Texture0 + texture_id);
		GL.BindTexture(TextureTarget.Texture2D, texture_id);
		shader.set_uniform(shader.uDiffuseTexture, texture_id);	
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



