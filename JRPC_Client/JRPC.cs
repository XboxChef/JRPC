using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using XDevkit;

namespace JRPC_Client
{
    public static class JRPC
    {
        private static readonly uint Byte = 4;
        private static readonly uint ByteArray = 7;
        private static uint connectionId;
        private static readonly uint Float = 3;
        private static readonly uint FloatArray = 6;
        private static readonly uint Int = 1;
        private static readonly uint IntArray = 5;
        private static readonly uint String = 2;
        private static Dictionary<Type, int> StructPrimitiveSizeMap = new Dictionary<Type, int>();
        private static readonly uint Uint64 = 8;
        private static readonly uint Uint64Array = 9;
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
        private static Dictionary<Type, int> ValueTypeSizeMap = new Dictionary<Type, int>()
    {
      {
        typeof (bool),
        4
      },
      {
        typeof (byte),
        1
      },
      {
        typeof (short),
        2
      },
      {
        typeof (int),
        4
      },
      {
        typeof (long),
        8
      },
      {
        typeof (ushort),
        2
      },
      {
        typeof (uint),
        4
      },
      {
        typeof (ulong),
        8
      },
      {
        typeof (float),
        4
      },
      {
        typeof (double),
        8
      }
    };
        private static readonly uint Void = 0;
        public static readonly uint JRPCVersion = 2;

        private static T[] ArrayReturn<T>(this IXboxConsole console, uint Address, uint Size)
        {
            if (Size == 0U)
                return new T[1];
            Type type = typeof(T);
            object obj = new object();
            if (type == typeof(short))
                obj = (object)console.ReadInt16(Address, Size);
            if (type == typeof(ushort))
                obj = (object)console.ReadUInt16(Address, Size);
            if (type == typeof(int))
                obj = (object)console.ReadInt32(Address, Size);
            if (type == typeof(uint))
                obj = (object)console.ReadUInt32(Address, Size);
            if (type == typeof(long))
                obj = (object)console.ReadInt64(Address, Size);
            if (type == typeof(ulong))
                obj = (object)console.ReadUInt64(Address, Size);
            if (type == typeof(float))
                obj = (object)console.ReadFloat(Address, Size);
            if (type == typeof(byte))
                obj = (object)console.GetMemory(Address, Size);
            return (T[])obj;
        }
        private static object CallArgs(IXboxConsole console, bool SystemThread, uint Type, Type t, string module, int ordinal, uint Address, uint ArraySize, params object[] Arguments)
        {
            string str2;
            ulong[] numArray4;
            object[] objArray;
            string text2;
            if (!IsValidReturnType(t))
            {
                objArray = new object[] { "Invalid type ", t.Name, Environment.NewLine, "JRPC only supports: bool, byte, short, int, long, ushort, uint, ulong, float, double" };
                throw new Exception(string.Concat(objArray));
            }
            console.ConnectTimeout = console.ConversationTimeout = 0x3d_0900;
            object[] index = new object[] { "consolefeatures ver=", JRPCVersion, " type=", Type };
            index[4] = SystemThread ? " system" : "";
            if (module == null)
            {
                text2 = "";
            }
            else
            {
                object[] objArray3 = new object[] { " module=\"", module, "\" ord=", ordinal };
                text2 = string.Concat(objArray3);
            }
            index[5] = text2;
            index[6] = " as=";
            index[7] = ArraySize;
            index[8] = " params=\"A\\";
            index[9] = Address.ToString("X");
            index[10] = @"\A\";
            index[11] = Arguments.Length;
            index[12] = @"\";
            string command = string.Concat(index);
            if (Arguments.Length > 0x25)
            {
                throw new Exception("Can not use more than 37 paramaters in a call");
            }
            object[] objArray4 = Arguments;
            int num16 = 0;
            while (true)
            {
                if (num16 < objArray4.Length)
                {
                    string[] strArray;
                    object obj2 = objArray4[num16];
                    bool flag = false;
                    if (obj2 is uint)
                    {
                        object[] objArray5 = new object[] { command, Int, @"\", UIntToInt((uint)obj2), @"\" };
                        command = string.Concat(objArray5);
                        flag = true;
                    }
                    if ((obj2 is int) || obj2 is bool || (obj2 is byte))
                    {
                        if (obj2 is bool flag1)
                        {
                            object[] objArray6 = new object[] { command, Int, "/", Convert.ToInt32((bool)obj2), @"\" };
                            command = string.Concat(objArray6);
                        }
                        else
                        {
                            object[] objArray7 = new object[] { command, Int, @"\" };
                            objArray7[3] = (obj2 is byte) ? Convert.ToByte(obj2).ToString() : Convert.ToInt32(obj2).ToString();
                            objArray7[4] = @"\";
                            command = string.Concat(objArray7);
                        }
                        flag = true;
                    }
                    else if ((obj2 is int[]) || (obj2 is uint[]))
                    {
                        byte[] buffer = IntArrayToByte((int[])obj2);
                        object[] objArray8 = new object[] { command, ByteArray.ToString(), "/", buffer.Length, @"\" };
                        command = string.Concat(objArray8);
                        int num = 0;
                        while (true)
                        {
                            if (num >= buffer.Length)
                            {
                                command = command + @"\";
                                flag = true;
                                break;
                            }
                            command = command + buffer[num].ToString("X2");
                            num++;
                        }
                    }
                    else if (obj2 is string)
                    {
                        object[] objArray9 = new object[] { command, ByteArray.ToString(), "/", ((string)obj2).Length, @"\", ((string)obj2).ToHexString(), @"\" };
                        command = string.Concat(objArray9);
                        flag = true;
                    }
                    else if (obj2 is double)
                    {
                        double num2 = (double)obj2;
                        strArray = new string[] { command, Float.ToString(), @"\", num2.ToString(), @"\" };
                        command = string.Concat(strArray);
                        flag = true;
                    }
                    else if (obj2 is float)
                    {
                        float num3 = (float)obj2;
                        strArray = new string[] { command, Float.ToString(), @"\", num3.ToString(), @"\" };
                        command = string.Concat(strArray);
                        flag = true;
                    }
                    else if (!(obj2 is float[]))
                    {
                        if (obj2 is byte[])
                        {
                            byte[] buffer3 = (byte[])obj2;
                            objArray = new object[] { command, ByteArray.ToString(), "/", buffer3.Length, @"\" };
                            command = string.Concat(objArray);
                            int num6 = 0;
                            while (true)
                            {
                                if (num6 >= buffer3.Length)
                                {
                                    command = command + @"\";
                                    flag = true;
                                    break;
                                }
                                command = command + buffer3[num6].ToString("X2");
                                num6++;
                            }
                        }
                    }
                    else
                    {
                        float[] numArray = (float[])obj2;
                        strArray = new string[] { command, ByteArray.ToString(), "/", (numArray.Length * 4).ToString(), @"\" };
                        command = string.Concat(strArray);
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 >= numArray.Length)
                            {
                                command = command + @"\";
                                flag = true;
                                break;
                            }
                            byte[] bytes = BitConverter.GetBytes(numArray[num4]);
                            Array.Reverse(bytes);
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 >= 4)
                                {
                                    num4++;
                                    break;
                                }
                                command = command + bytes[num5].ToString("X2");
                                num5++;
                            }
                        }
                    }
                    if (!flag)
                    {
                        strArray = new string[] { command, Uint64.ToString(), @"\", ConvertToUInt64(obj2).ToString(), @"\" };
                        command = string.Concat(strArray);
                    }
                    num16++;
                    continue;
                }
                command = command + "\"";
                str2 = SendCommand(console, command);
                string str4 = "buf_addr=";
                while (true)
                {
                    if (str2.Contains(str4))
                    {
                        Thread.Sleep(250);
                        str2 = SendCommand(console, "consolefeatures " + str4 + "0x" + uint.Parse(str2.Substring(str2.find(str4) + str4.Length), NumberStyles.HexNumber).ToString("X"));
                        continue;
                    }
                    console.ConversationTimeout = 0x7d0;
                    console.ConnectTimeout = 0x1388;
                    switch (Type)
                    {
                        case 1:
                            {
                                uint num8 = uint.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                                if (ReferenceEquals(t, typeof(uint)))
                                {
                                    return num8;
                                }
                                if (ReferenceEquals(t, typeof(int)))
                                {
                                    return UIntToInt(num8);
                                }
                                if (ReferenceEquals(t, typeof(short)))
                                {
                                    return short.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                                }
                                if (!ReferenceEquals(t, typeof(ushort)))
                                {
                                    break;
                                }
                                return ushort.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            }
                        case 2:
                            {
                                string str5 = str2.Substring(str2.find(" ") + 1);
                                if (ReferenceEquals(t, typeof(string)))
                                {
                                    return str5;
                                }
                                if (!ReferenceEquals(t, typeof(char[])))
                                {
                                    break;
                                }
                                return str5.ToCharArray();
                            }
                        case 3:
                            if (ReferenceEquals(t, typeof(double)))
                            {
                                return double.Parse(str2.Substring(str2.find(" ") + 1));
                            }
                            if (!ReferenceEquals(t, typeof(float)))
                            {
                                break;
                            }
                            return float.Parse(str2.Substring(str2.find(" ") + 1));

                        case 4:
                            {
                                byte num9 = byte.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                                if (ReferenceEquals(t, typeof(byte)))
                                {
                                    return num9;
                                }
                                if (!ReferenceEquals(t, typeof(char)))
                                {
                                    break;
                                }
                                return (char)num9;
                            }
                        case 8:
                            if (ReferenceEquals(t, typeof(long)))
                            {
                                return long.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);
                            }
                            if (!ReferenceEquals(t, typeof(ulong)))
                            {
                                break;
                            }
                            return ulong.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber);

                        default:
                            break;
                    }
                    if (Type != 5)
                    {
                        if (Type != 6)
                        {
                            if (Type != 7)
                            {
                                if (Type != Uint64Array)
                                {
                                    goto TR_0026;
                                }
                                else
                                {
                                    int num13 = 0;
                                    string s = "";
                                    numArray4 = new ulong[ArraySize];
                                    foreach (char ch4 in str2.Substring(str2.find(" ") + 1))
                                    {
                                        if ((ch4 != ',') && (ch4 != ';'))
                                        {
                                            s = s + ch4.ToString();
                                        }
                                        else
                                        {
                                            numArray4[num13] = ulong.Parse(s);
                                            num13++;
                                            s = "";
                                        }
                                        if (ch4 == ';')
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int num12 = 0;
                                string s = "";
                                byte[] buffer4 = new byte[ArraySize];
                                foreach (char ch3 in str2.Substring(str2.find(" ") + 1))
                                {
                                    if ((ch3 != ',') && (ch3 != ';'))
                                    {
                                        s = s + ch3.ToString();
                                    }
                                    else
                                    {
                                        buffer4[num12] = byte.Parse(s);
                                        num12++;
                                        s = "";
                                    }
                                    if (ch3 == ';')
                                    {
                                        break;
                                    }
                                }
                                return buffer4;
                            }
                        }
                        else
                        {
                            int num11 = 0;
                            string s = "";
                            float[] numArray3 = new float[ArraySize];
                            foreach (char ch2 in str2.Substring(str2.find(" ") + 1))
                            {
                                if ((ch2 != ',') && (ch2 != ';'))
                                {
                                    s = s + ch2.ToString();
                                }
                                else
                                {
                                    numArray3[num11] = float.Parse(s);
                                    num11++;
                                    s = "";
                                }
                                if (ch2 == ';')
                                {
                                    break;
                                }
                            }
                            return numArray3;
                        }
                    }
                    else
                    {
                        int num10 = 0;
                        string s = "";
                        uint[] numArray2 = new uint[8];
                        foreach (char ch in str2.Substring(str2.find(" ") + 1))
                        {
                            if ((ch != ',') && (ch != ';'))
                            {
                                s = s + ch.ToString();
                            }
                            else
                            {
                                numArray2[num10] = uint.Parse(s, NumberStyles.HexNumber);
                                num10++;
                                s = "";
                            }
                            if (ch == ';')
                            {
                                break;
                            }
                        }
                        return numArray2;
                    }
                    break;
                }
                break;
            }
            if (ReferenceEquals(t, typeof(ulong)))
            {
                return numArray4;
            }
            if (ReferenceEquals(t, typeof(long)))
            {
                long[] numArray5 = new long[ArraySize];
                for (int i = 0; i < ArraySize; i++)
                {
                    numArray5[i] = BitConverter.ToInt64(BitConverter.GetBytes(numArray4[i]), 0);
                }
                return numArray5;
            }
        TR_0026:
            return ((Type != Void) ? ((object)ulong.Parse(str2.Substring(str2.find(" ") + 1), NumberStyles.HexNumber)) : ((object)0));
        }

        private static byte[] IntArrayToByte(int[] iArray)
        {
            byte[] numArray = new byte[iArray.Length * 4];
            int index1 = 0;
            int num = 0;
            while (index1 < iArray.Length)
            {
                for (int index2 = 0; index2 < 4; ++index2)
                    numArray[num + index2] = BitConverter.GetBytes(iArray[index1])[index2];
                ++index1;
                num += 4;
            }
            return numArray;
        }

        private static void ReverseBytes(byte[] buffer, int groupSize)
        {
            if (buffer.Length % groupSize != 0)
                throw new ArgumentException("Group size must be a multiple of the buffer length", nameof(groupSize));
            for (int index1 = 0; index1 < buffer.Length; index1 += groupSize)
            {
                int index2 = index1;
                for (int index3 = index1 + groupSize - 1; index2 < index3; --index3)
                {
                    byte num = buffer[index2];
                    buffer[index2] = buffer[index3];
                    buffer[index3] = num;
                    ++index2;
                }
            }
        }

        private static string SendCommand(IXboxConsole console, string Command)
        {
            int connectionId = (int)JRPC.connectionId;
            string Response;
            try
            {
                console.SendTextCommand(JRPC.connectionId, Command, out Response);
                if (Response.Contains("error="))
                    throw new Exception(Response.Substring(11));
                if (Response.Contains("DEBUG"))
                    throw new Exception("JRPC is not installed on the current console");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == JRPC.UIntToInt(2195324935U))
                    throw new Exception("JRPC is not installed on the current console");
                throw ex;
            }
            return Response;
        }

        private static uint TypeToType<T>(bool Array) where T : struct
        {
            Type type = typeof(T);
            if (type == typeof(int) || type == typeof(uint) || (type == typeof(short) || type == typeof(ushort)))
                return Array ? JRPC.IntArray : JRPC.Int;
            if (type == typeof(string) || type == typeof(char[]))
                return JRPC.String;
            return type == typeof(float) || type == typeof(double) ? (Array ? JRPC.FloatArray : JRPC.Float) : (type == typeof(byte) || type == typeof(char) ? (Array ? JRPC.ByteArray : JRPC.Byte) : ((type == typeof(ulong) || type == typeof(long)) && Array ? JRPC.Uint64Array : JRPC.Uint64));
        }

        private static int UIntToInt(uint Value) => BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);

        internal static ulong ConvertToUInt64(object o)
        {
            switch (o)
            {
                case bool flag:
                    return flag ? 1UL : 0UL;
                case byte num:
                    return (ulong)num;
                case short num:
                    return (ulong)num;
                case int num:
                    return (ulong)num;
                case long num:
                    return (ulong)num;
                case ushort num:
                    return (ulong)num;
                case uint num:
                    return (ulong)num;
                case ulong num:
                    return num;
                case float num:
                    return (ulong)BitConverter.DoubleToInt64Bits((double)num);
                case double num:
                    return (ulong)BitConverter.DoubleToInt64Bits(num);
                default:
                    return 0;
            }
        }

        internal static bool IsValidReturnType(Type t) => JRPC.ValidReturnTypes.Contains(t);

        internal static bool IsValidStructType(Type t) => !t.IsPrimitive && t.IsValueType;

        public static T Call<T>(this IXboxConsole console, uint Address, params object[] Arguments) where T : struct => (T)JRPC.CallArgs(console, true, JRPC.TypeToType<T>(false), typeof(T), (string)null, 0, Address, 0U, Arguments);

        public static T Call<T>(
          this IXboxConsole console,
          string module,
          int ordinal,
          params object[] Arguments)
          where T : struct
        {
            return (T)JRPC.CallArgs(console, true, JRPC.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments);
        }

        public static T Call<T>(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          uint Address,
          params object[] Arguments)
          where T : struct
        {
            return (T)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(false), typeof(T), (string)null, 0, Address, 0U, Arguments);
        }

        public static T Call<T>(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          string module,
          int ordinal,
          params object[] Arguments)
          where T : struct
        {
            return (T)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(false), typeof(T), module, ordinal, 0U, 0U, Arguments);
        }

        public static T[] CallArray<T>(
          this IXboxConsole console,
          uint Address,
          uint ArraySize,
          params object[] Arguments)
          where T : struct
        {
            return ArraySize == 0U ? new T[1] : (T[])JRPC.CallArgs(console, true, JRPC.TypeToType<T>(true), typeof(T), (string)null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(
          this IXboxConsole console,
          string module,
          int ordinal,
          uint ArraySize,
          params object[] Arguments)
          where T : struct
        {
            return ArraySize == 0U ? new T[1] : (T[])JRPC.CallArgs(console, true, JRPC.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          uint Address,
          uint ArraySize,
          params object[] Arguments)
          where T : struct
        {
            return ArraySize == 0U ? new T[1] : (T[])JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(true), typeof(T), (string)null, 0, Address, ArraySize, Arguments);
        }

        public static T[] CallArray<T>(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          string module,
          int ordinal,
          uint ArraySize,
          params object[] Arguments)
          where T : struct
        {
            return ArraySize == 0U ? new T[1] : (T[])JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.TypeToType<T>(true), typeof(T), module, ordinal, 0U, ArraySize, Arguments);
        }

        public static string CallString(
          this IXboxConsole console,
          uint Address,
          params object[] Arguments)
        {
            return (string)JRPC.CallArgs(console, true, JRPC.String, typeof(string), (string)null, 0, Address, 0U, Arguments);
        }

        public static string CallString(
          this IXboxConsole console,
          string module,
          int ordinal,
          params object[] Arguments)
        {
            return (string)JRPC.CallArgs(console, true, JRPC.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
        }

        public static string CallString(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          uint Address,
          params object[] Arguments)
        {
            return (string)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.String, typeof(string), (string)null, 0, Address, 0U, Arguments);
        }

        public static string CallString(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          string module,
          int ordinal,
          params object[] Arguments)
        {
            return (string)JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.String, typeof(string), module, ordinal, 0U, 0U, Arguments);
        }

        public static void CallVoid(this IXboxConsole console, uint Address, params object[] Arguments) => JRPC.CallArgs(console, true, JRPC.Void, typeof(void), (string)null, 0, Address, 0U, Arguments);

        public static void CallVoid(
          this IXboxConsole console,
          string module,
          int ordinal,
          params object[] Arguments)
        {
            JRPC.CallArgs(console, true, JRPC.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
        }

        public static void CallVoid(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          uint Address,
          params object[] Arguments)
        {
            JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.Void, typeof(void), (string)null, 0, Address, 0U, Arguments);
        }

        public static void CallVoid(
          this IXboxConsole console,
          JRPC.ThreadType Type,
          string module,
          int ordinal,
          params object[] Arguments)
        {
            JRPC.CallArgs(console, Type == JRPC.ThreadType.System, JRPC.Void, typeof(void), module, ordinal, 0U, 0U, Arguments);
        }

        public static bool Connect(
          this IXboxConsole console,
          out IXboxConsole Console,
          string XboxNameOrIP = "default")
        {
            if (XboxNameOrIP == "default")
                XboxNameOrIP = new XboxManager().DefaultConsole;
            IXboxConsole xboxConsole = new XboxManager().OpenConsole(XboxNameOrIP);
            int num = 0;
            bool flag = false;
            while (!flag)
            {
                try
                {
                    JRPC.connectionId = xboxConsole.OpenConnection((string)null);
                    flag = true;
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == JRPC.UIntToInt(2195325184U))
                    {
                        if (num >= 3)
                        {
                            Console = xboxConsole;
                            return false;
                        }
                        ++num;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Console = xboxConsole;
                        return false;
                    }
                }
            }
            Console = xboxConsole;
            return true;
        }

        public static string ConsoleType(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=17 params=\"A\\0\\A\\0\\\"";
            string String = JRPC.SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static void constantMemorySet(this IXboxConsole console, uint Address, uint Value) => JRPC.constantMemorySetting(console, Address, Value, false, 0U, false, 0U);

        public static void constantMemorySet(
          this IXboxConsole console,
          uint Address,
          uint Value,
          uint TitleID)
        {
            JRPC.constantMemorySetting(console, Address, Value, false, 0U, true, TitleID);
        }

        public static void constantMemorySet(
          this IXboxConsole console,
          uint Address,
          uint Value,
          uint IfValue,
          uint TitleID)
        {
            JRPC.constantMemorySetting(console, Address, Value, true, IfValue, true, TitleID);
        }

        public static void constantMemorySetting(
          IXboxConsole console,
          uint Address,
          uint Value,
          bool useIfValue,
          uint IfValue,
          bool usetitleID,
          uint TitleID)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=18 params=\"A\\" + Address.ToString("X") + "\\A\\5\\" + (object)JRPC.Int + "\\" + (object)JRPC.UIntToInt(Value) + "\\" + (object)JRPC.Int + "\\" + (object)(useIfValue ? 1 : 0) + "\\" + (object)JRPC.Int + "\\" + (object)IfValue + "\\" + (object)JRPC.Int + "\\" + (object)(usetitleID ? 1 : 0) + "\\" + (object)JRPC.Int + "\\" + (object)JRPC.UIntToInt(TitleID) + "\\\"";
            JRPC.SendCommand(console, Command);
        }

        public static int find(this string String, string _Ptr)
        {
            if (_Ptr.Length == 0 || String.Length == 0)
                return -1;
            for (int index1 = 0; index1 < String.Length; ++index1)
            {
                if ((int)String[index1] == (int)_Ptr[0])
                {
                    bool flag = true;
                    for (int index2 = 0; index2 < _Ptr.Length; ++index2)
                    {
                        if ((int)String[index1 + index2] != (int)_Ptr[index2])
                            flag = false;
                    }
                    if (flag)
                        return index1;
                }
            }
            return -1;
        }

        public static string GetCPUKey(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=10 params=\"A\\0\\A\\0\\\"";
            string String = JRPC.SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static uint GetKernalVersion(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=13 params=\"A\\0\\A\\0\\\"";
            string String = JRPC.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1));
        }

        public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length)
        {
            uint BytesRead = 0;
            byte[] Data = new byte[Length];
            console.DebugTarget.GetMemory(Address, Length, Data, out BytesRead);
            console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
            return Data;
        }

        public static uint GetTemperature(
          this IXboxConsole console,
          JRPC.TemperatureType TemperatureType)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=15 params=\"A\\0\\A\\1\\" + (object)JRPC.Int + "\\" + (object)(int)TemperatureType + "\\\"";
            string String = JRPC.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static void Push(this byte[] InArray, out byte[] OutArray, byte Value)
        {
            OutArray = new byte[InArray.Length + 1];
            InArray.CopyTo((Array)OutArray, 0);
            OutArray[InArray.Length] = Value;
        }

        public static bool ReadBool(this IXboxConsole console, uint Address) => console.GetMemory(Address, 1U)[0] != (byte)0;

        public static byte ReadByte(this IXboxConsole console, uint Address) => console.GetMemory(Address, 1U)[0];

        public static float ReadFloat(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            JRPC.ReverseBytes(memory, 4);
            return BitConverter.ToSingle(memory, 0);
        }

        public static float[] ReadFloat(this IXboxConsole console, uint Address, uint ArraySize)
        {
            float[] numArray = new float[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            JRPC.ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToSingle(memory, index * 4);
            return numArray;
        }

        public static short ReadInt16(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 2U);
            JRPC.ReverseBytes(memory, 2);
            return BitConverter.ToInt16(memory, 0);
        }

        public static short[] ReadInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            short[] numArray = new short[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 2U);
            JRPC.ReverseBytes(memory, 2);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToInt16(memory, index * 2);
            return numArray;
        }

        public static int ReadInt32(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            JRPC.ReverseBytes(memory, 4);
            return BitConverter.ToInt32(memory, 0);
        }

        public static int[] ReadInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            int[] numArray = new int[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            JRPC.ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToInt32(memory, index * 4);
            return numArray;
        }

        public static long ReadInt64(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 8U);
            JRPC.ReverseBytes(memory, 8);
            return BitConverter.ToInt64(memory, 0);
        }

        public static long[] ReadInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            long[] numArray = new long[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 8U);
            JRPC.ReverseBytes(memory, 8);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = (long)BitConverter.ToUInt32(memory, index * 8);
            return numArray;
        }

        public static sbyte ReadSByte(this IXboxConsole console, uint Address) => (sbyte)console.GetMemory(Address, 1U)[0];

        public static string ReadString(this IXboxConsole console, uint Address, uint size) => Encoding.UTF8.GetString(console.GetMemory(Address, size));

        public static ushort ReadUInt16(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 2U);
            JRPC.ReverseBytes(memory, 2);
            return BitConverter.ToUInt16(memory, 0);
        }

        public static ushort[] ReadUInt16(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ushort[] numArray = new ushort[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 2U);
            JRPC.ReverseBytes(memory, 2);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToUInt16(memory, index * 2);
            return numArray;
        }

        public static uint ReadUInt32(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 4U);
            JRPC.ReverseBytes(memory, 4);
            return BitConverter.ToUInt32(memory, 0);
        }

        public static uint[] ReadUInt32(this IXboxConsole console, uint Address, uint ArraySize)
        {
            uint[] numArray = new uint[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 4U);
            JRPC.ReverseBytes(memory, 4);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = BitConverter.ToUInt32(memory, index * 4);
            return numArray;
        }

        public static ulong ReadUInt64(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory(Address, 8U);
            JRPC.ReverseBytes(memory, 8);
            return BitConverter.ToUInt64(memory, 0);
        }

        public static ulong[] ReadUInt64(this IXboxConsole console, uint Address, uint ArraySize)
        {
            ulong[] numArray = new ulong[ArraySize];
            byte[] memory = console.GetMemory(Address, ArraySize * 8U);
            JRPC.ReverseBytes(memory, 8);
            for (int index = 0; (long)index < (long)ArraySize; ++index)
                numArray[index] = (ulong)BitConverter.ToUInt32(memory, index * 8);
            return numArray;
        }

        public static uint ResolveFunction(this IXboxConsole console, string ModuleName, uint Ordinal)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=9 params=\"A\\0\\A\\2\\" + (object)JRPC.String + "/" + (object)ModuleName.Length + "\\" + ModuleName.ToHexString() + "\\" + (object)JRPC.Int + "\\" + (object)Ordinal + "\\\"";
            string String = JRPC.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static void SetLeds(
          this IXboxConsole console,
          JRPC.LEDState Top_Left,
          JRPC.LEDState Top_Right,
          JRPC.LEDState Bottom_Left,
          JRPC.LEDState Bottom_Right)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=14 params=\"A\\0\\A\\4\\" + (object)JRPC.Int + "\\" + (object)(uint)Top_Left + "\\" + (object)JRPC.Int + "\\" + (object)(uint)Top_Right + "\\" + (object)JRPC.Int + "\\" + (object)(uint)Bottom_Left + "\\" + (object)JRPC.Int + "\\" + (object)(uint)Bottom_Right + "\\\"";
            JRPC.SendCommand(console, Command);
        }

        public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data) => console.DebugTarget.SetMemory(Address, (uint)Data.Length, Data, out uint _);

        public static void ShutDownConsole(this IXboxConsole console)
        {
            try
            {
                string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=11 params=\"A\\0\\A\\0\\\"";
                JRPC.SendCommand(console, Command);
            }
            catch
            {
            }
        }

        public static byte[] ToByteArray(this string String)
        {
            byte[] numArray = new byte[String.Length + 1];
            for (int index = 0; index < String.Length; ++index)
                numArray[index] = (byte)String[index];
            return numArray;
        }

        public static string ToHexString(this string String)
        {
            string str = string.Empty;
            foreach (byte num in String)
                str += num.ToString("X2");
            return str;
        }

        public static byte[] ToWCHAR(this string String) => JRPC.WCHAR(String);

        public static byte[] WCHAR(string String)
        {
            byte[] numArray = new byte[String.Length * 2 + 2];
            int index = 1;
            foreach (byte num in String)
            {
                numArray[index] = num;
                index += 2;
            }
            return numArray;
        }

        public static void WriteBool(this IXboxConsole console, uint Address, bool Value) => console.SetMemory(Address, new byte[1]
        {
      Value ? (byte) 1 : (byte) 0
        });

        public static void WriteBool(this IXboxConsole console, uint Address, bool[] Value)
        {
            byte[] OutArray = new byte[0];
            for (int index = 0; index < Value.Length; ++index)
                OutArray.Push(out OutArray, Value[index] ? (byte)1 : (byte)0);
            console.SetMemory(Address, OutArray);
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte Value) => console.SetMemory(Address, new byte[1]
        {
      Value
        });

        public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value) => console.SetMemory(Address, Value);

        public static void WriteFloat(this IXboxConsole console, uint Address, float Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse((Array)bytes);
            console.SetMemory(Address, bytes);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            JRPC.ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 2);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt16(this IXboxConsole console, uint Address, short[] Value)
        {
            byte[] numArray = new byte[Value.Length * 2];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 2);
            JRPC.ReverseBytes(numArray, 2);
            console.SetMemory(Address, numArray);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 4);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt32(this IXboxConsole console, uint Address, int[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            JRPC.ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 8);
            console.SetMemory(Address, bytes);
        }

        public static void WriteInt64(this IXboxConsole console, uint Address, long[] Value)
        {
            byte[] numArray = new byte[Value.Length * 8];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 8);
            JRPC.ReverseBytes(numArray, 8);
            console.SetMemory(Address, numArray);
        }

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte Value) => console.SetMemory(Address, new byte[1]
        {
      BitConverter.GetBytes((short) Value)[0]
        });

        public static void WriteSByte(this IXboxConsole console, uint Address, sbyte[] Value)
        {
            byte[] OutArray = new byte[0];
            foreach (byte num in Value)
                OutArray.Push(out OutArray, num);
            console.SetMemory(Address, OutArray);
        }

        public static void WriteString(this IXboxConsole console, uint Address, string String)
        {
            byte[] OutArray = new byte[0];
            foreach (byte num in String)
                OutArray.Push(out OutArray, num);
            OutArray.Push(out OutArray, (byte)0);
            console.SetMemory(Address, OutArray);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 2);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort[] Value)
        {
            byte[] numArray = new byte[Value.Length * 2];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 2);
            JRPC.ReverseBytes(numArray, 2);
            console.SetMemory(Address, numArray);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 4);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint[] Value)
        {
            byte[] numArray = new byte[Value.Length * 4];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 4);
            JRPC.ReverseBytes(numArray, 4);
            console.SetMemory(Address, numArray);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            JRPC.ReverseBytes(bytes, 8);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong[] Value)
        {
            byte[] numArray = new byte[Value.Length * 8];
            for (int index = 0; index < Value.Length; ++index)
                BitConverter.GetBytes(Value[index]).CopyTo((Array)numArray, index * 8);
            JRPC.ReverseBytes(numArray, 8);
            console.SetMemory(Address, numArray);
        }

        public static uint XamGetCurrentTitleId(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=16 params=\"A\\0\\A\\0\\\"";
            string String = JRPC.SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static string XboxIP(this IXboxConsole console)
        {
            byte[] bytes = BitConverter.GetBytes(console.IPAddress);
            Array.Reverse((Array)bytes);
            return new IPAddress(bytes).ToString();
        }

        public static void XNotify(this IXboxConsole console, string Text) => console.XNotify(Text, 34U);

        public static void XNotify(this IXboxConsole console, string Text, uint Type)
        {
            string Command = "consolefeatures ver=" + (object)JRPC.JRPCVersion + " type=12 params=\"A\\0\\A\\2\\" + (object)JRPC.String + "/" + (object)Text.Length + "\\" + Text.ToHexString() + "\\" + (object)JRPC.Int + "\\" + (object)Type + "\\\"";
            JRPC.SendCommand(console, Command);
        }

        public enum LEDState
        {
            OFF = 0,
            RED = 8,
            GREEN = 128, // 0x00000080
            ORANGE = 136, // 0x00000088
        }

        public enum TemperatureType
        {
            CPU,
            GPU,
            EDRAM,
            MotherBoard,
        }

        public enum ThreadType
        {
            System,
            Title,
        }
    }
}
