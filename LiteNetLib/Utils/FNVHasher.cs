using System.Collections.Generic;

namespace LiteNetLib.Utils
{
	public sealed class FNVHasher : NetSerializerHasher
	{
		private readonly Dictionary<string, ulong> _hashCache = new Dictionary<string, ulong>();

		private readonly char[] _hashBuffer = new char[1024];

		public override ulong GetHash(string type)
		{
            ulong hash;
            if (_hashCache.TryGetValue(type, out hash))
			{
				return hash;
			}
			hash = 14695981039346656037uL;
			int len = type.Length;
			type.CopyTo(0, _hashBuffer, 0, len);
			for (int i = 0; i < len; i++)
			{
				hash ^= _hashBuffer[i];
				hash *= 1099511628211L;
			}
			_hashCache.Add(type, hash);
			return hash;
		}

		public override ulong ReadHash(NetDataReader reader)
		{
			return reader.GetULong();
		}

		public override void WriteHash(ulong hash, NetDataWriter writer)
		{
			writer.Put(hash);
		}
	}
}
