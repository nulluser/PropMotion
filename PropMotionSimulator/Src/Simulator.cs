/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Simulator.cs
	
	Handels Main OpenTK objects, axis simulation, and keyboard
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

// Simulator Core
public class Simulator : GameWindow
{
	const string MODULE = "[Sim]        ";
	
	private Thread main_thread;
	private bool exit = false;
	private object update_lock = new object();
	private bool viewport_changed = true;
	private int viewport_width, viewport_height;
	private int v_sync = 0;
		
	// OpenGL
	private Renderer renderer = null;
	
	private List<MotionObject> objects = new List<MotionObject>();// Model data

	private double ref_time;						// Time since start
	
	
	public Simulator(): base()
	{ 
		/// Load core config options
		
		lock (update_lock)
		{
			viewport_changed = true;
			//  viewport_width = Width;
			//    viewport_height = Height;
			
			v_sync = 0;//Util.get_config_int(MODULE, "display_vsync");// > 0 ? VSyncMode.On : VSyncMode.Off;
			viewport_width = 640;//Util.get_config_int(MODULE, "display_x");
			viewport_height = 480;//Util.get_config_int(MODULE, "display_y");
		}
		
		
		Resize += delegate(object sender, EventArgs e)
		{
			lock (update_lock)
			{
				viewport_changed = true;
				viewport_width = Width;
				viewport_height = Height;
			}
		};
			
		KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
		{
			//key_down(e.Key);
			key_down(e.Key);
			//if (e.Key == Key.Escape)
				//this.Exit();
		};
	}
	
	//// API Overrides
	
	// Called on load, init
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
						
		Context.MakeCurrent(null); // Release the OpenGL context so it can be used on the new thread.

		main_thread = new Thread(simulation_thread);
		main_thread.IsBackground = true;
		main_thread.Start();
	}


	protected override void OnUnload(EventArgs e)
	{
		renderer.unload();
		
		exit = true; // Set a flag that the rendering thread should stop running.
		main_thread.Join();
	}

	// Deal with keypresses
	void key_down(Key key)
	{
		if (key == Key.Space) Server.random_control_data();
		if (key == Key.Escape) this.Exit();
		
		if (key == Key.Q) Server.set_control_data(72, 1);
		if (key == Key.A) Server.set_control_data(72, 128);
		if (key == Key.Z) Server.set_control_data(72, 255);

		if (key == Key.W) Server.set_control_data(73, 255);
		if (key == Key.S) Server.set_control_data(73, 128);
		if (key == Key.X) Server.set_control_data(73, 1);

		if (key == Key.E) Server.set_control_data(74, 1);
		if (key == Key.D) Server.set_control_data(74, 128);
		if (key == Key.C) Server.set_control_data(74, 255);

		if (key == Key.R) Server.set_control_data(75, 1);
		if (key == Key.F) Server.set_control_data(75, 128);
		if (key == Key.V) Server.set_control_data(75, 255);

		if (key == Key.T) Server.set_control_data(76, 1);
		if (key == Key.G) Server.set_control_data(76, 128);
		if (key == Key.B) Server.set_control_data(76, 255);

		if (key == Key.Y) Server.set_control_data(77, 1);
		if (key == Key.H) Server.set_control_data(77, 128);
		if (key == Key.N) Server.set_control_data(77, 255);

		if (key == Key.U) Server.set_control_data(78, 1);
		if (key == Key.J) Server.set_control_data(78, 128);
		if (key == Key.M) Server.set_control_data(78, 255);

		if (key == Key.I) Server.set_control_data(79, 255);
		if (key == Key.K) Server.set_control_data(79, 128);
		if (key == Key.Comma) Server.set_control_data(79, 1);
	}	
	
	/*
		Internal
	*/
	
	/* 
		Called inside thread context 
	*/
	
	// Update simulation, called in render thread
	private void update(double t)
	{
		byte [] control_data = Server.get_control_data();
		
		renderer.update(control_data, (float)ref_time);
		
		// Pass motion data to all groups
		for (int i = 0; i < objects.Count; i++)
			objects[i].update(control_data, (float)ref_time);
	
		ref_time += t;//
	
		check_keys();
	}
	
	// Render, called in render thread
	private void render(double t)
	{
		lock (update_lock)
		{
			// Deal with dispaly changes
			if (viewport_changed)
			{
				float aspect = ClientSize.Width / (float)(ClientSize.Height);
				
				renderer.resize(viewport_width, viewport_height, aspect);

				viewport_changed = false;
			}
		}
		
		renderer.render(objects);
	}
	
	
	
	void simulation_thread()
	{
		MakeCurrent(); // The context now belongs to this thread. No other thread may use it!

		VSync = v_sync > 0 ? VSyncMode.On : VSyncMode.Off;

		renderer = new Renderer();
		
		if (renderer.init())
		{
			
			Util.alert(MODULE + "Unable to init Renderer");
			return;
		}
	
		process_config_file(Util.get_config_string(MODULE, "scene_config"));
		//parse_xml_config("DefaultScene.xml"); // Testing

		renderer.resolve_links(objects);
				
		Util.alert(MODULE + "Done ");
		
		simulation_loop();

		Context.MakeCurrent(null);	
	}
	
	// Core Loop
	void simulation_loop()
	{
		// Since we don't use OpenTK's timing mechanism, we need to keep time ourselves;
		Stopwatch render_watch = new Stopwatch();
		Stopwatch update_watch = new Stopwatch();
		
		update_watch.Start();
		render_watch.Start();
		
		while (!exit)
		{
			update(update_watch.Elapsed.TotalSeconds);
			update_watch.Reset();
			update_watch.Start();

			render(render_watch.Elapsed.TotalSeconds);
			render_watch.Reset(); //  Stopwatch may be inaccurate over larger intervals.
			render_watch.Start(); // Plus, timekeeping is easier if we always start counting from 0.

			SwapBuffers();
		}
	}
	
	
	//// Loading
	
	// Load XML config. Test
	private void parse_xml_config(string filename)
	{
		/*Util.alert(MODULE + "Loading config from: " + filename);
		
		XDocument config = new XDocument();
		
		try 
		{
			config = XDocument.Load(filename);
		}
		catch (Exception ex)
		{
			Util.error("Unable to load config: " + filename + " " + ex.Message);
			return;
		}
		
		//IEnumerable<XElement> childList = from el in config.Elements() select el;
		//foreach (XElement e in childList)
		//Console.WriteLine(e);

		//Console.WriteLine("Address: " + display[0].InnerText);
		
		Util.alert("DIsplayx: " + config.Root.Element("display_x").Value);*/
	}


	// Process Scene config file 
	private void process_config_file(string config_file)
	{
		if (string.IsNullOrEmpty(config_file)) 
		{
			Util.alert("No Scene Config File");
			return;
		}
		
		Util.alert(MODULE + "Loading scene data from " + config_file);
		
		// Process the lines
		try
		{
			foreach (string line in System.IO.File.ReadAllLines(config_file))
				process_config_line(line);
		}
		catch (Exception ex)
		{
			Util.error(MODULE + "Unable to open config " + config_file + " " + ex.Message);
			return;
		}
		
		resolve_links();

		Util.alert(MODULE + "Load Complete");	
	}
	
	// Process Scene config file line
	private void process_config_line(String line)
	{
		if (string.IsNullOrEmpty(line)) return;
				
		string[] items = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);//Split(' ');

		if (items.Length == 0) return;

		// Create a queue of the items 
		Queue<string> item_queue =  new Queue<string>(items);
		
		string cmd = item_queue.Dequeue();

		if (cmd.Length == 0) return;		// No length
		if (cmd[0] == '#' || cmd == "##") return;			// Comment

		// Parse Commands
		if (cmd == "display_x")
		{
			viewport_width = Util.get_int(item_queue);
			Util.alert(MODULE + "Display_x: " + viewport_width);
			viewport_changed = true;
			Width=viewport_width;
		} else
		if (cmd == "display_y")
		{
			viewport_height = Util.get_int(item_queue);
			Util.alert(MODULE + "Display_y: " + viewport_height);			
			viewport_changed = true;
			Height=viewport_height;
		} else
		if (cmd == "display_sync")
		{
			v_sync = Util.get_int(item_queue);
			Util.alert(MODULE + "V_sync: " + v_sync);			
			viewport_changed = true;
			VSync = v_sync > 0 ? VSyncMode.On : VSyncMode.Off;
		} else			
		if (cmd == "background")
		{
			if (items.Length < 4) { Util.alert("Background command expects 4	 parameters " + items.Length); return; }
			Color4 color = Util.get_color(item_queue); 
			renderer.set_background(color);
			
		} else
		// Parse Commands
		if (cmd == "camera")
		{
			if (items.Length < 4) { Util.alert("Camera command expects 4	 parameters " + items.Length); return; }
			Util.alert(MODULE + "Loading Camera " + items[1]);
			renderer.set_camera_pos(Util.get_vector(item_queue));
		} else
		if (cmd == "lightsrc")
		{
			LightSrc l = new LightSrc();
			if (!l.load_from_config(item_queue))
				renderer.add_light_src(l);
		} else
		if (cmd == "texture")
		{
			Texture t = new Texture();
			if (!t.load_from_config(item_queue))
				renderer.add_texture(t);
		} else
		if (cmd == "material")
		{
			Material m = new Material();
			if (!m.load_from_config(item_queue))
				renderer.add_material(m);
			
		} else
		if (cmd == "geometry")
		{
			Geometry g = new Geometry();
			if (!g.load_from_config(item_queue))
				renderer.add_geometry(g);
			
		} else
	
		// Parse Commands
		if (cmd == "model")
		{			
			MotionObject m = new MotionObject();		
				if (!m.load_from_model_config(item_queue))
				objects.Add(m);
		} else
		{
			Util.error(MODULE + "Unknown config command: " + cmd);
		}
	}
	
	// Resolve parent/child by name
	private void resolve_links()
	{
		Util.alert(MODULE + "Resolving Links");	
		
		List<string> name_list = new List<string>();

		for (var i = 0; i < objects.Count; i++)
			name_list.Add(objects[i].get_name());
		
		// Resolve parent index
		for (var i = 0; i < objects.Count; i++)
			objects[i].resolve_parent(name_list);
				
		// Nofify parts of their children
		for (var i = 0; i < objects.Count; i++)
		{			
			int parent_idx = objects[i].get_parent();
			if (parent_idx >= 0)
				objects[parent_idx].add_child(i);
		}
		
		/*Util.alert(MODULE + "Model Linkage");	
		
		for (var i = 0; i < objects.Count; i++)
		{
			string s = "  Model " + objects[i].get_name() + " Parent: " + objects[i].get_parent();
			
			List<int> child_list = objects[i].get_child_list();
			
			s += " Children: ";
			
			for (var j = 0; j < child_list.Count; j++)
				s += child_list[j] + " ";
			
			Util.alert(MODULE + s);	
		}*/
	}
	
	//// UI
	
	public void check_keys()
	{
		var keyboard = OpenTK.Input.Keyboard.GetState();
			
			
		if (keyboard[OpenTK.Input.Key.Space]) Server.random_control_data();		
		if (keyboard[OpenTK.Input.Key.Left]) renderer.move_camera(1f, 0f);
		if (keyboard[OpenTK.Input.Key.Right]) renderer.move_camera(-1f, 0f);
		if (keyboard[OpenTK.Input.Key.Up]) renderer.move_camera(0, 1f);
		if (keyboard[OpenTK.Input.Key.Down]) renderer.move_camera(0f, -1f);
		if (keyboard[OpenTK.Input.Key.Delete]) renderer.strafe_camera(1.0f);
		if (keyboard[OpenTK.Input.Key.PageDown]) renderer.strafe_camera(-1.0f);
	}
}

