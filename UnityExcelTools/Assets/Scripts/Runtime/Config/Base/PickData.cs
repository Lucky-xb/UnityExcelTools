using System.Collections.Generic;
using System;
using System.Text;

public class PickData
{
    /// <summary>
    /// 读取字节数据
    /// </summary>
    /// <param name="datas">数据</param>
    /// <param name="index">当前读取到第几个索引</param>
    public virtual void ReadData(byte[] datas, ref int index, ref int uid) { }

    public static T[] GetObjs<T>(byte[] datas) where T : PickData, new()
    {
        int index = 0;
        int uid = 0;
        int count = PickData.ReadInt(datas, ref index);
        T[] results = new T[count];
        for (int i = 0; i < count; i++)
        {
            results[i] = new T();
            results[i].ReadData(datas, ref index, ref uid);
        }
        return results;
    }

    public static byte[] WriteInt(int value)
    {
        byte[] data = System.BitConverter.GetBytes(value);
        return data;
    }

    public static byte[] WriteFloat(float value)
    {
        byte[] data = System.BitConverter.GetBytes(value);
        return data;
    }

    public static byte[] WriteString(string value)
    {
        byte[] data = Encoding.UTF8.GetBytes(value);
        int length = data.Length;
        List<byte> byteList = new List<byte>();
        byteList.AddRange(WriteInt(length));
        byteList.AddRange(data);
        return byteList.ToArray();
    }

    public static byte[] WriteIntArray(int[] values)
    {
        List<byte> byteList = new List<byte>();
        int count = values.Length;
        byteList.AddRange(WriteInt(count));
        for (int i = 0; i < count; i++)
        {
            byteList.AddRange(WriteInt(values[i]));
        }
        return byteList.ToArray();
    }

    public static byte[] WriteFloatArray(float[] values)
    {
        List<byte> byteList = new List<byte>();
        int count = values.Length;
        byteList.AddRange(WriteInt(count));
        for (int i = 0; i < count; i++)
        {
            byteList.AddRange(WriteFloat(values[i]));
        }
        return byteList.ToArray();
    }

    public static byte[] WriteStringArray(string[] values)
    {
        List<byte> byteList = new List<byte>();
        int count = values.Length;
        byteList.AddRange(WriteInt(count));
        for (int i = 0; i < count; i++)
        {
            byteList.AddRange(WriteString(values[i]));
        }
        return byteList.ToArray();
    }

    public static int ReadInt(byte[] data, ref int index)
    {
        byte[] read = new byte[sizeof(int)];
        Array.Copy(data, index, read, 0, read.Length);
        index += read.Length;
        return BitConverter.ToInt32(read, 0);
    }

    public static float ReadFloat(byte[] data, ref int index)
    {
        byte[] read = new byte[sizeof(float)];
        Array.Copy(data, index, read, 0, read.Length);
        index += read.Length;
        return BitConverter.ToSingle(read, 0);
    }

    public static string ReadString(byte[] data, ref int index)
    {
        int length = ReadInt(data, ref index);
        if (length <= 0)
            return "";
        byte[] read = new byte[length];
        Array.Copy(data, index, read, 0, length);
        index += length;
        return Encoding.UTF8.GetString(read);

    }

    public static int[] ReadIntArray(byte[] data, ref int index)
    {
        int count = ReadInt(data, ref index);
        int[] array = new int[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = ReadInt(data, ref index);
        }
        return array;
    }

    public static float[] ReadFloatArray(byte[] data, ref int index)
    {
        int count = ReadInt(data, ref index);
        float[] array = new float[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = ReadFloat(data, ref index);
        }
        return array;
    }

    public static string[] ReadStringArray(byte[] data, ref int index)
    {
        int count = ReadInt(data, ref index);
        string[] array = new string[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = ReadString(data, ref index);
        }
        return array;
    }
}