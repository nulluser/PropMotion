/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
*/

#version 430 core

precision highp float;

// In from Vertex Shader
in vec3 normal;
in vec3 frag_pos;
in vec2 tx_pos;

// Out to next Stage
out vec4 frag_color;


struct LightSource
{
	vec3 position;
	vec3 color;
};

uniform LightSource light_src[16];
uniform int num_lights;


uniform mat3 normal_matrix;

uniform vec3 camera_position;
//uniform vec3 light_position1;
//uniform vec3 light_color1;
uniform vec4 object_color;

uniform float emissive_strength;
uniform float ambient_strength;
uniform float diffuse_strength;
uniform float specular_strength;
uniform float specular_exp;

uniform sampler2D diffuse_map;
uniform sampler2D specular_map;
uniform sampler2D normal_map;


//uniform int   draw_mode;
uniform float  texture_blend; // 0: color from object, 1: color from texture

uniform float tx_offset;
uniform float ty_offset;

uniform int test_mode;

void main(void)
{
	vec3 norm = normalize(normal);

	// Half ass normal map
	//if (test_mode == 1)
		//norm =  (norm + texture2D(normal_map, tx_pos).xyz * 2 - 1) / 2.0;


	// Take samples
	vec4 diffuse_sample  = texture2D(diffuse_map, vec2(tx_pos.x + tx_offset, tx_pos.y+ty_offset));
	vec4 spec_samp = texture2D(specular_map, tx_pos+vec2(+ty_offset, 0));
	

	// Compute surface color
	vec4 surface_color = texture_blend * diffuse_sample + (1-texture_blend) * object_color;
	
	// Compute specualtr weight
	float spec_val = (spec_samp.r + spec_samp.g + spec_samp.b) / 3.0;
	
	
	// View Direction
	vec3 view_dir = normalize(camera_position - frag_pos);
	
	
	vec3 emissive = vec3(emissive_strength);// * surface_color;
	
	vec3 ambient = vec3(0.0, 0.0, 0.0);
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	vec3 specular = vec3(0.0, 0.0, 0.0);
	
	// Compute lights
	for (int i = 0; i < num_lights; i++)
	{
		// Light direction
		vec3 light_dir = normalize(light_src[i].position - frag_pos);

		// ambient
		ambient += ambient_strength * light_src[i].color;
	
		// diffuse 
		float diff = max(dot(norm, light_dir), 0.0);
		diffuse += diffuse_strength * diff * light_src[i].color;

		// specular
		vec3 spec_half = normalize(light_dir + view_dir);
		float spec_fact = pow(max(dot(norm, spec_half), 0.0), specular_exp);
		
		specular += specular_strength * spec_val * spec_fact * light_src[i].color;  
	}
	
		
	vec3 result = (emissive + ambient + diffuse + specular) * surface_color.xyz;
	
	
	int draw_mode1 = 0;//test_mode;
	//int draw_mode1 = draw_mode;
	
	
	if (draw_mode1 == 0)
		frag_color = vec4(result, object_color.a);
	else
		frag_color = vec4(surface_color.xyz, 1.0);
	
	// Gamma correction
	float gamma = 2.2;
	frag_color.rgb = pow(frag_color.rgb, vec3(1.0/gamma));
}

// Standard opengl lighting:
//Vertex Color = emission + globalAmbient + sum(attenuation * spotlight *
//               [lightAmbient + (max {L.N, 0} * diffuse) + (max {H.N, 0} ^ shininess)*specular])


