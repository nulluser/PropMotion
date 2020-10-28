/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
*/

using System;					// System Core
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;



// 	Utility Functions 
public static class Util
{
	private const string MODULE = "[Util]      ";
	
	private const string LOG_PATH = @"log.txt";
	
	// Lock to aboid stomping on writes
	private static Object log_lock = new Object();	// Log lock
	
	private static bool log_started = false;
	
	private static bool console_enabled = true;
	
	
	public static void disable_console()
	{
		console_enabled = false;
	}
	
	// Return system time in milliseconds
	public static double get_time()
	{
		return (DateTime.Now.ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond) * 1000.0f;
	}
	
	// Console alert. Display and log
	public static void alert(string msg)
	{
		string s = "MSG " + msg;
		
		if (console_enabled)
		{
			lock(log_lock)
			{
				Console.WriteLine(s);
			}
		}
		
		log_write(s);
	}		
	
	// Error. Display and log 
	public static void error(string msg)
	{
		string s = "ERR " + msg;
		
		if (console_enabled)
		{
			lock(log_lock)
			{
				// Save old color
				ConsoleColor old_color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				
				Console.WriteLine(s);
				
				// Restore color
				Console.ForegroundColor = old_color;
			}
		}
		
		log_write(s);
	}	
	
	// Log to file
	public static void log(string msg)
	{
		log_write("LOG " + msg);
	}
	
	
	// Log to file with no header
	public static void log_write(string msg)
	{
		lock(log_lock)
		{
			// delete previous log file
			if (!log_started)
			{
				log_started = true;
				File.Delete(LOG_PATH); 
			}
			
			DateTime localDate = DateTime.Now;
		
			string timestamp = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff tt"); 
		
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(LOG_PATH, true))
			{
				file.WriteLine(timestamp + " " + msg);
			}		
		}
	}
	
	// Log some data
	/*public static void log_bytes(byte [] data)
	{
		string path = @"log.txt";
		
		if (!File.Exists(path)) 
		{
			StreamWriter sw = File.CreateText(path);
		}

		using (StreamWriter sw = File.AppendText(path)) 
		{
			string s = System.Text.Encoding.ASCII.GetString(data);
			sw.Write(s);
		}	
	}*/
	
	
	
	
	
	
	
	
	
	// Converty buffer to string
	public static string buffer_tostring( byte [] buffer )
	{
		string str = String.Empty;
		
		if (buffer == null)
			return "Null Buffer";
		
		str += "(" + buffer.Length + ") ";
	
		for (int i = 0; i < buffer.Length; i++)
			str += buffer[i].ToString("X2") + " " ;
			//str += "0x" + buffer[i].ToString("X2") + ", " ;
		
		return str;
	}
	
	
	// Display a Buffer
	public static void show_buffer( string header, byte [] buffer)
	{
		Util.alert(header + buffer_tostring(buffer));
	}
	
	// Display a Buffer
	public static void log_buffer( string header, byte [] buffer)
	{
		Util.log(header + buffer_tostring(buffer));
	}	
	
	
	
	
		// Get float vetor from config strings
	public static Vector3 get_vector(Queue<string> items)
	{
		Vector3 v;
		
		Single.TryParse(items.Dequeue(), out v.X);
		Single.TryParse(items.Dequeue(), out v.Y);
		Single.TryParse(items.Dequeue(), out v.Z);

		return v;
	}

	// Get axis config from config strings
	public static Axis get_axis(Queue<string> items, int axis)
	{
		Axis a = new Axis();
		int motion_idx;
		float min_pos, max_pos;
		float max_vel, max_acc;
		
		Int32.TryParse(items.Dequeue(),  out motion_idx);
		Single.TryParse(items.Dequeue(), out min_pos);
		Single.TryParse(items.Dequeue(), out max_pos);
		Single.TryParse(items.Dequeue(), out max_vel);
		Single.TryParse(items.Dequeue(), out max_acc);
		
		motion_idx -= 1; // indexes start at 1
		
		if (motion_idx<-1) motion_idx = -1;
		
		float home = (min_pos+max_pos) / 2;
		
		a.set_paremeter(motion_idx, axis, min_pos, max_pos, max_vel, max_acc, home);

		return a;
	}
	
	// Get axis config from config strings
	public static Color4 get_color(Queue<string> items)
	{
		byte r, g, b, a;
		Byte.TryParse(items.Dequeue(),  out r);
		Byte.TryParse(items.Dequeue(),  out g);
		Byte.TryParse(items.Dequeue(),  out b);
		Byte.TryParse(items.Dequeue(),  out a);
		return new Color4(r, g, b, a);
	}	
	
	
	// Get axis config from config strings
	public static Vector3 get_color3(Queue<string> items)
	{
		byte r, g, b;
		Byte.TryParse(items.Dequeue(),  out r);
		Byte.TryParse(items.Dequeue(),  out g);
		Byte.TryParse(items.Dequeue(),  out b);
		return new Vector3((float)r/255.0f, (float)g/255.0f, b/255.0f);
	}
	
		// Get axis config from config strings
	public static float get_float(Queue<string> items)
	{
		float v;
		Single.TryParse(items.Dequeue(),  out v);
		return v;
	}
	
		// Get axis config from config strings
	public static int get_int(Queue<string> items)
	{
		int v;
		Int32.TryParse(items.Dequeue(),  out v);
		return v;
	}
		
	
	
	// Get config String 
	public static string get_config_string(string module, string item)
	{
		string value = String.Empty;
	
		try
		{
			value = System.Configuration.ConfigurationManager.AppSettings[item];
		}
		catch(Exception ex)
		{
			error(module + "Unable to load configuration item (" + item + "): " + ex.Message);
		}
		
		return  value;
	}
	
	// Get config Integer 
	public static int get_config_int(string module, string item)
	{
		string str = get_config_string(module, item);
		
		if (str == String.Empty) return 0;
		
		int value = 0;
	
		try
		{
			value = Int32.Parse(str);
		}
		catch(Exception ex)
		{
			error(module + "Unable to load configuration item (" + item + "): " + ex.Message);
		}
		
		return value;
	}
	
	// Floating point map to range
	public static float mapf(float x, float in_min, float in_max, float out_min, float out_max)
	{
		if (x < in_min) x = in_min;
		if (x > in_max) x = in_max;
	 
		var v = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

		return v;
	}	
	
	
	// Floating point map to range
	public static float degtorad(float v)
	{
		return (float)(v * Math.PI / 180.0f);
	}	
	
		
		
		
	// Find item in list
	public static int find_item(string item_name, List<string> list)
	{
		// Spacial parent name for containers
		if (item_name == "none") 
			return -1;
		
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == item_name)
				return i;
		}
		
		return -1;
	}

		
	
	
	
	/*
	// Get short from buffer location 
	public static ushort get_ushort(byte [] buffer, byte index)
	{
		if (BitConverter.IsLittleEndian)
			return BitConverter.ToUInt16(new byte[2] {buffer[index+1], buffer[index]}, 0);
		else
			return BitConverter.ToUInt16(new byte[2] {buffer[index], buffer[index+1] }, 0);
	}
	
	// Get short from buffer location 
	public static short get_short(byte [] buffer, byte index)
	{
		if (BitConverter.IsLittleEndian)
			return BitConverter.ToInt16(new byte[2] {buffer[index+1], buffer[index]}, 0);
		else
			return BitConverter.ToInt16(new byte[2] {buffer[index], buffer[index+1] }, 0);
	}	
	
	
	// Helper to get next byte in buffer and update index
	public static byte get_next_byte(byte [] payload, ref byte idx)
	{
		byte v = payload[idx]; idx+=1;
		return payload[idx];
	}	
	
	// Helper to get next short in buffer and update index
	public static short get_next_short(byte [] payload, ref byte idx)
	{
		short v = get_short(payload, idx); idx+=2;
		return v;
	}
	
	// Helper to get next ushort in buffer and update index
	public static ushort get_next_ushort(byte [] payload, ref byte idx)
	{
		ushort v = get_ushort(payload, idx); idx+=2;
		return v;
	}		
	
	// Helper to get next short in buffer, update index, and return as float
	public static float get_next_float(byte [] payload, ref byte idx)
	{
		short v = get_short(payload, idx); idx+=2;
		return (float)v;
	}
	
	
	// Helper to set next byte in buffer and update index
	public static void set_next_byte(byte [] payload, byte v, ref byte idx)
	{
		payload[idx] = v; idx++;
	}	
	
	// Helper to set next short in buffer and update index
	public static void set_next_short(byte [] payload, short v, ref byte idx)
	{
		payload[idx] = hi_byte((ushort)v); idx++;
		payload[idx] = lo_byte((ushort)v); idx++;	
	}
	
	// Helper to set next ushort in buffer and update index
	public static void set_next_ushort(byte [] payload, ushort v, ref byte idx)
	{
		payload[idx] = hi_byte((ushort)v); idx++;
		payload[idx] = lo_byte((ushort)v); idx++;
	}		
	
	// Helper to set next short in buffer, update index, and return as float
	public static void set_next_float(byte [] payload, float v, ref byte idx)
	{
		
		payload[idx] = hi_byte((short)v); idx++;
		payload[idx] = lo_byte((short)v); idx++;
	}
	
	
	
	// Get lo byte of word  
	// May need to deal with byte order for other arch
	public static byte lo_byte(ushort val)
	{
		return (byte)(val & 0xff);
	}
	
	// Get high byte of word 
	// May need to deal with byte order for other arch
	public static byte hi_byte(ushort val)
	{
		return (byte)(val >> 8);
	}	
	
	// Get lo byte of word  
	// May need to deal with byte order for other arch
	public static byte lo_byte(short val)
	{
		return (byte)(val & 0xff);
	}
	
	// Get high byte of word 
	// May need to deal with byte order for other arch
	public static byte hi_byte(short val)
	{
		return (byte)(val >> 8);
	}	
	

	// True if system is linux
	public static bool is_linux()
	{
		OperatingSystem os = Environment.OSVersion;
		PlatformID     pid = os.Platform;

		if (pid == PlatformID.Unix) return true;

		return false;
	}
	*/

	
}

