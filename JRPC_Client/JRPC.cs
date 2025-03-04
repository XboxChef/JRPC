//Original Code By James If you ever see this thanks
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using XDevkit;

namespace JRPC_Client
{
    public static class JRPC
    {
        private static readonly uint Void = 0,
            Int = 1,
            String = 2,
            Float = 3,
            Byte = 4,
            IntArray = 5,
            FloatArray = 6,
            ByteArray = 7,
            Uint64 = 8,
            Uint64Array = 9;
        private static uint connectionId;
        public static readonly uint JRPCVersion = 2;

        public static string ToHexString(this string String)
        {
            string Str = "";
            foreach (byte b in String)
                Str += b.ToString("X2");
            return Str;
        }

        public static void Push(this byte[] InArray, out byte[] OutArray, byte Value)
        {
            OutArray = new byte[InArray.Length + 1];
            InArray.CopyTo(OutArray, 0);
            OutArray[InArray.Length] = Value;
        }

        public static byte[] ToByteArray(this string String)
        {
            byte[] Return = new byte[String.Length+1];
            for (int i = 0; i < String.Length; i++)
                Return[i] = (byte)String[i];
            return Return;
        }

        public static int find(this string String, string _Ptr)
        {
            if (_Ptr.Length == 0 || String.Length == 0)
                return -1;
            for (int i = 0; i < String.Length; i++)
            {
                if (String[i] == _Ptr[0])
                {
                    bool Found = true;
                    for (int q = 0; q < _Ptr.Length; q++)
                        if (String[i + q] != _Ptr[q])
                            Found = false;
                    if (Found)
                        return i;
                }
            }
            return -1;
        }

        public static bool Connect(this IXboxConsole console, out IXboxConsole Console, string XboxNameOrIP = "default")
        {
            IXboxConsole Con;
            if (XboxNameOrIP == "default")
                XboxNameOrIP = new XboxManager().DefaultConsole;
            Con = new XboxManager().OpenConsole(XboxNameOrIP);
            bool Connected = false;
            while (!Connected)
            {
                try
                {
                    connectionId = Con.OpenConnection(null);
                    Connected = true;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == UIntToInt(0x82DA0100))
                    {
                        Console = Con;
                        return false;
                    }
                }
            }
            Console = Con;
            return true;
        }

        public static string XboxIP(this IXboxConsole console)
        {
            byte[] address = BitConverter.GetBytes(console.IPAddress);
            Array.Reverse(address);
            return new System.Net.IPAddress(address).ToString();
        }

        internal static ulong ConvertToUInt64(object o)
        {
            if (o is bool)
                return (ulong)(!(bool)o ? 0UL : 1UL);
            else
            {
                if (o is byte)
                    return (ulong)(byte)o;
                if (o is short)
                    return (ulong)(short)o;
                if (o is int)
                    return (ulong)(int)o;
                if (o is long)
                    return (ulong)(long)o;
                if (o is ushort)
                    return (ulong)(ushort)o;
                if (o is uint)
                    return (ulong)(uint)o;
                if (o is ulong)
                    return (ulong)o;
                if (o is float)
                    return (ulong)BitConverter.DoubleToInt64Bits((double)(float)o);
                if (o is double)
                    return (ulong)BitConverter.DoubleToInt64Bits((double)o);
                else
                    return 0UL;
            }
        }

        private static Dictionary<Type, int> ValueTypeSizeMap = new Dictionary<Type, int>()
        {
            { typeof (bool), 4 }, { typeof (byte), 1 }, { typeof (short), 2 }, { typeof (int), 4 },
            { typeof (long), 8 }, { typeof (ushort), 2 }, { typeof (uint), 4 }, { typeof (ulong), 8 },
            { typeof (float), 4   }, { typeof (double), 8 }
        };
        private static Dictionary<Type, int> StructPrimitiveSizeMap = new Dictionary<Type, int>();

        internal static bool IsValidStructType(Type t)
        {
            if (!t.IsPrimitive)
                return t.IsValueType;
            else
                return false;
        }

        private static HashSet<Type> ValidReturnTypes = new HashSet<Type>()
        {
            typeof (void),
            typeof (bool),
            typeof (byte),
            typeof (short),
            typeof (int),
            typeof (long),
            typeof (ushort),
            typeof (uint),
            typeof (ulong),
            typeof (float),
            typeof (double),
            typeof (string),
            typeof (bool[]),
            typeof (byte[]),
            typeof (short[]),
            typeof (int[]),
            typeof (long[]),
            typeof (ushort[]),
            typeof (uint[]),
            typeof (ulong[]),
            typeof (float[]),
            typeof (double[]),
            typeof (string[])
        };

        internal static bool IsValidReturnType(Type t)
        {
            return ValidReturnTypes.Contains(t);
        }

        static void ReverseBytes(byte[] buffer, int groupSize)
        {
            if (buffer.Length % groupSize != 0)
                throw new ArgumentException("Group size must be a multiple of the buffer length", "groupSize");
            int num1 = 0;
            while (num1 < buffer.Length)
            {
                int index1 = num1;
                for (int index2 = num1 + groupSize - 1; index1 < index2; --index2)
                {
                    byte num2 = buffer[index1];
                    buffer[index1] = buffer[index2];
                    buffer[index2] = num2;
                    ++index1;
                }
                num1 += groupSize;
            }
        }

        ///////////////////////// Reading Memory \\\\\\\\\\\\\\\\\\\\\\\\\
        public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length)
        {
            uint Out = 0; ;
            byte[] Return = new byte[Length];
            console.DebugTarget.GetMemory(Address, Length, Return, out Out);
            console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
            return Return;
        }

        public static sbyte ReadSByte(this IXboxConsole console, uint Address)
        {
            return (sbyte)GetMemory(console, Address, 1)[0];
        }

        public static byte ReadByte(this IXboxConsole console, uint Address)
        {
            return GetMemory(console, Address, 1)[0];
        }

        public static bool ReadBool(this IXboxConsole console, uint Address)
        {
            return GetMemory(console, Address, 1)[0] != 0;
        }

        public static float ReadFloat(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 4);
            ReverseBytes(Buff, 4);
            return BitConverter.ToSingle(Buff, 0);
        }

        public static float[] ReadFloat(this IXboxConsole console, uint Address, uint ArraySize)
        {
            float[] Return = new float[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 4);
            ReverseBytes(Buff, 4);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToSingle(Buff, i * 4);
            return Return;
        }

        public static short ReadInt16(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 2);
            ReverseBytes(Buff, 2);
            return BitConverter.ToInt16(Buff, 0);
        }

        public static short[] ReadInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            short[] Return = new short[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 2);
            ReverseBytes(Buff, 2);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToInt16(Buff, i * 2);
            return Return;
        }

        public static ushort ReadUInt16(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 2);
            ReverseBytes(Buff, 2);
            return BitConverter.ToUInt16(Buff, 0);
        }

        public static ushort[] ReadUInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ushort[] Return = new ushort[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 2);
            ReverseBytes(Buff, 2);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToUInt16(Buff, i * 2);
            return Return;
        }

        public static int ReadInt32(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 4);
            ReverseBytes(Buff, 4);
            return BitConverter.ToInt32(Buff, 0);
        }

        public static int[] ReadInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            int[] Return = new int[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 4);
            ReverseBytes(Buff, 4);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToInt32(Buff, i * 4);
            return Return;
        }

        public static uint ReadUInt32(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 4);
            ReverseBytes(Buff, 4);
            return BitConverter.ToUInt32(Buff, 0);
        }

        public static uint[] ReadUInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            uint[] Return = new uint[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 4);
            ReverseBytes(Buff, 4);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToUInt32(Buff, i * 4);
            return Return;
        }

        public static long ReadInt64(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 8);
            ReverseBytes(Buff, 8);
            return BitConverter.ToInt64(Buff, 0);
        }

        public static long[] ReadInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            long[] Return = new long[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 8);
            ReverseBytes(Buff, 8);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToUInt32(Buff, i * 8);
            return Return;
        }

        public static ulong ReadUInt64(this IXboxConsole console, uint Address)
        {
            byte[] Buff = GetMemory(console, Address, 8);
            ReverseBytes(Buff, 8);
            return BitConverter.ToUInt64(Buff, 0);
        }

        public static ulong[] ReadUInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ulong[] Return = new ulong[ArraySize];
            byte[] Buff = GetMemory(console, Address, ArraySize * 8);
            ReverseBytes(Buff, 8);
            for (int i = 0; i < ArraySize; i++)
                Return[i] = BitConverter.ToUInt32(Buff, i * 8);
            return Return;
        }

        public static string ReadString(this IXboxConsole console, uint Address, uint size)
        {
            return Encoding.UTF8.GetString(GetMemory(console, Address, size));
        }

        ///////////////////////// Writing Memory \\\\\\\\\\\\\\\\\\\\\\\\\
        public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data)
        {
            uint Out;
            console.DebugTarget.SetMemory(Address, (uint)Data.Length, Data, out Out);
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte Value)
        {
            SetMemory(console, Address, new byte[1] { BitConverter.GetBytes(Value)[0] });
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte[] Value)
        {
            byte[] Bytes = new byte[0];
            foreach (byte b in Value)
                Bytes.Push(out Bytes, b);
            SetMemory(console, Address, Bytes);
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte Value)
        {
            SetMemory(console, Address, new byte[1] { Value });
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value)
        {
            SetMemory(console, Address, Value);
        }

        public static void WriteBool(this IXboxConsole console, uint Address, bool Value)
        {
            SetMemory(console, Address, new byte[1] { (byte)(Value ? 1 : 0) });
        }

        public static void WriteBool(this IXboxConsole console, uint Address, bool[] Value)
        {
            byte[] Bytes = new byte[0];
            for(int i = 0; i < Value.Length; i++)
                Bytes.Push(out Bytes, (byte)(Value[i] ? 1 : 0));
            SetMemory(console, Address, Bytes);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            Array.Reverse(Buff);
            SetMemory(console, Address, Buff);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float[] Value)
        {
            byte[] Buff = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 4);
            ReverseBytes(Buff, 4);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 2);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short[] Value)
        {
            byte[] Buff = new byte[Value.Length * 2];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 2);
            ReverseBytes(Buff, 2);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 2);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort[] Value)
        {
            byte[] Buff = new byte[Value.Length * 2];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 2);
            ReverseBytes(Buff, 2);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 4);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int[] Value)
        {
            byte[] Buff = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 4);
            ReverseBytes(Buff, 4);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 4);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint[] Value)
        {
            byte[] Buff = new byte[Value.Length * 4];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 4);
            ReverseBytes(Buff, 4);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 8);
            SetMemory(console, Address, Buff);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long[] Value)
        {
            byte[] Buff = new byte[Value.Length * 8];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 8);
            ReverseBytes(Buff, 8);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value)
        {
            byte[] Buff = BitConverter.GetBytes(Value);
            ReverseBytes(Buff, 8);
            SetMemory(console, Address, Buff);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong[] Value)
        {
            byte[] Buff = new byte[Value.Length * 8];
            for (int i = 0; i < Value.Length; i++)
                BitConverter.GetBytes(Value[i]).CopyTo(Buff, i * 8);
            ReverseBytes(Buff, 8);
            SetMemory(console, Address, Buff);
        }

        public static void WriteString(this IXboxConsole console, uint Address, string String)
        {
            byte[] bValue = new byte[0];
            foreach(byte b in String)
                bValue.Push(out bValue, b);
            bValue.Push(out bValue, 0);
            SetMemory(console, Address, bValue);
        }

        public static uint ResolveFunction(this IXboxConsole console, string ModuleName, uint Ordinal)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=9 params=\"A\\0\\A\\2\\" + 
                JRPC.String + "/" + ModuleName.Length + "\\" + ModuleName.ToHexString() + "\\" + 
                JRPC.Int + "\\" + Ordinal + "\\\"",
                Responce = SendCommand(console, cmd);
            return uint.Parse(Responce.Substring(Responce.find(" ")+1), System.Globalization.NumberStyles.HexNumber);
        }

        public static string GetCPUKey(this IXboxConsole console)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=10 params=\"A\\0\\A\\0\\\"",
               Responce = SendCommand(console, cmd);
            return Responce.Substring(Responce.find(" ") + 1);
        }

        public static void ShutDownConsole(this IXboxConsole console)
        {
            try
            {
                string cmd = "consolefeatures ver=" + JRPCVersion + " type=11 params=\"A\\0\\A\\0\\\"";
                SendCommand(console, cmd);
            }
            catch { }
        }

        public static byte[] WCHAR(string String)
        {
            byte[] Bytes = new byte[String.Length * 2 + 2];
            int i = 1;
            foreach (byte b in String)
            {
                Bytes[i] = b;
                i += 2;
            }
            return Bytes;
        }

        public static byte[] ToWCHAR(this string String)
        {
            return WCHAR(String);
        }

        public static uint GetKernalVersion(this IXboxConsole console)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=13 params=\"A\\0\\A\\0\\\"",
            Responce = SendCommand(console, cmd);
            return uint.Parse(Responce.Substring(Responce.find(" ") + 1));
        }

        public enum LEDState
        {
            OFF = 0x00,
            RED = 0x08,
            GREEN = 0x80,
            ORANGE = 0x88,
        };

        public static void SetLeds(this IXboxConsole console, LEDState Top_Left, LEDState Top_Right, LEDState Bottom_Left, LEDState Bottom_Right)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=14 params=\"A\\0\\A\\4\\" + 
                JRPC.Int + "\\" + (uint)Top_Left + "\\" + 
                JRPC.Int + "\\" + (uint)Top_Right + "\\" +
                JRPC.Int + "\\" + (uint)Bottom_Left + "\\" +
                JRPC.Int + "\\" + (uint)Bottom_Right + "\\\"",
            Responce = SendCommand(console, cmd);
        }

        public enum TemperatureType
        {
            CPU,
            GPU,
            EDRAM,
            MotherBoard
        };

        public static uint GetTemperature(this IXboxConsole console, TemperatureType TemperatureType)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=15 params=\"A\\0\\A\\1\\" + 
                JRPC.Int + "\\" + (int)TemperatureType + "\\\"",
            Responce = SendCommand(console, cmd);
            return uint.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
        }

        public static void XNotify(this IXboxConsole console, string Text)
        {
            XNotify(console, Text, 34);
        }

        public static void XNotify(this IXboxConsole console, string Text, uint Type)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=12 params=\"A\\0\\A\\2\\" + 
                JRPC.String + "/" + Text.Length + "\\" + Text.ToHexString() + "\\" + 
                JRPC.Int + "\\" + Type + "\\\"";
            SendCommand(console, cmd);
        }

        public static uint XamGetCurrentTitleId(this IXboxConsole console)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=16 params=\"A\\0\\A\\0\\\"",
            Responce = SendCommand(console, cmd);
            return uint.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
        }

        public static string ConsoleType(this IXboxConsole console)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=17 params=\"A\\0\\A\\0\\\"",
            Responce = SendCommand(console, cmd);
            return Responce.Substring(Responce.find(" ") + 1);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value)
        {
            constantMemorySetting(console, Address, Value, false, 0, false, 0);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint TitleID)
        {
            constantMemorySetting(console, Address, Value, false, 0, true, TitleID);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value, uint IfValue, uint TitleID)
        {
            constantMemorySetting(console, Address, Value, true, IfValue, true, TitleID);
        }

        public static void constantMemorySetting(IXboxConsole console, uint Address, uint Value, bool useIfValue, uint IfValue, bool usetitleID, uint TitleID)
        {
            string cmd = "consolefeatures ver=" + JRPCVersion + " type=18 params=\"A\\" + Address.ToString("X") + "\\A\\5\\" + 
                JRPC.Int + "\\" + UIntToInt(Value) + "\\" +
                JRPC.Int + "\\" + (useIfValue ? 1 : 0) + "\\" +
                JRPC.Int + "\\" + IfValue + "\\" +
                JRPC.Int + "\\" + (usetitleID ? 1 : 0) + "\\" +
                JRPC.Int + "\\" + UIntToInt(TitleID) + "\\\"";
            SendCommand(console, cmd);
        }

        private static uint TypeToType<T>(bool Array) where T : struct
        {
            Type Type = typeof(T);
            if (Type == typeof(int) || Type == typeof(uint) || Type == typeof(short) || Type == typeof(ushort)) {
                if(Array)
                    return JRPC.IntArray;
                return JRPC.Int;
            }
            if (Type == typeof(string) || Type == typeof(char[]))
                return JRPC.String;
            if (Type == typeof(float) || Type == typeof(double)) {
                if (Array)
                    return JRPC.FloatArray;
                return JRPC.Float;
            }
            if (Type == typeof(byte) || Type == typeof(char)) {
                if (Array)
                    return JRPC.ByteArray;
                return JRPC.Byte;
            }
            if (Type == typeof(ulong) || Type == typeof(long)) {
                if (Array)
                    return JRPC.Uint64Array;
                return JRPC.Uint64;
            }
            return JRPC.Uint64;
        }

        public enum ThreadType
        {
            System,
            Title
        }

        public static T Call<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, true, TypeToType<T>(false), typeof(T), null, 0, Address, 0, false, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, true, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, false, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), null, 0, Address, 0, false, Arguments);
        }

        public static T Call<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, false, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            CallArgs(console, true, JRPC.Void, typeof(void), null, 0, Address, 0, false, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            CallArgs(console, true, JRPC.Void, typeof(void), module, ordinal, 0, 0, false, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments)
        {
            CallArgs(console, Type == ThreadType.System, JRPC.Void, typeof(void), null, 0, Address, 0, false, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            CallArgs(console, Type == ThreadType.System, JRPC.Void, typeof(void), module, ordinal, 0, 0, false, Arguments);
        }

        private static T[] ArrayReturn<T>(this IXboxConsole console, uint Address, uint Size)
        {
            if(Size == 0)
                return new T[1];
            Type type = typeof(T);
            object Return = new object();

            if (type == typeof(short))
                Return = ReadInt16(console, Address, Size);
            if (type == typeof(ushort))
                Return = ReadUInt16(console, Address, Size);
            if (type == typeof(int))
                Return = ReadInt32(console, Address, Size);
            if (type == typeof(uint))
                Return = ReadUInt32(console, Address, Size);
            if (type == typeof(long))
                Return = ReadInt64(console, Address, Size);
            if (type == typeof(ulong))
                Return = ReadUInt64(console, Address, Size);
            if (type == typeof(float))
                Return = ReadFloat(console, Address, Size);
            if (type == typeof(byte))
                Return = GetMemory(console, Address, Size);
            return (T[])Return;
        }

        public static T[] CallArray<T>(this IXboxConsole console, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, true, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, false, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, true, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, false, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, ThreadType Type, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, false, Arguments);
        }

        public static T[] CallArray<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, false, Arguments);
        }
        
        public static string CallString(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            return (string)CallArgs(console, true, JRPC.String, typeof(string), null, 0, Address, 0, false, Arguments);
        }

        public static string CallString(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            return (string)CallArgs(console, true, JRPC.String, typeof(string), module, ordinal, 0, 0, false, Arguments);
        }

        public static string CallString(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments)
        {
            return (string)CallArgs(console, Type == ThreadType.System, JRPC.String, typeof(string), null, 0, Address, 0, false, Arguments);
        }

        public static string CallString(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            return (string)CallArgs(console, Type == ThreadType.System, JRPC.String, typeof(string), module, ordinal, 0, 0, false, Arguments);
        }

        private static int UIntToInt(uint Value)
        {
            byte[] array = BitConverter.GetBytes(Value);
            return BitConverter.ToInt32(array, 0);
        }

        private static byte[] IntArrayToByte(int[] iArray)
        {
            byte[] Bytes = new byte[iArray.Length * 4];
            for (int i = 0, q = 0; i < iArray.Length; i++, q += 4)
                for (int w = 0; w < 4; w++)
                    Bytes[q + w] = BitConverter.GetBytes(iArray[i])[w];
            return Bytes;
        }

        public static T CallVM<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, true, TypeToType<T>(false), typeof(T), null, 0, Address, 0, true, Arguments);
        }

        public static T CallVM<T>(this IXboxConsole console, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, true, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, true, Arguments);
        }

        public static T CallVM<T>(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), null, 0, Address, 0, true, Arguments);
        }

        public static T CallVM<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments) where T : struct
        {
            return (T)CallArgs(console, Type == ThreadType.System, TypeToType<T>(false), typeof(T), module, ordinal, 0, 0, true, Arguments);
        }

        public static void CallVMVoid(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            CallArgs(console, true, JRPC.Void, typeof(void), null, 0, Address, 0, true, Arguments);
        }

        public static void CallVMVoid(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            CallArgs(console, true, JRPC.Void, typeof(void), module, ordinal, 0, 0, true, Arguments);
        }

        public static void CallVMVoid(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments)
        {
            CallArgs(console, Type == ThreadType.System, JRPC.Void, typeof(void), null, 0, Address, 0, true, Arguments);
        }

        public static void CallVMVoid(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            CallArgs(console, Type == ThreadType.System, JRPC.Void, typeof(void), module, ordinal, 0, 0, true, Arguments);
        }

        public static T[] CallVMArray<T>(this IXboxConsole console, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, true, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, true, Arguments);
        }

        public static T[] CallVMArray<T>(this IXboxConsole console, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, true, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, true, Arguments);
        }

        public static T[] CallVMArray<T>(this IXboxConsole console, ThreadType Type, uint Address, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), null, 0, Address, ArraySize, true, Arguments);
        }

        public static T[] CallVMArray<T>(this IXboxConsole console, ThreadType Type, string module, int ordinal, uint ArraySize, params object[] Arguments) where T : struct
        {
            if (ArraySize == 0)
                return new T[1];
            return (T[])CallArgs(console, Type == ThreadType.System, TypeToType<T>(true), typeof(T), module, ordinal, 0, ArraySize, true, Arguments);
        }

        public static string CallVMString(this IXboxConsole console, uint Address, params object[] Arguments)
        {
            return (string)CallArgs(console, true, JRPC.String, typeof(string), null, 0, Address, 0, true, Arguments);
        }

        public static string CallVMString(this IXboxConsole console, string module, int ordinal, params object[] Arguments)
        {
            return (string)CallArgs(console, true, JRPC.String, typeof(string), module, ordinal, 0, 0, true, Arguments);
        }

        public static string CallVMString(this IXboxConsole console, ThreadType Type, uint Address, params object[] Arguments)
        {
            return (string)CallArgs(console, Type == ThreadType.System, JRPC.String, typeof(string), null, 0, Address, 0, true, Arguments);
        }

        public static string CallVMString(this IXboxConsole console, ThreadType Type, string module, int ordinal, params object[] Arguments)
        {
            return (string)CallArgs(console, Type == ThreadType.System, JRPC.String, typeof(string), module, ordinal, 0, 0, true, Arguments);
        }

        private static string SendCommand(IXboxConsole console, string Command)
        {
            string Responce;
            if (connectionId == null)
                throw new Exception("IXboxConsole argument did not connect using JRPC's connect function.");
            try
            {
                console.SendTextCommand(connectionId, Command, out Responce);
                if (Responce.Contains("error="))
                    throw new Exception(Responce.Substring(11));
                if (Responce.Contains("DEBUG"))
                    throw new Exception("JRPC is not installed on the current console");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == UIntToInt(0x82DA0007))
                    throw new Exception("JRPC is not installed on the current console");
                else
                    throw ex;
            }
            return Responce;
        }

        private static object CallArgs(IXboxConsole console, bool SystemThread, uint Type, Type t, string module, int ordinal, uint Address, uint ArraySize, bool VM, params object[] Arguments)
        {
            if (!JRPC.IsValidReturnType(t))
                throw new Exception("Invalid type " + (object)t.Name + Environment.NewLine + "JRPC only supports: " + "bool, byte," +
                    " short, int, long, ushort, uint, ulong, float, double");
            else
            {
                console.ConnectTimeout =  console.ConversationTimeout = 4000000;//set to 400 secounds incase there is a little delay in recving
                string SendCMD = "";
                uint NumOfArgs = 0;
                foreach (object obj in Arguments)
                {
                    bool Done = false;
                    if (obj is uint)
                    {
                        SendCMD += JRPC.Int + "\\" + UIntToInt((uint)obj) + "\\";
                        NumOfArgs += 1;
                        Done = true;
                    }
                    if (obj is int || obj is bool || obj is byte)
                    {
                        if (obj is bool)
                        {
                            SendCMD += JRPC.Int + "\\" + Convert.ToInt32((bool)obj) + "\\";
                        }
                        else
                        {
                            SendCMD += JRPC.Int + "\\" + ((obj is byte) ? Convert.ToByte(obj).ToString() : Convert.ToInt32(obj).ToString()) + "\\";
                        }
                        NumOfArgs += 1;
                        Done = true;
                    }
                    else if (obj is int[] || obj is uint[])
                    {
                        if (!VM)
                        {
                            byte[] Array = IntArrayToByte((int[])obj);
                            SendCMD += JRPC.ByteArray.ToString() + "/" + Array.Length + "\\";
                            for (int i = 0; i < Array.Length; i++)
                                SendCMD += Array[i].ToString("X2");
                            SendCMD += "\\";
                            NumOfArgs += 1;
                        }
                        else
                        {
                            bool isInt = obj is int[];
                            int len;
                            if(isInt)
                            {
                                int[] iarray = (int[])obj;
                                len = iarray.Length;
                            }
                            else
                            {
                                uint[] iarray = (uint[])obj;
                                len = iarray.Length;
                            }
                            int[] Iarray = new int[len];
                            for(int i = 0; i < len; i++)
                            {
                                if(isInt)
                                {
                                    int[] tiarray = (int[])obj;
                                    Iarray[i] = tiarray[i];
                                }
                                else
                                {
                                    uint[] tiarray = (uint[])obj;
                                    Iarray[i] = UIntToInt(tiarray[i]);
                                }
                                SendCMD += JRPC.Int + "\\" + Iarray[i] + "\\";
                                NumOfArgs += 1;
                            }
                        }
                        Done = true;
                    }
                    else if (obj is string)
                    {
                        string Str = (string)obj;
                        SendCMD += JRPC.ByteArray.ToString() + "/" + Str.Length + "\\" + JRPC.ToHexString((string)obj) + "\\";
                        NumOfArgs += 1;
                        Done = true;
                    }
                    else if (obj is double)
                    {
                        double d = (double)obj;
                        SendCMD += JRPC.Float.ToString() + "\\" + d.ToString() + "\\";
                        NumOfArgs += 1;
                        Done = true;
                    }
                    else if (obj is float)
                    {
                        float Fl = (float)obj;
                        SendCMD += JRPC.Float.ToString() + "\\" + Fl.ToString() + "\\";
                        NumOfArgs += 1;
                        Done = true;
                    }
                    else if (obj is float[])
                    {
                        float[] floatArray = (float[])obj;
                        if (!VM)
                        {
                            SendCMD += JRPC.ByteArray.ToString() + "/" + (floatArray.Length * 4).ToString() + "\\";
                            for (int i = 0; i < floatArray.Length; i++)
                            {
                                byte[] bytes = BitConverter.GetBytes(floatArray[i]);
                                Array.Reverse(bytes);
                                for (int q = 0; q < 4; q++)
                                    SendCMD += bytes[q].ToString("X2");
                            }
                            SendCMD += "\\";
                            NumOfArgs += 1;
                        }
                        else
                        {
                            for (int i = 0; i < floatArray.Length; i++)
                            {
                                SendCMD += JRPC.Float.ToString() + "\\" + floatArray[i].ToString() + "\\";
                                NumOfArgs += 1;
                            }
                        }
                        Done = true;
                    }
                    else if (obj is byte[])
                    {
                        byte[] ByteArray = (byte[])obj;
                        SendCMD += JRPC.ByteArray.ToString() + "/" + ByteArray.Length + "\\";
                        for (int i = 0; i < ByteArray.Length; i++)
                            SendCMD += ByteArray[i].ToString("X2");
                        SendCMD += "\\";
                        NumOfArgs += 1;
                        Done = true;
                    }

                    if (!Done)
                    {
                        SendCMD += JRPC.Uint64.ToString() + "\\" + ConvertToUInt64(obj).ToString()+ "\\";
                        NumOfArgs += 1;
                    }
                }
                SendCMD += "\"";

                string startSendCMD = "consolefeatures ver=" + JRPCVersion + " type=" + Type + (SystemThread ? " system" : "") +
                    (module != null ? " module=\"" + module + "\" ord=" + ordinal : "") +
                    (VM ? " VM" : "") +
                    " as=" + ArraySize + " params=\"A\\" + Address.ToString("X") + "\\A\\" + NumOfArgs + "\\" + SendCMD,
                    Responce;
                if (NumOfArgs > 37)
                    throw new Exception("Can not use more than 37 paramaters in a call");

                Responce = SendCommand(console, startSendCMD);
                string Find = "buf_addr=";
                while (Responce.Contains(Find))
                {
                    System.Threading.Thread.Sleep(250);
                    uint address = uint.Parse(Responce.Substring(Responce.find(Find) + Find.Length), System.Globalization.NumberStyles.HexNumber);
                    Responce = SendCommand(console, "consolefeatures " + Find + "0x" + address.ToString("X"));
                }
                console.ConversationTimeout = 2000;//reset the timeout
                console.ConnectTimeout = 5000;

                switch(Type)
                {
                    case 1:/*Int*/
                        uint uVal = uint.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber); 
                        if(t == typeof(uint))
                            return uVal;
                        if (t == typeof(int))
                            return UIntToInt(uVal);
                        if (t == typeof(short))
                            return short.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber); 
                        if (t == typeof(ushort))
                            return ushort.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber); 
                            break;
                    case 2:/*String*/
                        string sString = Responce.Substring(Responce.find(" ") + 1);
                        if (t == typeof(string))
                            return sString;
                        if (t == typeof(char[]))
                            return sString.ToCharArray();
                        break;
                    case 3:/*Float*/
                        if (t == typeof(double))
                            return double.Parse(Responce.Substring(Responce.find(" ") + 1));
                        if (t == typeof(float))
                            return float.Parse(Responce.Substring(Responce.find(" ") + 1));;
                        break;
                    case 4:/*Byte*/
                        byte bByte = byte.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
                        if (t == typeof(byte))
                            return bByte;
                        if (t == typeof(char))
                            return (char)bByte;
                        break;
                    case 8:/*UInt64*/
                        if (t == typeof(long))
                            return long.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
                        if (t == typeof(ulong))
                            return ulong.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
                        break;
                }
                if (Type == 5)//IntArray
                {
                    string String = Responce.Substring(Responce.find(" ") + 1);
                    int Tmp = 0;
                    string Temp = "";
                    uint[] Uarray = new uint[8];
                    foreach (char Char1 in String)
                    {
                        if (Char1 != ',' && Char1 != ';')
                            Temp += Char1.ToString();
                        else
                        {
                            Uarray[Tmp] = uint.Parse(Temp, System.Globalization.NumberStyles.HexNumber);
                            Tmp += 1;
                            Temp = "";
                        }
                        if (Char1 == ';')
                            break;
                    }
                    return Uarray;
                }
                if (Type == 6)//FloatArray
                {
                    string String = Responce.Substring(Responce.find(" ") + 1);
                    int Tmp = 0;
                    string Temp = "";
                    float[] Farray = new float[ArraySize];
                    foreach (char Char1 in String)
                    {
                        if (Char1 != ',' && Char1 != ';')
                            Temp += Char1.ToString();
                        else
                        {
                            Farray[Tmp] = float.Parse(Temp);
                            Tmp += 1;
                            Temp = "";
                        }
                        if (Char1 == ';')
                            break;
                    }
                    return Farray;
                }
                if (Type == 7)//ByteArray
                {
                    string String = Responce.Substring(Responce.find(" ") + 1);
                    int Tmp = 0;
                    string Temp = "";
                    byte[] Barray = new byte[ArraySize];
                    foreach (char Char1 in String)
                    {
                        if (Char1 != ',' && Char1 != ';')
                            Temp += Char1.ToString();
                        else
                        {
                            Barray[Tmp] = byte.Parse(Temp);
                            Tmp += 1;
                            Temp = "";
                        }
                        if (Char1 == ';')
                            break;
                    }
                    return Barray;
                }
                if (Type == JRPC.Uint64Array)
                {
                    string String = Responce.Substring(Responce.find(" ") + 1);
                    int Tmp = 0;
                    string Temp = "";
                    ulong[] ulongArray = new ulong[ArraySize];
                    foreach (char Char1 in String)
                    {
                        if (Char1 != ',' && Char1 != ';')
                            Temp += Char1.ToString();
                        else
                        {
                            ulongArray[Tmp] =ulong.Parse(Temp);
                            Tmp += 1;
                            Temp = "";
                        }
                        if (Char1 == ';')
                            break;
                    }
                    if(t == typeof(ulong))
                        return ulongArray;
                    else if (t == typeof(long))
                    {
                        long[] longArray = new long[ArraySize];
                        for (int i = 0; i < ArraySize; i++)
                            longArray[i] = BitConverter.ToInt64(BitConverter.GetBytes(ulongArray[i]), 0);
                        return longArray;
                    }
                }

                if (Type == JRPC.Void)
                    return 0;
                return ulong.Parse(Responce.Substring(Responce.find(" ") + 1), System.Globalization.NumberStyles.HexNumber);
            }
        }
    }
}
