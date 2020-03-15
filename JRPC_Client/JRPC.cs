namespace JRPC_Client
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using XDevkit;

    public class JRPC
    {
        public static bool activeConnection;
        private static uint Byte = 4;
        private static uint ByteArray = 7;
        private static TcpClient client;
        private static uint Float = 3;
        private static uint FloatArray = 6;
        private static uint Int = 1;
        private static uint IntArray = 5;
        private static BinaryReader JRPC_BReader;
        private static StreamReader JRPC_Reader;
        private static StreamWriter JRPC_Writer;
        private static byte[] myBuff = new byte[0x100];
        private static uint outInt;
        private static uint String = 2;
        private static short Temp16;
        private static int Temp32;
        private static long Temp64;
        private static uint Uint64 = 8;
        private static ushort uTemp16;
        private static uint uTemp32;
        private static ulong uTemp64;
        private static uint Void;
        public static IXboxConsole xbConsole;
        private static uint xbConsolenection;
        public static bool xbdmConnection;
        public static IXboxManager xbManager;

        public void AND_Int16(uint Offset, short input)
        {
            Temp16 = ReadInt16(Offset);
            Temp16 = (short) (Temp16 & input);
            WriteInt16(Offset, Temp16);
        }

        public void AND_Int32(uint Offset, int input)
        {
            Temp32 = ReadInt32(Offset);
            Temp32 &= input;
            WriteInt32(Offset, Temp32);
        }

        public void AND_Int64(uint Offset, long input)
        {
            Temp64 = ReadInt64(Offset);
            Temp64 &= input;
            WriteInt64(Offset, Temp64);
        }

        public void AND_UInt16(uint Offset, ushort input)
        {
            uTemp16 = ReadUInt16(Offset);
            uTemp16 = (ushort) (uTemp16 & input);
            WriteUInt16(Offset, uTemp16);
        }

        public void AND_UInt32(uint Offset, uint input)
        {
            uTemp32 = ReadUInt32(Offset);
            uTemp32 &= input;
            WriteUInt32(Offset, uTemp32);
        }

        public void AND_UInt64(uint Offset, ulong input)
        {
            uTemp64 = ReadUInt64(Offset);
            uTemp64 &= input;
            WriteUInt64(Offset, uTemp64);
        }

        public byte CallByte(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return 0;
            }
            SendCMD(Offset, Byte, Arguments);
            return Convert.ToByte(Recv());
        }

        public byte[] CallByteArray(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return new byte[8];
            }
            SendCMD(Offset, ByteArray, Arguments);
            string str = Recv();
            int index = 0;
            string s = "";
            byte[] buffer = new byte[8];
            foreach (char ch in str)
            {
                if (ch == ';')
                {
                    return buffer;
                }
                if (ch != ',')
                {
                    s = s + ch.ToString();
                }
                else
                {
                    buffer[index] = byte.Parse(s);
                    index++;
                    s = "";
                }
            }
            return buffer;
        }

        public float CallFloat(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return 0f;
            }
            SendCMD(Offset, Float, Arguments);
            return float.Parse(Recv(), NumberStyles.Float);
        }

        public float[] CallFloatArray(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return new float[8];
            }
            SendCMD(Offset, FloatArray, Arguments);
            string str = Recv();
            int index = 0;
            string s = "";
            float[] numArray = new float[8];
            foreach (char ch in str)
            {
                if (ch == ';')
                {
                    return numArray;
                }
                if (ch != ',')
                {
                    s = s + ch.ToString();
                }
                else
                {
                    numArray[index] = float.Parse(s, NumberStyles.Float);
                    index++;
                    s = "";
                }
            }
            return numArray;
        }

        public string CallString(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return "";
            }
            SendCMD(Offset, String, Arguments);
            return Recv();
        }

        private void CallStruct(string Commands)
        {
            Send(Commands, true);
        }

        public uint CallUInt(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return 0;
            }
            SendCMD(Offset, Int, Arguments);
            return uint.Parse(Recv(), NumberStyles.HexNumber);
        }

        public ulong CallUInt64(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return 0L;
            }
            SendCMD(Offset, Int, Arguments);
            return ulong.Parse(Recv(), NumberStyles.HexNumber);
        }

        public uint[] CallUIntArray(uint Offset, params object[] Arguments)
        {
            if (!activeConnection)
            {
                return new uint[8];
            }
            SendCMD(Offset, IntArray, Arguments);
            string str = Recv();
            int index = 0;
            string s = "";
            uint[] numArray = new uint[8];
            foreach (char ch in str)
            {
                if (ch == ';')
                {
                    return numArray;
                }
                if (ch != ',')
                {
                    s = s + ch.ToString();
                }
                else
                {
                    numArray[index] = uint.Parse(s, NumberStyles.HexNumber);
                    index++;
                    s = "";
                }
            }
            return numArray;
        }

        public void CallVoid(uint Offset, params object[] Arguments)
        {
            if (activeConnection)
            {
                SendCMD(Offset, Void, Arguments);
                Recv();
            }
        }

        public static void Connect(string Console = "", bool ShowPopupMessages = false)
        {
            string[] strArray = Console.Split(new char[] { '.' });
            if (!xbdmConnection)
            {
                string str;
                string str2;
                xbManager = new XboxManager();
                xbConsole = xbManager.OpenConsole((Console == "") ? xbManager.DefaultConsole : Console);
                try
                {
                    xbConsolenection = xbConsole.OpenConnection(null);
                }
                catch (Exception)
                {
                    return;
                }
                if (xbConsole.DebugTarget.IsDebuggerConnected(out str, out str2))
                {
                    xbdmConnection = true;
                }
                else
                {
                    xbConsole.DebugTarget.ConnectAsDebugger("JRPC", XboxDebugConnectFlags.Force);
                    if (!xbConsole.DebugTarget.IsDebuggerConnected(out str, out str2))
                    {
                        //MessageBox.Show("tried to connect to " + ((Console == "") ? xbManager.DefaultConsole : Console) + " and failed :(");
                    }
                    else
                    {
                        xbdmConnection = true;
                    }
                }
            }
            if (xbdmConnection)
            {
                client = new TcpClient((Console == "") ? XboxIP() : Console, 0x581);
                client.NoDelay = true;
                client.LingerState.Enabled = true;
                client.LingerState.LingerTime = 5;
                client.SendBufferSize = 0x2134;
                client.ReceiveBufferSize = 0x3e8;
                JRPC_BReader = new BinaryReader(client.GetStream());
                JRPC_Reader = new StreamReader(client.GetStream());
                JRPC_Writer = new StreamWriter(client.GetStream());
                if (Recv() != "JRPC2 connected")
                {
                    activeConnection = false;
                    if (ShowPopupMessages)
                    {
                        //MessageBox.Show("JRPC Couldn't Connect!\n\nPlease Try Again!");
                    }
                }
                else
                {
                    activeConnection = true;
                    if (ShowPopupMessages)
                    {
                        //MessageBox.Show("JRPC Connected to " + xbManager.DefaultConsole);
                    }
                }
            }
            else
            {
                activeConnection = false;
                if (ShowPopupMessages)
                {
                    //MessageBox.Show("JRPC Couldn't Connect!\n\nPlease Try Again!");
                }
            }
        }

        public string ConsoleType()
        {
            if (!activeConnection)
            {
                return "Unknown";
            }
            ResetStruct();
            SpawnEndian((uint) 0x11);
            CallStruct(@"A\0\T\17\A\0\");
            return Recv();
        }

       
	public string CPUKey()
        {
            if (!activeConnection)
            {
                return "";
            }
            CallStruct(@"A\0\T\10\A\0\");
            return Recv();
        }


        public string GetKernalVersion()
        {
            if (!activeConnection)
            {
                return "";
            }
            CallStruct(@"A\0\T\13\A\0\");
            return Recv();
        }
        public byte[] GetMemory(uint address, uint length)
        {
            uint g;
            byte[] data = new byte[length];
            xbConsole.DebugTarget.GetMemory(address, length, data, out g);
            xbConsole.DebugTarget.InvalidateMemoryCache(true, address, length);
            return data;
        }
        public byte[] GetMemory(uint Offset, uint length)
        {
            uint num2;
            byte[] data = new byte[length];
            if (length < 450)
            {
                CallStruct(string.Concat(new object[] { @"A\", Offset.ToString("X8"), @"\T\19\A\1\", Int, @"\", length, @"\" }));
                string str = Recv();
                for (int i = 0; i < length; i++)
                {
                    data[i] = byte.Parse(str[i * 2] + str[(i * 2) + 1], NumberStyles.HexNumber);
                }
                return data;
            }
            xbConsole.DebugTarget.GetMemory(Offset, length, data, out num2);
            return data;
        }

        public uint getTemperature(Temperature temperature)
        {
            if (!activeConnection)
            {
                return 0;
            }
            CallStruct(string.Concat(new object[] { @"A\0\T\15\A\1\", Int, @"\", temperature, @"\" }));
            return uint.Parse(Recv(), NumberStyles.HexNumber);
        }

        private byte[] IntArrayToByte(int[] iArray)
        {
            byte[] buffer = new byte[iArray.Length * 4];
            int index = 0;
            for (int i = 0; index < iArray.Length; i += 4)
            {
                for (int j = 0; j < 4; j++)
                {
                    buffer[i + j] = BitConverter.GetBytes(iArray[index])[j];
                }
                index++;
            }
            return buffer;
        }

        public void NOP(uint Offset)
        {
            WriteUInt32(Offset, 0x60000000);
        }

        public byte[] ObjectToBytes(object obj, int Size)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream();
            formatter.Serialize(serializationStream, obj);
            byte[] buffer = serializationStream.ToArray();
            byte[] buffer2 = buffer;
            for (int i = 0; i < buffer.Length; i += Size)
            {
                int index = i;
                for (int j = Size - 1; index < (i + Size); j -= 2)
                {
                    if (index >= buffer.Length)
                    {
                        break;
                    }
                    buffer[index] = buffer2[index + j];
                    index++;
                }
            }
            return buffer;
        }

        public void OR_Int16(uint Offset, short input)
        {
            Temp16 = ReadInt16(Offset);
            Temp16 = (short) (Temp16 | input);
            WriteInt16(Offset, Temp16);
        }

        public void OR_Int32(uint Offset, int input)
        {
            Temp32 = ReadInt32(Offset);
            Temp32 |= input;
            WriteInt32(Offset, Temp32);
        }

        public void OR_Int64(uint Offset, long input)
        {
            Temp64 = ReadInt64(Offset);
            Temp64 |= input;
            WriteInt64(Offset, Temp64);
        }

        public void OR_UInt16(uint Offset, ushort input)
        {
            uTemp16 = ReadUInt16(Offset);
            uTemp16 = (ushort) (uTemp16 | input);
            WriteUInt16(Offset, uTemp16);
        }

        public void OR_UInt32(uint Offset, uint input)
        {
            uTemp32 = ReadUInt32(Offset);
            uTemp32 |= input;
            WriteUInt32(Offset, uTemp32);
        }

        public void OR_UInt64(uint Offset, ulong input)
        {
            uTemp64 = ReadUInt64(Offset);
            uTemp64 |= input;
            WriteUInt64(Offset, uTemp64);
        }

        public bool ReadBool(uint Offset)
        {
            myBuff = GetMemory(Offset, 1);
            return (myBuff[0] != 0);
        }

        public byte ReadByte(uint Offset)
        {
            myBuff = GetMemory(Offset, 1);
            return myBuff[0];
        }

        public float ReadFloat(uint Offset)
        {
            myBuff = GetMemory(Offset, 4);
            Array.Reverse(myBuff, 0, 4);
            return BitConverter.ToSingle(myBuff, 0);
        }

        public short ReadInt16(uint Offset)
        {
            myBuff = GetMemory(Offset, 2);
            Array.Reverse(myBuff, 0, 2);
            return BitConverter.ToInt16(myBuff, 0);
        }

        public int ReadInt32(uint Offset)
        {
            myBuff = GetMemory(Offset, 4);
            Array.Reverse(myBuff, 0, 4);
            return BitConverter.ToInt32(myBuff, 0);
        }

        public long ReadInt64(uint Offset)
        {
            myBuff = GetMemory(Offset, 8);
            Array.Reverse(myBuff, 0, 8);
            return BitConverter.ToInt64(myBuff, 0);
        }

        public sbyte ReadSByte(uint Offset)
        {
            myBuff = GetMemory(Offset, 1);
            return (sbyte) myBuff[0];
        }

        public string ReadString(uint offset, uint length)
        {
            byte[] memory = GetMemory(offset, length);
            return new string(Encoding.ASCII.GetChars(memory)).Split(new char[1])[0];
        }

        public ushort ReadUInt16(uint Offset)
        {
            myBuff = GetMemory(Offset, 2);
            Array.Reverse(myBuff, 0, 2);
            return BitConverter.ToUInt16(myBuff, 0);
        }

        public uint ReadUInt32(uint Offset)
        {
            myBuff = GetMemory(Offset, 4);
            Array.Reverse(myBuff, 0, 4);
            return BitConverter.ToUInt32(myBuff, 0);
        }

        public ulong ReadUInt64(uint Offset)
        {
            myBuff = GetMemory(Offset, 8);
            Array.Reverse(myBuff, 0, 8);
            return BitConverter.ToUInt64(myBuff, 0);
        }

        public void RebootConsole()
        {
            if (activeConnection)
            {
                CallStruct(@"A\0\T\11\A\0\");
            }
        }

        private static string Recv()
        {
            JRPC_Reader.DiscardBufferedData();
            return JRPC_Reader.ReadLine();
        }

        private JRPCstruct ResetStruct()
        {
            return new JRPCstruct { Offset = 0, NumOfArgs = 0, Type = 0, ArgType = new uint[12], intArg = new uint[12], floatArg = new float[12], byteArg = new byte[12], LongArg = new ulong[12], ArraySize = new uint[12], intArray = new uint[12, 8], floatArray = new float[12, 8], byteArray = new byte[12, 8], stringArg = new char[12, 0x100] };
        }

        public uint ResolveFunction(string ModuleName, uint Ordinal)
        {
            if (!activeConnection)
            {
                return 0;
            }
            JRPCstruct j = ResetStruct();
            j.Type = SpawnEndian((uint) 9);
            j.ArgType[0] = SpawnEndian(String);
            j.stringArg = StringToChar(j, 0, ModuleName);
            j.ArgType[1] = SpawnEndian(Int);
            j.intArg[1] = SpawnEndian(Ordinal);
            string commands = string.Concat(new object[] { @"A\0\T\9\A\2\", String.ToString(), "/", ModuleName.Length, @"\", StringToByteString(ModuleName), @"\", Int.ToString(), @"\", Ordinal.ToString(), @"\" });
            CallStruct(commands);
            return uint.Parse(Recv(), NumberStyles.HexNumber);
        }

        private void Send(string Message, bool Check = false)
        {
            string str = "";
            if (Message.Length < 900)
            {
                Send2(Message + "\r\n");
            }
            else
            {
                string[] strArray = SplitByLength(Message, 800);
                for (int i = 0; i < strArray.Length; i++)
                {
                    Send2(strArray[i]);
                    str = str + strArray[i];
                }
                Send2("\r\n");
            }
            if (Check)
            {
                if (Recv() != Message)
                {
                    Send2("Bye\r\n");
                    throw new ArgumentException("Error: Arguments did not send to console correctly!");
                }
                Send2("Good\r\n");
            }
        }

        private void Send2(string Message)
        {
            JRPC_Writer.Write(Message);
            JRPC_Writer.Flush();
        }

        private void SendCMD(uint Offset, uint Type, params object[] Arguments)
        {
            string commands = string.Concat(new object[] { @"A\", Offset.ToString("X8"), @"\T\", Type, @"\A\", Arguments.Length, @"\" });
            int num = 0;
            foreach (object obj2 in Arguments)
            {
                bool flag = false;
                if (obj2 is uint)
                {
                    object obj3 = commands;
                    commands = string.Concat(new object[] { obj3, Int.ToString(), @"\", UIntToInt((uint) obj2), @"\" });
                    flag = true;
                }
                if (((obj2 is int) || (obj2 is bool)) || (obj2 is byte))
                {
                    if (obj2 is bool)
                    {
                        object obj4 = commands;
                        commands = string.Concat(new object[] { obj4, Int.ToString(), "/", Convert.ToInt32((bool) obj2), @"\" });
                    }
                    else
                    {
                        string str3 = commands;
                        commands = str3 + Int.ToString() + @"\" + ((obj2 is byte) ? Convert.ToByte(obj2).ToString() : Convert.ToInt32(obj2).ToString()) + @"\";
                    }
                    flag = true;
                }
                else if ((obj2 is int[]) || (obj2 is uint[]))
                {
                    byte[] buffer = IntArrayToByte((int[]) obj2);
                    object obj5 = commands;
                    commands = string.Concat(new object[] { obj5, ByteArray.ToString(), "/", buffer.Length, @"\" });
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        commands = commands + buffer[i].ToString("X2");
                    }
                    commands = commands + @"\";
                    flag = true;
                }
                else if (obj2 is string)
                {
                    string str2 = (string) obj2;
                    object obj6 = commands;
                    commands = string.Concat(new object[] { obj6, ByteArray.ToString(), "/", str2.Length, @"\", StringToByteString((string) obj2), @"\" });
                    flag = true;
                }
                else if (obj2 is float)
                {
                    float num3 = (float) obj2;
                    string str4 = commands;
                    commands = str4 + Float.ToString() + @"\" + num3.ToString() + @"\";
                    flag = true;
                }
                else if (obj2 is float[])
                {
                    float[] numArray = (float[]) obj2;
                    string str5 = commands;
                    string[] strArray3 = new string[] { str5, ByteArray.ToString(), "/", (numArray.Length * 4).ToString(), @"\" };
                    commands = string.Concat(strArray3);
                    for (int j = 0; j < numArray.Length; j++)
                    {
                        byte[] bytes = BitConverter.GetBytes(numArray[j]);
                        Array.Reverse(bytes);
                        for (int k = 0; k < 4; k++)
                        {
                            commands = commands + bytes[k].ToString("X2");
                        }
                    }
                    commands = commands + @"\";
                    flag = true;
                }
                else if (obj2 is byte[])
                {
                    byte[] buffer3 = (byte[]) obj2;
                    object obj7 = commands;
                    commands = string.Concat(new object[] { obj7, ByteArray.ToString(), "/", buffer3.Length, @"\" });
                    for (int m = 0; m < buffer3.Length; m++)
                    {
                        commands = commands + buffer3[m].ToString("X2");
                    }
                    commands = commands + @"\";
                    flag = true;
                }
                if (!flag)
                {
                    string str6 = commands;
                    commands = str6 + Uint64.ToString() + @"\" + Convert.ToInt64(obj2).ToString() + @"\";
                }
                num++;
            }
            CallStruct(commands);
        }

        public void SetLeds(LEDState Top_Left, LEDState Top_Right, LEDState Bottom_Left, LEDState Bottom_Right)
        {
            if (activeConnection)
            {
                CallStruct(string.Concat(new object[] { 
                    @"A\0\T\14\A\4\", Int, @"\", Top_Left, Int, @"\", Top_Right, Int, @"\", Bottom_Left, Int, @"\", Top_Left, Int, @"\", Bottom_Right, 
                    @"\"
                 }));
                Recv();
            }
        }

        public void SetMemory(uint Offset, byte[] Data)
        {
            uint num;
            xbConsole.DebugTarget.SetMemory(Offset, (uint) Data.Length, Data, out num);
        }

        private int SpawnEndian(int Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private float SpawnEndian(float Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        private uint SpawnEndian(uint Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private ulong SpawnEndian(ulong Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static string[] SplitByLength(string String, int maxLength)
        {
            int num = (String.Length / maxLength) + 1;
            string[] strArray = new string[num];
            if ((maxLength - String.Length) > 0)
            {
                strArray[0] = String;
                return strArray;
            }
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < maxLength; j++)
                {
                    string[] strArray2;
                    IntPtr ptr;
                    if (num2 >= String.Length)
                    {
                        break;
                    }
                    (strArray2 = strArray)[(int) (ptr = (IntPtr) i)] = strArray2[(int) ptr] + String[num2];
                    num2++;
                }
            }
            return strArray;
        }

        private string StringToByteString(string String)
        {
            string str = "";
            for (int i = 0; i < String.Length; i++)
            {
                str = str + ((byte) String[i]).ToString("X2");
            }
            return str;
        }

        private char[,] StringToChar(JRPCstruct J, int type, string String)
        {
            char[,] stringArg = J.stringArg;
            for (int i = 0; i < String.Length; i++)
            {
                stringArg[type, i] = String.ToCharArray()[i];
            }
            stringArg[type, String.Length] = '\0';
            return stringArg;
        }

        private byte[] StructToByte(JRPCstruct str)
        {
            int cb = Marshal.SizeOf(str);
            byte[] destination = new byte[cb];
            IntPtr ptr = Marshal.AllocHGlobal(cb);
            Marshal.StructureToPtr(str, ptr, false);
            Marshal.Copy(ptr, destination, 0, cb);
            Marshal.FreeHGlobal(ptr);
            return destination;
        }

        private string StructToString(JRPCstruct JRPC)
        {
            byte[] buffer = StructToByte(JRPC);
            string str = "";
            string oldValue = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    if (i >= (buffer.Length - 1))
                    {
                        oldValue = oldValue + "0;";
                    }
                    else
                    {
                        oldValue = oldValue + "0,";
                    }
                }
                else
                {
                    oldValue = "";
                }
                if (i >= (buffer.Length - 1))
                {
                    str = str + buffer[i] + ";";
                }
                else
                {
                    str = str + buffer[i] + ",";
                }
            }
            return str.Replace(oldValue, "NULL;");
        }

        private int UIntToInt(uint Value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
        }

        public void WriteBool(uint Offset, bool input)
        {
            myBuff[0] = input ? ((byte) 1) : ((byte) 0);
            SetMemory(Offset, myBuff);
        }

        public void WriteByte(uint Offset, byte input)
        {
            myBuff[0] = input;
            SetMemory(Offset, myBuff);
        }

        public void WriteFloat(uint Offset, float input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 4);
            SetMemory(Offset, myBuff);
        }

        public void WriteInt16(uint Offset, short input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 2);
            SetMemory(Offset, myBuff);
        }

        public void WriteInt32(uint Offset, int input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 4);
            SetMemory(Offset, myBuff);
        }

        public void WriteInt64(uint Offset, long input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 8);
            SetMemory(Offset, myBuff);
        }

        public void WriteSByte(uint Offset, sbyte input)
        {
            myBuff[0] = (byte) input;
            SetMemory(Offset, myBuff);
        }

        public void WriteString(uint offset, string String)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(String + "\0");
            SetMemory(offset, bytes);
        }

        public void WriteUInt16(uint Offset, ushort input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 2);
            SetMemory(Offset, myBuff);
        }

        public void WriteUInt32(uint Offset, uint input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 4);
            SetMemory(Offset, myBuff);
        }

        public void WriteUInt64(uint Offset, ulong input)
        {
            BitConverter.GetBytes(input).CopyTo(myBuff, 0);
            Array.Reverse(myBuff, 0, 8);
            SetMemory(Offset, myBuff);
        }

        public uint XamGetCurrentTitleId()
        {
            if (!activeConnection)
            {
                return 0;
            }
            CallStruct(@"A\0\T\16\A\0\");
            return uint.Parse(Recv(), NumberStyles.HexNumber);
        }

        public static string XboxIP()
        {
            if (!xbdmConnection)
            {
                return "10.0.0.0";
            }
            byte[] array = new byte[4];
            BitConverter.GetBytes(xbConsole.IPAddress).CopyTo(array, 0);
            Array.Reverse(array);
            return new IPAddress(array).ToString();
        }

        public void XNotify(string Text)
        {
            CallStruct(string.Concat(new object[] { @"A\0\T\12\A\1\", String, "/", Text.Length, @"\", StringToByteString(Text), @"\" }));
            Recv();
        }

        public void XOR_Int16(uint Offset, short input)
        {
            Temp16 = ReadInt16(Offset);
            Temp16 = (short) (Temp16 ^ input);
            WriteInt16(Offset, Temp16);
        }

        public void XOR_Int32(uint Offset, int input)
        {
            Temp32 = ReadInt32(Offset);
            Temp32 ^= input;
            WriteInt32(Offset, Temp32);
        }

        public void XOR_Int64(uint Offset, long input)
        {
            Temp64 = ReadInt64(Offset);
            Temp64 ^= input;
            WriteInt64(Offset, Temp64);
        }

        public void XOR_Uint16(uint Offset, ushort input)
        {
            uTemp16 = ReadUInt16(Offset);
            uTemp16 = (ushort) (uTemp16 ^ input);
            WriteUInt16(Offset, input);
        }

        public void XOR_UInt32(uint Offset, uint input)
        {
            uTemp32 = ReadUInt32(Offset);
            uTemp32 ^= input;
            WriteUInt32(Offset, uTemp32);
        }

        public void XOR_UInt64(uint Offset, ulong input)
        {
            uTemp64 = ReadUInt64(Offset);
            uTemp64 ^= input;
            WriteUInt64(Offset, uTemp64);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JRPCstruct
        {
            public uint Offset;
            public uint NumOfArgs;
            public uint Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public uint[] ArgType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public uint[] intArg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public float[] floatArg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public byte[] byteArg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public ulong[] LongArg;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
            public uint[] ArraySize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x60)]
            public uint[,] intArray;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x60)]
            public float[,] floatArray;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x60)]
            public byte[,] byteArray;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0xc00)]
            public char[,] stringArg;
        }

        public enum LEDState
        {
            GREEN = 0x80,
            OFF = 0,
            ORANGE = 0x88,
            RED = 8
        }

        public enum Temperature
        {
            CPU,
            GPU,
            EDRAM,
            MotherBoard
        }
    }
}

