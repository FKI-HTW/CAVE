using System.Collections.Concurrent;

namespace HTW.CAVE.SimpleTcp
{
	public abstract class SimpleTcpEndpoint
	{
		public ConcurrentQueue<SimpleTcpMessage> messageQueue;
		
		public SimpleTcpEndpoint()
		{
			messageQueue = new ConcurrentQueue<SimpleTcpMessage>();
		}
		
		public abstract bool SendMessage(SimpleTcpMessage message);
	}
}
