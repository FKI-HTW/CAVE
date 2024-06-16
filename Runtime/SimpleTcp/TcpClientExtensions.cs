using System.Net.Sockets;

namespace HTW.CAVE.SimpleTcp
{
	public enum TcpClientStatus
	{
		Idle,
		Disconnect,
		DataAvailable
	}

	public static class TcpClientExtensions
	{
		public static TcpClientStatus GetClientStatus(this TcpClient client)
		{
			var socket = client.Client;
		
			bool status = socket.Poll(30, SelectMode.SelectRead);
			
			if (status)
			{
				if (socket.Available > 0)
					return TcpClientStatus.DataAvailable;
			
				return TcpClientStatus.Disconnect;
			}
			
			return TcpClientStatus.Idle;
		}
	}
}
