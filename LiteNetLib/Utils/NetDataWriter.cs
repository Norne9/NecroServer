using System;
using System.Text;

namespace LiteNetLib.Utils
{
	public class NetDataWriter
	{
		protected byte[] _data;

		protected int _position;

		private int _maxLength;

		private readonly bool _autoResize;

		public byte[] Data => _data;

		public int Length => _position;

		public NetDataWriter()
		{
			_maxLength = 64;
			_data = new byte[_maxLength];
			_autoResize = true;
		}

		public NetDataWriter(bool autoResize)
		{
			_maxLength = 64;
			_data = new byte[_maxLength];
			_autoResize = autoResize;
		}

		public NetDataWriter(bool autoResize, int initialSize)
		{
			_maxLength = initialSize;
			_data = new byte[_maxLength];
			_autoResize = autoResize;
		}

		public void ResizeIfNeed(int newSize)
		{
			if (_maxLength < newSize)
			{
				while (_maxLength < newSize)
				{
					_maxLength *= 2;
				}
				Array.Resize(ref _data, _maxLength);
			}
		}

		public void Reset(int size)
		{
			ResizeIfNeed(size);
			_position = 0;
		}

		public void Reset()
		{
			_position = 0;
		}

		public byte[] CopyData()
		{
			byte[] resultData = new byte[_position];
			Buffer.BlockCopy(_data, 0, resultData, 0, _position);
			return resultData;
		}

		public void Put(float value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 4);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 4;
		}

		public void Put(double value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 8);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 8;
		}

		public void Put(long value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 8);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 8;
		}

		public void Put(ulong value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 8);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 8;
		}

		public void Put(int value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 4);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 4;
		}

		public void Put(uint value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 4);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 4;
		}

		public void Put(char value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 2);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 2;
		}

		public void Put(ushort value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 2);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 2;
		}

		public void Put(short value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 2);
			}
			FastBitConverter.GetBytes(_data, _position, value);
			_position += 2;
		}

		public void Put(sbyte value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 1);
			}
			_data[_position] = (byte)value;
			_position++;
		}

		public void Put(byte value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 1);
			}
			_data[_position] = value;
			_position++;
		}

		public void Put(byte[] data, int offset, int length)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + length);
			}
			Buffer.BlockCopy(data, offset, _data, _position, length);
			_position += length;
		}

		public void Put(byte[] data)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + data.Length);
			}
			Buffer.BlockCopy(data, 0, _data, _position, data.Length);
			_position += data.Length;
		}

		public void PutBytesWithLength(byte[] data, int offset, int length)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + length + 4);
			}
			FastBitConverter.GetBytes(_data, _position, length);
			Buffer.BlockCopy(data, offset, _data, _position + 4, length);
			_position += length + 4;
		}

		public void PutBytesWithLength(byte[] data)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + data.Length + 4);
			}
			FastBitConverter.GetBytes(_data, _position, data.Length);
			Buffer.BlockCopy(data, 0, _data, _position + 4, data.Length);
			_position += data.Length + 4;
		}

		public void Put(bool value)
		{
			if (_autoResize)
			{
				ResizeIfNeed(_position + 1);
			}
			_data[_position] = (byte)(value ? 1 : 0);
			_position++;
		}

		public void PutArray(float[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 4 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(double[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 8 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(long[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 8 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(ulong[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 8 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(int[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 4 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(uint[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 4 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(ushort[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 2 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(short[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len * 2 + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(bool[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			if (_autoResize)
			{
				ResizeIfNeed(_position + len + 2);
			}
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(string[] value)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i]);
			}
		}

		public void PutArray(string[] value, int maxLength)
		{
			ushort len = (ushort)((value != null) ? ((ushort)value.Length) : 0);
			Put(len);
			for (int i = 0; i < len; i++)
			{
				Put(value[i], maxLength);
			}
		}

		public void Put(NetEndPoint endPoint)
		{
			Put(endPoint.Host);
			Put(endPoint.Port);
		}

		public void Put(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				Put(0);
			}
			else
			{
				int bytesCount = Encoding.UTF8.GetByteCount(value);
				if (_autoResize)
				{
					ResizeIfNeed(_position + bytesCount + 4);
				}
				Put(bytesCount);
				Encoding.UTF8.GetBytes(value, 0, value.Length, _data, _position);
				_position += bytesCount;
			}
		}

		public void Put(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value))
			{
				Put(0);
			}
			else
			{
				int length = (value.Length > maxLength) ? maxLength : value.Length;
				int bytesCount = Encoding.UTF8.GetByteCount(value);
				if (_autoResize)
				{
					ResizeIfNeed(_position + bytesCount + 4);
				}
				Put(bytesCount);
				Encoding.UTF8.GetBytes(value, 0, length, _data, _position);
				_position += bytesCount;
			}
		}
	}
}
