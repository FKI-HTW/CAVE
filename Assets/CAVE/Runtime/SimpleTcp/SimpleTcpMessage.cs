using System;
using System.Text;

namespace HTW.CAVE.SimpleTcp
{
	public struct SimpleTcpMessage
	{
		public int type;
	
		public byte[] bytes;
		
		public SimpleTcpMessage(int type, byte[] bytes)
		{
			this.type = type;
			this.bytes = bytes;
		}

		public SimpleTcpMessage(int type, string text)
		{
			this.type = type;
			this.bytes = Encoding.UTF8.GetBytes(text);
		}

		public SimpleTcpMessage(int type, int value)
		{
			this.type = type;
			this.bytes = BitConverter.GetBytes(value);
		}

		public SimpleTcpMessage(int type, bool value) : this(type, value ? 1 : 0)
		{
		}
		
		public SimpleTcpMessage(int type) : this(type, Array.Empty<byte>())
		{
		}
		
		public string GetString() => Encoding.UTF8.GetString(bytes);

		public int GetInt() => BitConverter.ToInt32(bytes, 0);

		public bool GetBool() => GetInt() > 0;
	}
}
