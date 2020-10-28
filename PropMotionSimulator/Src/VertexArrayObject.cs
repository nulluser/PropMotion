/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: VertexArrayObject
*/

using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using STL_Tools;


// Vertex element
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct Vertex
{
	public Vector3 Position;
	public Vector3 Normal;
	public Vector2 Txpos;
	//public uint Color;

	public Vertex(float x, float y, float z, 
					float nx, float ny, float nz, 
					float tx, float ty)
					//Color color)
	{
		Position = new Vector3(x, y, z);
		Normal = new Vector3(nx, ny, nz);
		Txpos = new Vector2(tx, ty);
		//Color = ToRgba(color);
	}
	
	public Vertex(Vector3 p, Vector3 n, Vector2 t)
	{
		Position = new Vector3(p.X, p.Y, p.Z);
		Normal = new Vector3(n.X, n.Y, n.Z);
		Txpos = new Vector2(t.X, t.Y);
		//Color = ToRgba(color);
	}

	//static uint ToRgba(Color color)
	////{
	//	return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
	//}
}

public class VertexArrayObject
{
	const string MODULE = "[VAO]        ";
	
	private int vao_id;			// Vertex Array Object
	private int vbo_id;			// Vertex Buffer Object
	private int num_vertices;	// Number of elements to draw


	// Load data from triangle mesh using provided shader
	public void load_mesh_data(STL_Tools.TriangleMesh[] mesh, Shader shader)
	{
		Vertex[]  vertices; // Todo, can this be local? What happens if it is garbage collected
		
		// Allocate vertex array
		vertices = new Vertex[mesh.Length*3];
	
		//Color color =  Color.FromArgb(0, 128, 128);
		//var rand = new Random();
		
		// TODO need to aboid recomputing data
		STLReader t = new STL_Tools.STLReader();
		Vector3 min =  t.GetMinMeshPosition(mesh);
		Vector3 max =  t.GetMaxMeshPosition(mesh);
		
		//Util.alert(MODULE + " mesh min: " + min.X + " " + min.Y + " " + min.Z);
		//Util.alert(MODULE + " mesh max: " + max.X + " " + max.Y + " " + max.Z);
		
		uint v_index = 0;
		
		float min_tx = 10000f;
		float max_tx = -10000f;
		
		float min_ty = 10000f;
		float max_ty = -10000f;
				
		for (int i = 0; i < mesh.Length; i++)
		{
			//byte b = (byte)(rand.Next()%255);
			//color = Color.FromArgb(b, b, b);
			
			float tx1 = (mesh[i].vert1.X - min.X) / (max.X - min.X);
			float ty1 = (mesh[i].vert1.Y - min.Y) / (max.Y - min.Y);
			float tx2 = (mesh[i].vert2.X - min.X) / (max.X - min.X);
			float ty2 = (mesh[i].vert2.Y - min.Y) / (max.Y - min.Y);
			float tx3 = (mesh[i].vert3.X - min.X) / (max.X - min.X);
			float ty3 = (mesh[i].vert3.Y - min.Y) / (max.Y - min.Y);
			
			if (tx1 < min_tx) min_tx = tx1;	if (tx1 > max_tx) max_tx = tx1;
			if (ty1 < min_ty) min_ty = ty1;	if (ty1 > max_ty) max_ty = ty1;
			if (tx2 < min_tx) min_tx = tx2;	if (tx2 > max_tx) max_tx = tx2;
			if (ty2 < min_ty) min_ty = ty2;	if (ty2 > max_ty) max_ty = ty2;
			if (tx3 < min_tx) min_tx = tx3;	if (tx3 > max_tx) max_tx = tx3;
			if (ty3 < min_ty) min_ty = ty3;	if (ty3 > max_ty) max_ty = ty3;
			
			
			Vector2 t1 = new Vector2((mesh[i].vert1.X - min.X) / (max.X - min.X), (mesh[i].vert1.Y - min.Y) / (max.Y - min.Y));
			Vector2 t2 = new Vector2((mesh[i].vert2.X - min.X) / (max.X - min.X), (mesh[i].vert2.Y - min.Y) / (max.Y - min.Y));
			Vector2 t3 = new Vector2((mesh[i].vert3.X - min.X) / (max.X - min.X), (mesh[i].vert3.Y - min.Y) / (max.Y - min.Y));
			
			vertices[v_index++] = new  Vertex(mesh[i].vert1, mesh[i].normal1, t1);
			vertices[v_index++] = new  Vertex(mesh[i].vert2, mesh[i].normal2, t2);
			vertices[v_index++] = new  Vertex(mesh[i].vert3, mesh[i].normal3, t3);
		}
		
		//Util.alert("tx min: " + min_tx + " " + min_ty);
		//Util.alert("tx max: " + max_tx + " " + max_ty);
		
		create_buffers(vertices, shader);
	}	


	private void create_buffers(Vertex[] vertices, Shader shader)
	{
		// Save number of vertices. Supplied data is temporary
		num_vertices = vertices.Length;
		
		// Compute stride between vertices
		int stride = BlittableValueType.StrideOf(vertices);
		
		// Create Vertex Array Object
		GL.GenVertexArrays(1, out vao_id);
		GL.BindVertexArray(vao_id);		
		
		// Create vertex buffer object
		GL.GenBuffers(1, out vbo_id);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_id);
		
		// Copy buffer data to GPU
		GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(num_vertices * stride), 
						vertices, BufferUsageHint.StaticDraw);
					  
		// Enable attributes
		GL.EnableVertexAttribArray(shader.aVertexPos);
		GL.EnableVertexAttribArray(shader.aVertexNormal);
		GL.EnableVertexAttribArray(shader.aVertexTxPos);

		// Configure attributes
		GL.VertexAttribPointer(shader.aVertexPos,    3, VertexAttribPointerType.Float, true, stride, 0);
		GL.VertexAttribPointer(shader.aVertexNormal, 3, VertexAttribPointerType.Float, true, stride, new IntPtr(12));
		GL.VertexAttribPointer(shader.aVertexTxPos,  2, VertexAttribPointerType.Float, true, stride, new IntPtr(24));

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
		
	// Draw the object
	public void draw()
	{
		GL.BindVertexArray(vao_id);
		GL.DrawArrays(PrimitiveType.Triangles, 0, num_vertices);
	}
}

