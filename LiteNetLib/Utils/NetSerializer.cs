using System;
using System.Collections.Generic;
using System.Reflection;

namespace LiteNetLib.Utils
{
	public sealed class NetSerializer
	{
		private sealed class CustomType
		{
			public readonly CustomTypeWrite WriteDelegate;

			public readonly CustomTypeRead ReadDelegate;

			public CustomType(CustomTypeWrite writeDelegate, CustomTypeRead readDelegate)
			{
				WriteDelegate = writeDelegate;
				ReadDelegate = readDelegate;
			}
		}

		private delegate void CustomTypeWrite(NetDataWriter writer, object customObj);

		private delegate object CustomTypeRead(NetDataReader reader);

		private sealed class StructInfo
		{
			public readonly Action<NetDataWriter>[] WriteDelegate;

			public readonly Action<NetDataReader>[] ReadDelegate;

			public readonly Type[] FieldTypes;

			public object Reference;

			public Func<object> CreatorFunc;

			public Action<object, object> OnReceive;

			public readonly ulong Hash;

			public readonly int MembersCount;

			public StructInfo(ulong hash, int membersCount)
			{
				Hash = hash;
				MembersCount = membersCount;
				WriteDelegate = new Action<NetDataWriter>[membersCount];
				ReadDelegate = new Action<NetDataReader>[membersCount];
				FieldTypes = new Type[membersCount];
			}

			public void Write(NetDataWriter writer, object obj)
			{
				Reference = obj;
				for (int i = 0; i < MembersCount; i++)
				{
					WriteDelegate[i](writer);
				}
			}

			public void Read(NetDataReader reader)
			{
				for (int i = 0; i < MembersCount; i++)
				{
					ReadDelegate[i](reader);
				}
			}
		}

		private readonly Dictionary<ulong, StructInfo> _cache;

		private readonly Dictionary<Type, CustomType> _registeredCustomTypes;

		private static readonly HashSet<Type> BasicTypes = new HashSet<Type>
		{
			typeof(int),
			typeof(uint),
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(char),
			typeof(ushort),
			typeof(long),
			typeof(ulong),
			typeof(string),
			typeof(NetEndPoint),
			typeof(float),
			typeof(double),
			typeof(bool)
		};

		private readonly NetDataWriter _writer;

		private readonly NetSerializerHasher _hasher;

		private readonly int _maxStringLength;

		public NetSerializer(int maxStringLength = 1024)
			: this(new FNVHasher(), maxStringLength)
		{
		}

		public NetSerializer(NetSerializerHasher hasher, int maxStringLength = 1024)
		{
			_maxStringLength = maxStringLength;
			_hasher = hasher;
			_cache = new Dictionary<ulong, StructInfo>();
			_registeredCustomTypes = new Dictionary<Type, CustomType>();
			_writer = new NetDataWriter();
		}

		private bool RegisterCustomTypeInternal<T>(Func<T> constructor) where T : INetSerializable
		{
			Type t = typeof(T);
			if (_registeredCustomTypes.ContainsKey(t))
			{
				return false;
			}
			CustomType rwDelegates = new CustomType(delegate(NetDataWriter writer, object obj)
			{
				((T)obj).Serialize(writer);
			}, delegate(NetDataReader reader)
			{
				T val = constructor();
				val.Deserialize(reader);
				return val;
			});
			_registeredCustomTypes.Add(t, rwDelegates);
			return true;
		}

		/// <summary>
		/// Register custom property type
		/// </summary>
		/// <typeparam name="T">INetSerializable structure</typeparam>
		/// <returns>True - if register successful, false - if type already registered</returns>
		public bool RegisterCustomType<T>() where T : struct, INetSerializable
		{
			return RegisterCustomTypeInternal(() => new T());
		}

		/// <summary>
		/// Register custom property type
		/// </summary>
		/// <typeparam name="T">INetSerializable class</typeparam>
		/// <returns>True - if register successful, false - if type already registered</returns>
		public bool RegisterCustomType<T>(Func<T> constructor) where T : class, INetSerializable
		{
			return RegisterCustomTypeInternal(constructor);
		}

		/// <summary>
		/// Register custom property type
		/// </summary>
		/// <param name="writeDelegate"></param>
		/// <param name="readDelegate"></param>
		/// <returns>True - if register successful, false - if type already registered</returns>
		public bool RegisterCustomType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate)
		{
			Type t = typeof(T);
			if (!BasicTypes.Contains(t) && !_registeredCustomTypes.ContainsKey(t))
			{
				CustomType rwDelegates = new CustomType(delegate(NetDataWriter writer, object obj)
				{
					writeDelegate(writer, (T)obj);
				}, (NetDataReader reader) => readDelegate(reader));
				_registeredCustomTypes.Add(t, rwDelegates);
				return true;
			}
			return false;
		}

		private static Delegate CreateDelegate(Type type, MethodInfo info)
		{
			return Delegate.CreateDelegate(type, info);
		}

		private static Func<TClass, TProperty> ExtractGetDelegate<TClass, TProperty>(MethodInfo info)
		{
			return (Func<TClass, TProperty>)CreateDelegate(typeof(Func<TClass, TProperty>), info);
		}

		private static Action<TClass, TProperty> ExtractSetDelegate<TClass, TProperty>(MethodInfo info)
		{
			return (Action<TClass, TProperty>)CreateDelegate(typeof(Action<TClass, TProperty>), info);
		}

		private StructInfo RegisterInternal<T>() where T : class
		{
			Type t = typeof(T);
			ulong nameHash = _hasher.GetHash(t.Name);
            StructInfo info;
            if (_cache.TryGetValue(nameHash, out info))
			{
				return info;
			}
			PropertyInfo[] props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			int propsCount = props.Length;
			if (props != null && propsCount >= 0)
			{
				info = new StructInfo(nameHash, propsCount);
				for (int i = 0; i < props.Length; i++)
				{
					PropertyInfo property = props[i];
					Type propertyType = property.PropertyType;
					info.FieldTypes[i] = (propertyType.IsArray ? propertyType.GetElementType() : propertyType);
					bool isEnum = propertyType.IsEnum;
					MethodInfo getMethod = property.GetGetMethod();
					MethodInfo setMethod = property.GetSetMethod();
					if (isEnum)
					{
						Type underlyingType = Enum.GetUnderlyingType(propertyType);
						if (underlyingType == typeof(byte))
						{
							info.ReadDelegate[i] = delegate(NetDataReader reader)
							{
								property.SetValue(info.Reference, Enum.ToObject(propertyType, reader.GetByte()), null);
							};
							info.WriteDelegate[i] = delegate(NetDataWriter writer)
							{
								writer.Put((byte)property.GetValue(info.Reference, null));
							};
							continue;
						}
						if (underlyingType == typeof(int))
						{
							info.ReadDelegate[i] = delegate(NetDataReader reader)
							{
								property.SetValue(info.Reference, Enum.ToObject(propertyType, reader.GetInt()), null);
							};
							info.WriteDelegate[i] = delegate(NetDataWriter writer)
							{
								writer.Put((int)property.GetValue(info.Reference, null));
							};
							continue;
						}
						throw new Exception("Not supported enum underlying type: " + underlyingType.Name);
					}
					if (propertyType == typeof(NetEndPoint))
					{
						Action<T, NetEndPoint> setDelegate = (Action<T, NetEndPoint>)ExtractSetDelegate<T, NetEndPoint>(setMethod);
						Func<T, NetEndPoint> getDelegate = (Func<T, NetEndPoint>)ExtractGetDelegate<T, NetEndPoint>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate((T)info.Reference, reader.GetNetEndPoint());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(string))
					{
						Action<T, string> setDelegate2 = (Action<T, string>)ExtractSetDelegate<T, string>(setMethod);
						Func<T, string> getDelegate2 = (Func<T, string>)ExtractGetDelegate<T, string>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate2((T)info.Reference, reader.GetString(_maxStringLength));
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate2((T)info.Reference), _maxStringLength);
						};
						continue;
					}
					if (propertyType == typeof(bool))
					{
						Action<T, bool> setDelegate3 = (Action<T, bool>)ExtractSetDelegate<T, bool>(setMethod);
						Func<T, bool> getDelegate3 = (Func<T, bool>)ExtractGetDelegate<T, bool>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate3((T)info.Reference, reader.GetBool());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate3((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(byte))
					{
						Action<T, byte> setDelegate4 = (Action<T, byte>)ExtractSetDelegate<T, byte>(setMethod);
						Func<T, byte> getDelegate4 = (Func<T, byte>)ExtractGetDelegate<T, byte>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate4((T)info.Reference, reader.GetByte());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate4((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(sbyte))
					{
						Action<T, sbyte> setDelegate5 = (Action<T, sbyte>)ExtractSetDelegate<T, sbyte>(setMethod);
						Func<T, sbyte> getDelegate5 = (Func<T, sbyte>)ExtractGetDelegate<T, sbyte>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate5((T)info.Reference, reader.GetSByte());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate5((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(short))
					{
						Action<T, short> setDelegate6 = (Action<T, short>)ExtractSetDelegate<T, short>(setMethod);
						Func<T, short> getDelegate6 = (Func<T, short>)ExtractGetDelegate<T, short>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate6((T)info.Reference, reader.GetShort());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate6((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(char))
					{
						Action<T, char> setDelegate7 = (Action<T, char>)ExtractSetDelegate<T, char>(setMethod);
						Func<T, char> getDelegate7 = (Func<T, char>)ExtractGetDelegate<T, char>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate7((T)info.Reference, reader.GetChar());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate7((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(ushort))
					{
						Action<T, ushort> setDelegate8 = (Action<T, ushort>)ExtractSetDelegate<T, ushort>(setMethod);
						Func<T, ushort> getDelegate8 = (Func<T, ushort>)ExtractGetDelegate<T, ushort>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate8((T)info.Reference, reader.GetUShort());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate8((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(int))
					{
						Action<T, int> setDelegate9 = (Action<T, int>)ExtractSetDelegate<T, int>(setMethod);
						Func<T, int> getDelegate9 = (Func<T, int>)ExtractGetDelegate<T, int>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate9((T)info.Reference, reader.GetInt());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate9((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(uint))
					{
						Action<T, uint> setDelegate10 = (Action<T, uint>)ExtractSetDelegate<T, uint>(setMethod);
						Func<T, uint> getDelegate10 = (Func<T, uint>)ExtractGetDelegate<T, uint>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate10((T)info.Reference, reader.GetUInt());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate10((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(long))
					{
						Action<T, long> setDelegate11 = (Action<T, long>)ExtractSetDelegate<T, long>(setMethod);
						Func<T, long> getDelegate11 = (Func<T, long>)ExtractGetDelegate<T, long>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate11((T)info.Reference, reader.GetLong());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate11((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(ulong))
					{
						Action<T, ulong> setDelegate12 = (Action<T, ulong>)ExtractSetDelegate<T, ulong>(setMethod);
						Func<T, ulong> getDelegate12 = (Func<T, ulong>)ExtractGetDelegate<T, ulong>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate12((T)info.Reference, reader.GetULong());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate12((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(float))
					{
						Action<T, float> setDelegate13 = (Action<T, float>)ExtractSetDelegate<T, float>(setMethod);
						Func<T, float> getDelegate13 = (Func<T, float>)ExtractGetDelegate<T, float>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate13((T)info.Reference, reader.GetFloat());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate13((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(double))
					{
						Action<T, double> setDelegate14 = (Action<T, double>)ExtractSetDelegate<T, double>(setMethod);
						Func<T, double> getDelegate14 = (Func<T, double>)ExtractGetDelegate<T, double>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate14((T)info.Reference, reader.GetDouble());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.Put(getDelegate14((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(string[]))
					{
						Action<T, string[]> setDelegate15 = (Action<T, string[]>)ExtractSetDelegate<T, string[]>(setMethod);
						Func<T, string[]> getDelegate15 = (Func<T, string[]>)ExtractGetDelegate<T, string[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate15((T)info.Reference, reader.GetStringArray(_maxStringLength));
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate15((T)info.Reference), _maxStringLength);
						};
						continue;
					}
					if (propertyType == typeof(bool[]))
					{
						Action<T, bool[]> setDelegate18 = (Action<T, bool[]>)ExtractSetDelegate<T, bool[]>(setMethod);
						Func<T, bool[]> getDelegate17 = (Func<T, bool[]>)ExtractGetDelegate<T, bool[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate18((T)info.Reference, reader.GetBoolArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate17((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(byte[]))
					{
						Action<T, byte[]> setDelegate21 = (Action<T, byte[]>)ExtractSetDelegate<T, byte[]>(setMethod);
						Func<T, byte[]> getDelegate20 = (Func<T, byte[]>)ExtractGetDelegate<T, byte[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate21((T)info.Reference, reader.GetBytesWithLength());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutBytesWithLength(getDelegate20((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(short[]))
					{
						Action<T, short[]> setDelegate24 = (Action<T, short[]>)ExtractSetDelegate<T, short[]>(setMethod);
						Func<T, short[]> getDelegate23 = (Func<T, short[]>)ExtractGetDelegate<T, short[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate24((T)info.Reference, reader.GetShortArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate23((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(ushort[]))
					{
						Action<T, ushort[]> setDelegate25 = (Action<T, ushort[]>)ExtractSetDelegate<T, ushort[]>(setMethod);
						Func<T, ushort[]> getDelegate25 = (Func<T, ushort[]>)ExtractGetDelegate<T, ushort[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate25((T)info.Reference, reader.GetUShortArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate25((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(int[]))
					{
						Action<T, int[]> setDelegate23 = (Action<T, int[]>)ExtractSetDelegate<T, int[]>(setMethod);
						Func<T, int[]> getDelegate24 = (Func<T, int[]>)ExtractGetDelegate<T, int[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate23((T)info.Reference, reader.GetIntArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate24((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(uint[]))
					{
						Action<T, uint[]> setDelegate22 = (Action<T, uint[]>)ExtractSetDelegate<T, uint[]>(setMethod);
						Func<T, uint[]> getDelegate22 = (Func<T, uint[]>)ExtractGetDelegate<T, uint[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate22((T)info.Reference, reader.GetUIntArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate22((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(long[]))
					{
						Action<T, long[]> setDelegate20 = (Action<T, long[]>)ExtractSetDelegate<T, long[]>(setMethod);
						Func<T, long[]> getDelegate21 = (Func<T, long[]>)ExtractGetDelegate<T, long[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate20((T)info.Reference, reader.GetLongArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate21((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(ulong[]))
					{
						Action<T, ulong[]> setDelegate19 = (Action<T, ulong[]>)ExtractSetDelegate<T, ulong[]>(setMethod);
						Func<T, ulong[]> getDelegate19 = (Func<T, ulong[]>)ExtractGetDelegate<T, ulong[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate19((T)info.Reference, reader.GetULongArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate19((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(float[]))
					{
						Action<T, float[]> setDelegate17 = (Action<T, float[]>)ExtractSetDelegate<T, float[]>(setMethod);
						Func<T, float[]> getDelegate18 = (Func<T, float[]>)ExtractGetDelegate<T, float[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate17((T)info.Reference, reader.GetFloatArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate18((T)info.Reference));
						};
						continue;
					}
					if (propertyType == typeof(double[]))
					{
						Action<T, double[]> setDelegate16 = (Action<T, double[]>)ExtractSetDelegate<T, double[]>(setMethod);
						Func<T, double[]> getDelegate16 = (Func<T, double[]>)ExtractGetDelegate<T, double[]>(getMethod);
						info.ReadDelegate[i] = delegate(NetDataReader reader)
						{
							setDelegate16((T)info.Reference, reader.GetDoubleArray());
						};
						info.WriteDelegate[i] = delegate(NetDataWriter writer)
						{
							writer.PutArray(getDelegate16((T)info.Reference));
						};
						continue;
					}
					bool array = false;
					if (propertyType.IsArray)
					{
						array = true;
						propertyType = propertyType.GetElementType();
					}
                    CustomType registeredCustomType;
                    if (_registeredCustomTypes.TryGetValue(propertyType, out registeredCustomType))
					{
						if (array)
						{
							info.ReadDelegate[i] = delegate(NetDataReader reader)
							{
								ushort uShort = reader.GetUShort();
								Array array3 = Array.CreateInstance(propertyType, uShort);
								for (int k = 0; k < uShort; k++)
								{
									array3.SetValue(registeredCustomType.ReadDelegate(reader), k);
								}
								property.SetValue(info.Reference, array3, null);
							};
							info.WriteDelegate[i] = delegate(NetDataWriter writer)
							{
								Array array2 = (Array)property.GetValue(info.Reference, null);
								writer.Put((ushort)array2.Length);
								for (int j = 0; j < array2.Length; j++)
								{
									registeredCustomType.WriteDelegate(writer, array2.GetValue(j));
								}
							};
						}
						else
						{
							info.ReadDelegate[i] = delegate(NetDataReader reader)
							{
								property.SetValue(info.Reference, registeredCustomType.ReadDelegate(reader), null);
							};
							info.WriteDelegate[i] = delegate(NetDataWriter writer)
							{
								registeredCustomType.WriteDelegate(writer, property.GetValue(info.Reference, null));
							};
						}
						continue;
					}
					throw new Exception("Unknown property type: " + propertyType.Name);
				}
				_cache.Add(nameHash, info);
				return info;
			}
			throw new ArgumentException("Type does not contain acceptable fields");
		}

		/// <summary>
		/// Reads all available data from NetDataReader and calls OnReceive delegates
		/// </summary>
		/// <param name="reader">NetDataReader with packets data</param>
		public void ReadAllPackets(NetDataReader reader)
		{
			while (reader.AvailableBytes > 0)
			{
				ReadPacket(reader);
			}
		}

		/// <summary>
		/// Reads all available data from NetDataReader and calls OnReceive delegates
		/// </summary>
		/// <param name="reader">NetDataReader with packets data</param>
		/// <param name="userData">Argument that passed to OnReceivedEvent</param>
		public void ReadAllPackets<T>(NetDataReader reader, T userData)
		{
			while (reader.AvailableBytes > 0)
			{
				ReadPacket(reader, userData);
			}
		}

		/// <summary>
		/// Reads one packet from NetDataReader and calls OnReceive delegate
		/// </summary>
		/// <param name="reader">NetDataReader with packet</param>
		public void ReadPacket(NetDataReader reader)
		{
			ReadPacket(reader, null);
		}

		private StructInfo ReadInfo(NetDataReader reader)
		{
			ulong hash = _hasher.ReadHash(reader);
            StructInfo info;
            if (!_cache.TryGetValue(hash, out info))
			{
				throw new Exception("Undefined packet received");
			}
			return info;
		}

		/// <summary>
		/// Reads packet with known type
		/// </summary>
		/// <param name="reader">NetDataReader with packet</param>
		/// <returns>Returns packet if packet in reader is matched type</returns>
		public T ReadKnownPacket<T>(NetDataReader reader) where T : class, new()
		{
			StructInfo info = ReadInfo(reader);
			if (_hasher.GetHash(typeof(T).Name) != info.Hash)
			{
				return null;
			}
			info.Reference = ((info.CreatorFunc != null) ? info.CreatorFunc() : new T());
			info.Read(reader);
			return (T)info.Reference;
		}

		/// <summary>
		/// Reads packet with known type (non alloc variant)
		/// </summary>
		/// <param name="reader">NetDataReader with packet</param>
		/// <param name="target">Deserialization target</param>
		/// <returns>Returns true if packet in reader is matched type</returns>
		public bool ReadKnownPacket<T>(NetDataReader reader, T target) where T : class, new()
		{
			StructInfo info = ReadInfo(reader);
			if (_hasher.GetHash(typeof(T).Name) != info.Hash)
			{
				return false;
			}
			info.Reference = target;
			info.Read(reader);
			return true;
		}

		/// <summary>
		/// Reads one packet from NetDataReader and calls OnReceive delegate
		/// </summary>
		/// <param name="reader">NetDataReader with packet</param>
		/// <param name="userData">Argument that passed to OnReceivedEvent</param>
		public void ReadPacket(NetDataReader reader, object userData)
		{
			StructInfo info = ReadInfo(reader);
			if (info.CreatorFunc != null)
			{
				info.Reference = info.CreatorFunc();
			}
			info.Read(reader);
			info.OnReceive(info.Reference, userData);
		}

		/// <summary>
		/// Register and subscribe to packet receive event
		/// </summary>
		/// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
		/// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
		public void Subscribe<T>(Action<T> onReceive, Func<T> packetConstructor) where T : class, new()
		{
			StructInfo structInfo = RegisterInternal<T>();
			structInfo.CreatorFunc = (() => packetConstructor());
			structInfo.OnReceive = delegate(object o, object userData)
			{
				onReceive((T)o);
			};
		}

		/// <summary>
		/// Register packet type for direct reading (ReadKnownPacket)
		/// </summary>
		/// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
		public void Register<T>(Func<T> packetConstructor = null) where T : class, new()
		{
			StructInfo info = RegisterInternal<T>();
			if (packetConstructor != null)
			{
				info.CreatorFunc = (() => packetConstructor());
			}
			info.OnReceive = delegate
			{
			};
		}

		/// <summary>
		/// Register and subscribe to packet receive event (with userData)
		/// </summary>
		/// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
		/// <param name="packetConstructor">Method that constructs packet intead of slow Activator.CreateInstance</param>
		public void Subscribe<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : class, new()
		{
			StructInfo structInfo = RegisterInternal<T>();
			structInfo.CreatorFunc = (() => packetConstructor());
			structInfo.OnReceive = delegate(object o, object userData)
			{
				onReceive((T)o, (TUserData)userData);
			};
		}

		/// <summary>
		/// Register and subscribe to packet receive event
		/// This metod will overwrite last received packet class on receive (less garbage)
		/// </summary>
		/// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
		public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
		{
			StructInfo structInfo = RegisterInternal<T>();
			structInfo.Reference = new T();
			structInfo.OnReceive = delegate(object o, object userData)
			{
				onReceive((T)o);
			};
		}

		/// <summary>
		/// Register and subscribe to packet receive event
		/// This metod will overwrite last received packet class on receive (less garbage)
		/// </summary>
		/// <param name="onReceive">event that will be called when packet deserialized with ReadPacket method</param>
		public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
		{
			StructInfo structInfo = RegisterInternal<T>();
			structInfo.Reference = new T();
			structInfo.OnReceive = delegate(object o, object userData)
			{
				onReceive((T)o, (TUserData)userData);
			};
		}

		/// <summary>
		/// Serialize struct to NetDataWriter (fast)
		/// </summary>
		/// <param name="writer">Serialization target NetDataWriter</param>
		/// <param name="obj">Struct to serialize</param>
		public void Serialize<T>(NetDataWriter writer, T obj) where T : class, new()
		{
			StructInfo info = RegisterInternal<T>();
			_hasher.WriteHash(info.Hash, writer);
			info.Write(writer, obj);
		}

		/// <summary>
		/// Serialize struct to byte array
		/// </summary>
		/// <param name="obj">Struct to serialize</param>
		/// <returns>byte array with serialized data</returns>
		public byte[] Serialize<T>(T obj) where T : class, new()
		{
			_writer.Reset();
			Serialize(_writer, obj);
			return _writer.CopyData();
		}
	}
}
