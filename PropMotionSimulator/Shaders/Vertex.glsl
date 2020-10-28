/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
*/

#version 430 core

precision highp float;

// In from Program
in vec3 in_position;  // convert to vec4
in vec3 in_normal;
in vec2 in_tx_cord;

// Out to Fragment Shader
out vec3 normal;
out vec3 frag_pos;
out vec2 tx_pos;

uniform mat4 model_matrix;
//uniform mat4 view_matrix;
//uniform mat4 projection_matrix;
uniform mat4 projection_view_matrix;
uniform mat3 normal_matrix;

void main(void)
{
	// Copy Texture Cord
	tx_pos = in_tx_cord;
	
	// Compute normal from normal matrix
	normal = normal_matrix * in_normal;

	// Vertex position homogeneous
	vec4 local_pos = vec4(in_position, 1.0);
	
	// Compute fragment position
	frag_pos = (model_matrix * local_pos).xyz;		// Frag pos in world space

	// Compute vertex position
	gl_Position = projection_view_matrix * model_matrix * local_pos;
}


