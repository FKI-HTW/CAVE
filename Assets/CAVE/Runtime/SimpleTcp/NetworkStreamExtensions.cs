using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HTW.CAVE.SimpleTcp
{
	public static class NetworkStreamExtensions
	{
		public static byte[] ReadNBytes(this NetworkStream networkStream, int n)
		{
			var buffer = new byte[n];
			var bytesRead = 0;

			while (bytesRead < n)
			{
				var bytes = networkStream.Read(buffer, bytesRead, buffer.Length - bytesRead);
				
				if(bytes == 0)
					throw new InvalidDataException("Missing transmitted bytes.");

				bytesRead += bytes;
			}

			return buffer;
		}
	
		public static int ReadMessage(this NetworkStream networkStream, out byte[] bytes)
		{
			var messagePackage = networkStream.ReadNBytes(Marshal.SizeOf<int>());
			var lengthPackage = networkStream.ReadNBytes(Marshal.SizeOf<int>());
			var message = BitConverter.ToInt32(messagePackage, 0);
			var length = BitConverter.ToInt32(lengthPackage, 0);

			if (length > 0)
				bytes = networkStream.ReadNBytes(length);
			else
				bytes = Array.Empty<byte>();
				
			return message;
		}
	
		public static void SendMessage(this NetworkStream networkStream, int message, byte[] bytes)
		{
			var messagePackage = BitConverter.GetBytes(message);
			var lengthPackage = BitConverter.GetBytes(bytes.Length);
			networkStream.Write(messagePackage, 0, messagePackage.Length);
			networkStream.Write(lengthPackage, 0, lengthPackage.Length);
			networkStream.Write(bytes, 0, bytes.Length);
			networkStream.Flush();
		}
	}
}
