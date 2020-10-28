/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Serial.cs
*/

using System;
using System.Text;
using System.IO.Ports;
using System.Collections.Concurrent;

// Serial Packet Driver
class Serial
{
	private const string MODULE = "[Serial]     ";		// Module name

	/* Serial related */
	private const int READ_TIMEOUT = 1000;				// Serial read timeout
	private const int WRITE_TIMEOUT = 1000;				// Serial write timeout

	private const char SYNC_CHAR = 'x';					// TODO, Move to config
	private const string SYNC_HEADER = "<!*$";			// TODO, Move to config

	/* Data */
	private SerialPort serial_port = null;				// Actual serial port

	private int updates;
	private int update_rate;
	
	private int packet_start;
	private int packet_length;
	
	private System.Timers.Timer second_timer;
	
	// Serial Constructor 
	public Serial()
	{
	}
	
	// Start Serial Port 
	public void start(string comm_port, int baud_rate, int serial_packet_start, int serial_packet_length)
	{
		Util.alert(MODULE + "Starting. Port: " + comm_port + " Baud: " + baud_rate + " Start: " + serial_packet_start +  " Length: " + serial_packet_length);

		packet_start = serial_packet_start;
		packet_length = serial_packet_length;
		
		// Check baud rate
		if (comm_port == String.Empty)
		{
			Util.error(MODULE + "Port name invalid");
			return;
		}

		// Check baud rate
		if (baud_rate == 0)
		{
			Util.error(MODULE + "Baud rate invalid");
			return;
		}
		
		// Create the port
		serial_port = new SerialPort(comm_port, baud_rate, Parity.None, 8, StopBits.One);

		// Read that DataReceived may need to be after open()
		serial_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serial_data_callback);
		
		serial_port.Encoding = Encoding.GetEncoding("utf-8");
        serial_port.Handshake = Handshake.None;
		serial_port.ReadTimeout = READ_TIMEOUT;
		serial_port.WriteTimeout = WRITE_TIMEOUT;
		
		try 
		{
			serial_port.Open();
		}
		catch(Exception ex)
		{
			Util.error(MODULE + "Unable to open serial port " + comm_port + ": " + ex.Message);
			//running = false;
			return;
		}
		
		Util.alert(MODULE + "Started");
		
		second_timer = new System.Timers.Timer(1000);
		second_timer.Elapsed += second_pulse;
		second_timer.AutoReset = true;
		second_timer.Enabled = true;
	}

	// Stop Serial Port 
	public void stop()
	{
		Util.alert(MODULE + "Stopping");
		
		// Close port
		if (serial_port != null)
			serial_port.Close();	
		
		Util.alert(MODULE + "Stopped");
	}
	
	// Called once a second for debugging
	private void second_pulse(Object source, System.Timers.ElapsedEventArgs e) 
	//public void second_pulse()
	{
		// Record updates
		//rx_rate = rx_bytes; //tx_rate = tx_bytes;
		update_rate = updates;
		
		Util.alert(MODULE + "update rate: " + updates);
		
		// Clear byte counters
		//tx_bytes = 0; 
		//rx_bytes = 0;
		updates = 0;
	}
	
	// Return stat string
	/*public string stat_string()
	{
		return String.Format("Rate: {0,5} RX: {1,5}", upadte_rate);
	}*/
	
	
	/*
		Primitives 
	*/
	
	// Read a buffer with timeout 
	private byte [] read_buffer(int length)
	{
		// Port is invalid
		if (serial_port == null) return null;
		
		// Make sure port is open
		if (!serial_port.IsOpen) return null;
		
		byte [] data = null;
		
		int num_bytes = 0;
		try 
		{
			data = new byte[length];
			num_bytes = serial_port.Read(data, 0, length);
		}
		catch (Exception ex)
		{
			Util.alert("Read Buffer: Timeout Waiting for Data " + ex.Message + " Recv: " + num_bytes);
			return null;
		}
		
		//rx_bytes += num_bytes;
		
		return data;
	}
	
	// Write buffer 
	private bool write_buffer(byte [] data)	
	{
		// Check for no data
		if (data == null) return true;
		
		// Port is invalid
		if (serial_port == null) return true;

		// Make sure port is open
		if (!serial_port.IsOpen) return true;
		
		try
		{
			serial_port.Write(data, 0, data.Length);
		}
		catch (Exception)
		{
			//Util.alert(MODULE + "TX Write Failure. " + ex.Message);
			return true;
		}	

		// Keep track of how much we sent
		//tx_bytes += data.Length;
		
		return false;
	}

	/*
		End of Primitives 
	*/
	
	
	/*
		Receive data processing 
	*/
	
	// Called when data arrives at port. Read entire packet 
	// Needs to be non blocking
	private void serial_data_callback(object sender, SerialDataReceivedEventArgs e)
	{
		//Util.alert("Sending data");
		
		// Port is invalid
		if (serial_port == null) return;
		
		// Port closed
		if (!serial_port.IsOpen) return;
		
		// Ignore EoF event
		if (e.EventType == SerialData.Eof) return;

		// Read the buffer
		byte [] rx_buffer = read_buffer(serial_port.BytesToRead);

		//if (update_rate< 10)
		//	Console.Write("Data: " + rx_buffer[0]);

		if (rx_buffer[0] != SYNC_CHAR) return;
	
		// Send header
		serial_port.Write(SYNC_HEADER);

		byte [] motion_data = Server.get_control_data();
		
		byte [] tx_packet = new byte[packet_length];
	
		// Reserved for remapping
		for (int i = 0; i < packet_length; i++)
		{
			tx_packet[i] = motion_data[i + packet_start];
			//Console.Write(tx_packet[i].ToString("X2"));
		}
		
		// Send the packet out
		if (write_buffer(tx_packet)) 
		{
			//Util.alert(MODULE + "Packet write failed");
			return;
		}
		
		updates++;
	}	
	
}



