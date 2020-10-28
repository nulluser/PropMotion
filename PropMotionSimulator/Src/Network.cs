/*
	Vixen Motion Simulator

	(C) 2020 nulluser@gmail.com
	
	File: Network.cs
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Device emulator
public class Network
{
	private const string MODULE = "[Network]    ";	
	private Thread thread_handle = null;			// Handle to main thread
	private bool running = false;					// True if thread running
	
	private const int UDP_PORT = 5568;				// Port 
	private const int MOTION_OFFSET = 126;			// Start position in packet for motion data
	private const int MAX_RX = 1024;				// Max RX bytes
	private const int RX_TIMEOUT = 2000;			// RX timeout in ms
	
	public Network()
	{
	}

	public void start()
	{	   
		Util.alert(MODULE + "Starting");
			
		running = true;
		
		thread_handle = new Thread(thread);
		thread_handle.Start();			
	}

	// Stop Network server
	public void stop()
	{
		Util.alert(MODULE + "Stopping");		
		
		running = false;
				
		if (thread_handle != null)
			thread_handle.Join();
		
		Util.alert(MODULE + "Stopped");					
	}

	// Loop and wait for client
	void thread()
	{
		byte[] data = new byte[MAX_RX];
		
		IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, UDP_PORT);
		
		UdpClient listen_socket;
	
		IPEndPoint client;	   
	   
		try 
		{
			listen_socket = new UdpClient(endpoint);
		}
		catch (Exception ex)
		{
			Util.alert(MODULE + "Unable to bind. In use? " + ex.Message);
			return;
		}

		client = new IPEndPoint(IPAddress.Any, 0);

		listen_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, RX_TIMEOUT);

		Util.alert(MODULE + "Waiting for client");
		
		while(running)
		{
			try
			{
				data = listen_socket.Receive(ref client);
			}
			catch (Exception)
			{
				//Util.alert("Waiting for client");
				continue;
				
			}
			
			 //Console.Write("Data: " + data.Length + " " );
			
			int control_data_len = data.Length - MOTION_OFFSET;;

			//if (data.Length >= MOTION_OFFSET + control_data_len)
			//Util.alert("control_data_len" + control_data_len);
			
			if (control_data_len > 0)
			{
				byte [] control_data = new byte[control_data_len];

				for (int i = 0; i < control_data_len; i++)
				{
					control_data[i] = data[i + MOTION_OFFSET];
				}

				/*for (int i = 0; i < control_data.Length; i++)
				{
					Console.Write(control_data[i].ToString("X2"));
					Console.Write(" ");
				}*/
				
				//Console.WriteLine();
				
				Server.update_control_data(control_data);
			}
		}
		
	}
}



