using System;
using System.Text;

namespace LiteNetLib.Utils
{
	public class NetDataReader
	{
		protected byte[] _data;

		protected int _position;

		protected int _dataSize;

		public byte[] Data => _data;

		public int Position => _position;

		public bool EndOfData => _position == _dataSize;

		public int AvailableBytes => _dataSize - _position;

		public void SetSource(NetDataWriter dataWriter)
		{
			_data = dataWriter.Data;
			_position = 0;
			_dataSize = dataWriter.Length;
		}

		public void SetSource(byte[] source)
		{
			_data = source;
			_position = 0;
			_dataSize = source.Length;
		}

		public void SetSource(byte[] source, int offset)
		{
			_data = source;
			_position = offset;
			_dataSize = source.Length;
		}

		public void SetSource(byte[] source, int offset, int dataSize)
		{
			_data = source;
			_position = offset;
			_dataSize = dataSize;
		}

		/// <summary>
		/// Clone NetDataReader without data copy (usable for OnReceive)
		/// </summary>
		/// <returns>new NetDataReader instance</returns>
		public NetDataReader Clone()
		{
			return new NetDataReader(_data, _position, _dataSize);
		}

		public NetDataReader()
		{
		}

		public NetDataReader(byte[] source)
		{
			SetSource(source);
		}

		public NetDataReader(byte[] source, int offset)
		{
			SetSource(source, offset);
		}

		public NetDataReader(byte[] source, int offset, int maxSize)
		{
			SetSource(source, offset, maxSize);
		}

		public NetEndPoint GetNetEndPoint()
		{
			string @string = GetString(1000);
			int port = GetInt();
			return new NetEndPoint(@string, port);
		}

		public byte GetByte()
		{
			byte result = _data[_position];
			_position++;
			return result;
		}

		public sbyte GetSByte()
		{
			sbyte result = (sbyte)_data[_position];
			_position++;
			return result;
		}

		public bool[] GetBoolArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			bool[] arr = new bool[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetBool();
			}
			return arr;
		}

		public ushort[] GetUShortArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			ushort[] arr = new ushort[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetUShort();
			}
			return arr;
		}

		public short[] GetShortArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			short[] arr = new short[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetShort();
			}
			return arr;
		}

		public long[] GetLongArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			long[] arr = new long[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetLong();
			}
			return arr;
		}

		public ulong[] GetULongArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			ulong[] arr = new ulong[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetULong();
			}
			return arr;
		}

		public int[] GetIntArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			int[] arr = new int[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetInt();
			}
			return arr;
		}

		public uint[] GetUIntArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			uint[] arr = new uint[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetUInt();
			}
			return arr;
		}

		public float[] GetFloatArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			float[] arr = new float[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetFloat();
			}
			return arr;
		}

		public double[] GetDoubleArray()
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			double[] arr = new double[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetDouble();
			}
			return arr;
		}

		public string[] GetStringArray(int maxLength)
		{
			ushort size = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			string[] arr = new string[size];
			for (int i = 0; i < size; i++)
			{
				arr[i] = GetString(maxLength);
			}
			return arr;
		}

		public bool GetBool()
		{
			bool result = _data[_position] > 0;
			_position++;
			return result;
		}

		public char GetChar()
		{
			char result = BitConverter.ToChar(_data, _position);
			_position += 2;
			return result;
		}

		public ushort GetUShort()
		{
			ushort result = BitConverter.ToUInt16(_data, _position);
			_position += 2;
			return result;
		}

		public short GetShort()
		{
			short result = BitConverter.ToInt16(_data, _position);
			_position += 2;
			return result;
		}

		public long GetLong()
		{
			long result = BitConverter.ToInt64(_data, _position);
			_position += 8;
			return result;
		}

		public ulong GetULong()
		{
			ulong result = BitConverter.ToUInt64(_data, _position);
			_position += 8;
			return result;
		}

		public int GetInt()
		{
			int result = BitConverter.ToInt32(_data, _position);
			_position += 4;
			return result;
		}

		public uint GetUInt()
		{
			uint result = BitConverter.ToUInt32(_data, _position);
			_position += 4;
			return result;
		}

		public float GetFloat()
		{
			float result = BitConverter.ToSingle(_data, _position);
			_position += 4;
			return result;
		}

		public double GetDouble()
		{
			double result = BitConverter.ToDouble(_data, _position);
			_position += 8;
			return result;
		}

		public string GetString(int maxLength)
		{
			int bytesCount = GetInt();
			if (bytesCount > 0 && bytesCount <= maxLength * 2)
			{
				if (Encoding.UTF8.GetCharCount(_data, _position, bytesCount) > maxLength)
				{
					return string.Empty;
				}
				string @string = Encoding.UTF8.GetString(_data, _position, bytesCount);
				_position += bytesCount;
				return @string;
			}
			return string.Empty;
		}

		public string GetString()
		{
			int bytesCount = GetInt();
			if (bytesCount <= 0)
			{
				return string.Empty;
			}
			string @string = Encoding.UTF8.GetString(_data, _position, bytesCount);
			_position += bytesCount;
			return @string;
		}

		public byte[] GetRemainingBytes()
		{
			byte[] outgoingData = new byte[AvailableBytes];
			Buffer.BlockCopy(_data, _position, outgoingData, 0, AvailableBytes);
			_position = _data.Length;
			return outgoingData;
		}

		public void GetRemainingBytes(byte[] destination)
		{
			Buffer.BlockCopy(_data, _position, destination, 0, AvailableBytes);
			_position = _data.Length;
		}

		public void GetBytes(byte[] destination, int lenght)
		{
			Buffer.BlockCopy(_data, _position, destination, 0, lenght);
			_position += lenght;
		}

		public byte[] GetBytesWithLength()
		{
			int length = GetInt();
			byte[] outgoingData = new byte[length];
			Buffer.BlockCopy(_data, _position, outgoingData, 0, length);
			_position += length;
			return outgoingData;
		}

		public byte PeekByte()
		{
			return _data[_position];
		}

		public sbyte PeekSByte()
		{
			return (sbyte)_data[_position];
		}

		public bool PeekBool()
		{
			return _data[_position] > 0;
		}

		public char PeekChar()
		{
			return BitConverter.ToChar(_data, _position);
		}

		public ushort PeekUShort()
		{
			return BitConverter.ToUInt16(_data, _position);
		}

		public short PeekShort()
		{
			return BitConverter.ToInt16(_data, _position);
		}

		public long PeekLong()
		{
			return BitConverter.ToInt64(_data, _position);
		}

		public ulong PeekULong()
		{
			return BitConverter.ToUInt64(_data, _position);
		}

		public int PeekInt()
		{
			return BitConverter.ToInt32(_data, _position);
		}

		public uint PeekUInt()
		{
			return BitConverter.ToUInt32(_data, _position);
		}

		public float PeekFloat()
		{
			return BitConverter.ToSingle(_data, _position);
		}

		public double PeekDouble()
		{
			return BitConverter.ToDouble(_data, _position);
		}

		public string PeekString(int maxLength)
		{
			int bytesCount = BitConverter.ToInt32(_data, _position);
			if (bytesCount > 0 && bytesCount <= maxLength * 2)
			{
				if (Encoding.UTF8.GetCharCount(_data, _position + 4, bytesCount) > maxLength)
				{
					return string.Empty;
				}
				return Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
			}
			return string.Empty;
		}

		public string PeekString()
		{
			int bytesCount = BitConverter.ToInt32(_data, _position);
			if (bytesCount <= 0)
			{
				return string.Empty;
			}
			return Encoding.UTF8.GetString(_data, _position + 4, bytesCount);
		}

		public void Clear()
		{
			_position = 0;
			_dataSize = 0;
			_data = null;
		}
	}
}
