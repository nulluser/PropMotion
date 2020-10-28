/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: VixenMotion.cs
*/

using System;

// Main Program
public class Server
{
	const string MODULE = "[Main]       ";
	
	const string APP_NAME = "Prop Motion Simulator - 2020 nulluser@gmail.com";

	const int MAX_control_data = 512;		// TODO : Move to config
	const int MOTION_HOME = 128;			// TODO : Move to config
	
	private static Object control_data_lock = new Object();	// data lock
	private static byte [] control_data = new byte[MAX_control_data];
	
	private static int motion_updates = 0;
	private static int frame_updates = 0;
	private static float frame_rate_target = 60;	// TODO move to config

	//private static System.Timers.Timer second_timer;
	private static Network network = null;			// Vixen Output Emulator
	private static Serial serial = null;
	
	[STAThread]
	public static void Main()
	{
		Util.alert(MODULE + APP_NAME);
		
		string version = Util.get_config_string(MODULE, "server_version");
	
		if (version == String.Empty) 
		{
			Util.alert(MODULE + "No Version Set");
			version = "None";
		}	

		Util.alert(MODULE + "Version : " + version);
		
		// Center motion
		for (int i = 0; i < MAX_control_data; i++)
			control_data[i] = MOTION_HOME;
		
		// Network server
		network = new Network();
		network.start();

		// Serial Port
		string serial_port = Util.get_config_string(MODULE, "serial_port");
		int serial_baud = Util.get_config_int(MODULE, "serial_baud");
		int packet_start = Util.get_config_int(MODULE, "serial_packet_start");
		int packet_length = Util.get_config_int(MODULE, "serial_packet_length");
				
		serial = new Serial();
		serial.start(serial_port, serial_baud, packet_start, packet_length);						
		
		setup_timer();
		
		
		Simulator simulation = new Simulator();
		// Start simulator and pass control
		//using (Simulator simulation = new Simulator())
		{
			//second_timer.Enabled = true;
			simulation.Title = APP_NAME;
			simulation.Run(frame_rate_target, 0.0);
			//second_timer.Enabled = false;
		}
		
		
		network.stop(); 
		serial.stop();
		
		Util.alert(MODULE + "Main Exit");
	}

	// Tell server we updated a frame
	public static void update_frame_data()
	{
		frame_updates++;
	}
	
	// Process motion data from network
	public static void update_control_data(byte[] data)
	{	
		lock(control_data_lock)
		{
			motion_updates++;
			
			for (int i = 0; i < data.Length; i++)
				control_data[i] = data[i];
		}
	}	
	
	
	// Process motion data from network
	public static void random_control_data()
	{	
		lock(control_data_lock)
		{
			Random r = new Random();
			
			for (int i = 0; i < control_data.Length; i++)
				control_data[i] = (byte)(r.Next() % 255);
		}
	}		
	
	// Retur copy of motion data
	public static byte [] get_control_data()
	{
		byte [] data = new byte[control_data.Length];
		
		lock (control_data_lock)
		{
			for (int i = 0; i < control_data.Length; i++)
			{
				data[i] = control_data[i];
				//Console.Write(control_data[i] + " " );
			}
			
			//Console.WriteLine();
		}
		
		return data;
	}
	
	public static void set_control_data(byte axis, byte val)
	{
		lock (control_data_lock)
		{
			control_data[axis] = val;
	
			//Console.WriteLine();
		}
		
	}	
	
	// Timer creation 
	private static void setup_timer()
	{
		// Todo, hangs, replace with threading
		/*second_timer = new System.Timers.Timer(1000);
		second_timer.Elapsed +=timer_event;
		second_timer.AutoReset = true;
		second_timer.Enabled = false;*/
	}
	
	// Timer handler 
	private static void timer_event(Object source, System.Timers.ElapsedEventArgs e) 
	{
		//Util.alert(MODULE + "Frame Updates " + frame_updates + " Motion Updates: " + motion_updates + " Ser);
		//motion_updates = 0;
		//frame_updates = 0;
		//simulation.second_pulse();
	}	
}

