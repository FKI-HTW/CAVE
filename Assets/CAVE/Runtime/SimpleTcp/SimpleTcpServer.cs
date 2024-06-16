using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HTW.CAVE.SimpleTcp
{
	public class SimpleTcpServer : SimpleTcpEndpoint
	{		
		private TcpListener m_Listener;
		
		private Thread m_Thread;
		
		private volatile bool m_Listen;
		
		private NetworkStream m_ClientStream;
		
		public SimpleTcpServer() : base()
		{
		}
		
		public bool Listen(int port)
		{
			try
			{
				m_Listener = new TcpListener(IPAddress.Loopback, port);
				m_Listener.Start();
			} catch (SocketException) {
				return false;
			}
			
			m_Listen = true;
			m_Thread = new Thread(ListenIncomingConnections);
			m_Thread.IsBackground = true;
			m_Thread.Start();
			
			return true;
		}
		
		public void Stop()
		{
			m_Listen = false;
			
			if (m_Thread != null)
			{
				m_Thread.Join();
				m_Thread = null;
			}
			
			if (m_Listener != null)
			{
				m_Listener.Stop();
				m_Listener = null;
			}
		}
		
		public override bool SendMessage(SimpleTcpMessage message)
		{
			if (m_ClientStream == null)
				return false;
				
			m_ClientStream.SendMessage(message.type, message.bytes);
			
			return true;
		}
		
		private void ListenIncomingConnections()
		{
			while (m_Listen)
			{
				while (!m_Listener.Pending() && m_Listen)
					Thread.Sleep(20);
					
				if (!m_Listen)
					break;
			
				var client = m_Listener.AcceptTcpClient();
				
				m_ClientStream = client.GetStream();
								
				ListenToClient(client);
				
				m_ClientStream.Close();
				m_ClientStream = null;
				
				client.Close();
			}
		}
		
		private void ListenToClient(TcpClient client)
		{
			while (m_Listen)
			{
				try
				{
					var status = client.GetClientStatus();
					
					if (status == TcpClientStatus.Disconnect)
						return;
				
					if (status == TcpClientStatus.DataAvailable)
					{
						var type = m_ClientStream.ReadMessage(out byte[] bytes);
						base.messageQueue.Enqueue(new SimpleTcpMessage(type, bytes));
					} else {
						Thread.Sleep(20);
					}
				} catch(IOException) {
					return;
				}
			}
		}
	}
}
