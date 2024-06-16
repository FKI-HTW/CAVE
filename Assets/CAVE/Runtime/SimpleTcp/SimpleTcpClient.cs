using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace HTW.CAVE.SimpleTcp
{
	public class SimpleTcpClient : SimpleTcpEndpoint
	{
		private TcpClient m_Client;
		
		private Thread m_Thread;
		
		private NetworkStream m_ClientStream;
		
		private volatile bool m_Listen;
		
		public SimpleTcpClient() : base()
		{
		}
		
		public bool Connect(string host, int port)
		{
			try
			{
				m_Client = new TcpClient();
				m_Client.Connect(host, port);
				m_ClientStream = m_Client.GetStream();
			} catch(SocketException) {
				m_Client = null;
				return false;
			}
			
			m_Listen = true;
			m_Thread = new Thread(ListenToServer);
			m_Thread.IsBackground = true;
			m_Thread.Start();
			
			return true;
		}
		
		public bool IsConnected() => m_Client != null;
		
		public void Stop()
		{
			m_Listen = false;
			
			if (m_Thread != null)
			{
				m_Thread.Join();
				m_Thread = null;
			}
		}
		
		public override bool SendMessage(SimpleTcpMessage message)
		{
			if(m_ClientStream == null)
				return false;
				
			m_ClientStream.SendMessage(message.type, message.bytes);
			
			return true;
		}
		
		private void ListenToServer()
		{
			while (m_Listen)
			{		
				try
				{
					var status = m_Client.GetClientStatus();
					
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
					m_Listen = false;
				}
			}
			
			m_ClientStream.Close();
			m_ClientStream = null;
			m_Client.Close();
			m_Client = null;
		}
	}
}
