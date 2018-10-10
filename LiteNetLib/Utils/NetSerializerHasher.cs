namespace LiteNetLib.Utils
{
	public abstract class NetSerializerHasher
	{
		public abstract ulong GetHash(string type);

		public abstract void WriteHash(ulong hash, NetDataWriter writer);

		public abstract ulong ReadHash(NetDataReader reader);
	}
}
