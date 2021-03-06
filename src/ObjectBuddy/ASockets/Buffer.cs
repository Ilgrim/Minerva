﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.IO;

namespace AwesomeSockets
{
    public class Buffer
    {
        private const int DEFAULT_BUFFER_SIZE = 1024;
        private readonly int bufferSize;
        private readonly byte[] bytes;
        private int position;
        private bool finalized;

        private Buffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            bytes = new byte[bufferSize];
            position = 0;
            finalized = false;
        }

        private Buffer(int bufferSize, byte[] bytes, int position, bool finalized)
        {
            this.bufferSize = bufferSize;
            this.bytes = bytes;
            this.position = position;
            this.finalized = finalized;
        }

        #region public operation methods
        public static Buffer New()
        {
            return New(DEFAULT_BUFFER_SIZE);
        }

        public static Buffer New(int bufferSize)
        {
            return new Buffer(bufferSize);
        }


        public static void ClearBuffer(Buffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            buffer.ClearBuffer();
        }

        public static void FinalizeBuffer(Buffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            buffer.FinalizeBuffer();
        }

        public static void Add(Buffer buffer, Buffer bufferToWrite)
        {
            if (buffer == null || bufferToWrite == null) throw new ArgumentNullException("buffer");
            buffer.ClearBuffer();
            buffer = New();
            buffer.Add(bufferToWrite.bytes);
        }

        /*public static void Add(Buffer buffer, byte[] byteArray)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (byteArray.Length == 0) throw new DataException("Cannot provide a zero-length array");
            buffer.ClearBuffer();
            buffer.Add(byteArray);
            buffer.FinalizeBuffer();
        }*/

        public static void Add(Buffer buffer, object value)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.finalized) throw new BufferFinalizedException("Buffer provided is in 'finalized' state. You must call 'ClearBuffer()' to reset it.");
            buffer.Add(value);
        }

        public static T Get<T>(Buffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (typeof(T) == typeof(bool)) return (T) (object) buffer.GetBoolean();
            if (typeof(T) == typeof(byte)) return (T) (object) buffer.GetByte();
            if (typeof(T) == typeof(sbyte)) return (T) (object) buffer.GetSByte();
            if (typeof(T) == typeof(char)) return (T) (object) buffer.GetChar();
            if (typeof(T) == typeof(double)) return (T) (object) buffer.GetDouble();
            if (typeof(T) == typeof(float)) return (T) (object) buffer.GetFloat();
            if (typeof(T) == typeof(int)) return (T) (object) buffer.GetInt();
            if (typeof(T) == typeof(uint)) return (T) (object) buffer.GetUInt();
            if (typeof(T) == typeof(long)) return (T) (object) buffer.GetLong();
            if (typeof(T) == typeof(ulong)) return (T) (object) buffer.GetULong();
            if (typeof(T) == typeof(short)) return (T) (object) buffer.GetShort();
            if (typeof(T) == typeof(ushort)) return (T) (object) buffer.GetUShort();
            if (typeof(T) == typeof(string)) return (T) (object) buffer.GetString();
            throw new DataException(string.Format("Provided type ({0}) cannot be deserialized from buffer. You must provide a (except struct and enum) or a string", typeof(T)));
        }

        public static byte[] GetBytes(Buffer buffer, int length)
        {
            return buffer.GetBytes(length);
        }

        public static byte[] GetBuffer(Buffer buffer)
        {
           if (buffer == null) throw new ArgumentNullException("buffer");
           if (!buffer.finalized) throw new BufferFinalizedException("Buffer provided is not in 'finalized' state. You must call 'FinalizeBuffer()' in order to get the full buffer");
           return buffer.GetBuffer();
        }

        public static byte[] GetBufferRef(Buffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            buffer.ClearBuffer();
            return buffer.GetBuffer();
        }

        //public static void EncryptBuffer(Buffer buffer, string encryptionKey, string initVector, int keySize = (int)KeySizes.TwoFiftySix, int ivSize = (int)KeySizes.OneTwenyEight)
        //{
        //    var plaintextBytes = buffer.bytes;
        //    var rijndaelEncryptor = buffer.CreateRijndael(encryptionKey, initVector, keySize, ivSize).CreateEncryptor();

        //    MemoryStream mStream = new MemoryStream();
        //    CryptoStream cStream = new CryptoStream(mStream, rijndaelEncryptor, CryptoStreamMode.Write);

        //    cStream.Write(plaintextBytes, 0, plaintextBytes.Length);
        //    cStream.FlushFinalBlock();

        //    //buffer.SaveNullPosition();
        //    Add(buffer, mStream.ToArray());

        //    mStream.Close();
        //    cStream.Close();
        //}

        //public static void DecryptBuffer(Buffer buffer, string decryptionKey, string initVector, int keySize = (int)KeySizes.TwoFiftySix, int ivSize = (int)KeySizes.OneTwenyEight)
        //{
        //    var cipherBytes = buffer.bytes;
        //    var rijndaelDecryptor = buffer.CreateRijndael(decryptionKey, initVector, keySize, ivSize).CreateDecryptor();

        //    MemoryStream mStream = new MemoryStream(cipherBytes);
        //    CryptoStream cStream = new CryptoStream(mStream, rijndaelDecryptor, CryptoStreamMode.Read);

        //    var plaintextBytes = new byte[cipherBytes.Length];

        //    cStream.Read(plaintextBytes, 0, cipherBytes.Length);

        //    Add(buffer, plaintextBytes);
        //    //buffer.ApplyNullPosition();

        //    mStream.Close();
        //    cStream.Close();
        //}

        public static Buffer Duplicate(Buffer bufferToDup)
        {
            return new Buffer(bufferToDup.bufferSize, bufferToDup.bytes, bufferToDup.position, 
                              bufferToDup.finalized);
        }

        //Here for converting doubles and floats...
        public static void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count)
        {
            System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
        }

        public override bool Equals(object obj)
        {
            Buffer other = (Buffer) obj;
            return ((bufferSize == other.bufferSize) && 
                    (bytes.Equals(other.bytes)) && 
                    (position == other.position) && 
                    (finalized == other.finalized));
        }

        public override int GetHashCode()
        {
            return bytes.Aggregate((x, y) => Convert.ToByte(x ^ y ^ bufferSize ^ position ^ (finalized ? 1 : 0)));
        }

        public int GetSize()
        {
            return bytes.Length;
        }
        #endregion

        #region private operation methods
        private byte[] GetBuffer()
        { 
            var tempList = new List<byte?>();
            for (var i = 0; i < bufferSize; i++)
            {
                tempList.Add(bytes[i]);
            }

            return bytes;
        }

        private void ClearBuffer()
        {
            for (var i = 0; i < bufferSize; i ++)
            {
                bytes[i] = 0;
            }
            position = 0;
            finalized = false;
        }

        private void FinalizeBuffer()
        {
            finalized = true;
            position = 0;
        }

        private void Add(byte[] byteArray)
        {
            System.Buffer.BlockCopy(byteArray, 0, bytes, position, byteArray.Length);
            position += byteArray.Length;
        }

        private void Add(object primitive)
        {
            byte[] array;

            if (primitive == null) throw new ArgumentNullException("primitive");
            if (primitive is byte[])
                array = (byte[])primitive;
            else
                array = ConvertToByteArray(primitive);

            //TODO: Need to increase buffer size in this case, rather than throwing an exception
            if (!CheckBufferBoundaries(array)) throw new ConstraintException("Failed to add primitive to buffer. There is no additional room for it.");
            foreach (var b in array)
            {
                bytes[position] = b;
                position += 1;
            }
        }

        private static byte[] ConvertToByteArray(object primitive)
        {
            if (primitive is bool) return BitConverter.GetBytes((bool) primitive);
            if (primitive is byte) return BitConverter.GetBytes((byte)primitive);
            if (primitive is sbyte) return BitConverter.GetBytes((sbyte)primitive);
            if (primitive is char) return BitConverter.GetBytes((char)primitive);
            if (primitive is double) return BitConverter.GetBytes((double)primitive);
            if (primitive is float) return BitConverter.GetBytes((float)primitive);
            if (primitive is int) return BitConverter.GetBytes((int)primitive);
            if (primitive is uint) return BitConverter.GetBytes((uint)primitive);
            if (primitive is long) return BitConverter.GetBytes((long)primitive);
            if (primitive is ulong) return BitConverter.GetBytes((ulong)primitive);
            if (primitive is short) return BitConverter.GetBytes((short)primitive);
            if (primitive is ushort) return BitConverter.GetBytes((ushort)primitive);
            if (primitive is string)
            {
                var str = primitive as string;
                if (str.Contains("\0")) throw new DataException("String cannot contain null character '\\0'");
                var termStr = string.Format("{0}{1}", primitive as string, "\0");   //The '\0' is to null-terminate the string so we can deserialize it on the other end as strings aren't fixed in size
                byte[] bytes = new byte[termStr.Length * sizeof(char)];
                System.Buffer.BlockCopy(termStr.ToCharArray(), 0, bytes, 0, bytes.Length);
                return bytes;
            }
            throw new DataException("Provided type cannot be serialized for transmission. You must provide a value type (except enum and struct) or a string");
        }
        #endregion

        #region private getters
        private byte[] GetBytes(int length)
        {
            if (!CheckBufferBoundaries(length)) throw new ConstraintException("Failed to get byte[], reached end of buffer.");
            var tmp = new byte[length];
            Array.Copy(bytes, position, tmp, 0, length);
            position += length;
            return tmp;
        }

        private bool GetBoolean()
        {
            if (!CheckBufferBoundaries(sizeof(bool))) throw new ConstraintException("Failed to get bool, reached end of buffer.");
            var value = BitConverter.ToBoolean(bytes, position);
            position += sizeof(bool); 
            return value;
        }

        private byte GetByte()
        {
            if (!CheckBufferBoundaries(sizeof(byte))) throw new ConstraintException("Failed to get byte, reached end of buffer.");
            var value = bytes[position];
            position += sizeof(byte);
            position += 1;
            return value;
        }

        private sbyte GetSByte()
        {
            if (!CheckBufferBoundaries(sizeof(sbyte))) throw new ConstraintException("Failed to get sbyte, reached end of buffer.");
            var value = bytes[position];
            position += sizeof(sbyte);
            position += 1;
            return (sbyte)value;
        }

        private char GetChar()
        {
            if (!CheckBufferBoundaries(sizeof(char))) throw new ConstraintException("Failed to get char, reached end of buffer.");
            var value = BitConverter.ToChar(bytes, position);
            position += sizeof(char);
            return value;
        }

        private double GetDouble()
        {
            if (!CheckBufferBoundaries(sizeof(double))) throw new ConstraintException("Failed to get double, reached end of buffer.");
            var value = BitConverter.ToDouble(bytes, position);
            position += sizeof(double);
            return value;
        }

        private float GetFloat()
        {
            if (!CheckBufferBoundaries(sizeof(float))) throw new ConstraintException("Failed to get float, reached end of buffer.");
            var value = BitConverter.ToSingle(bytes, position);
            position += sizeof(float);
            return value;
        }

        private int GetInt()
        {
            if (!CheckBufferBoundaries(sizeof(int))) throw new ConstraintException("Failed to get int, reached end of buffer.");
            var value = BitConverter.ToInt32(bytes, position);
            position += sizeof(int);
            return value;
        }

        private uint GetUInt()
        {
            if (!CheckBufferBoundaries(sizeof(uint))) throw new ConstraintException("Failed to get uint, reached end of buffer.");
            var value = BitConverter.ToUInt32(bytes, position);
            position += sizeof(uint);
            return value;
        }

        private long GetLong()
        {
            if (!CheckBufferBoundaries(sizeof(long))) throw new ConstraintException("Failed to get long, reached end of buffer.");
            var value = BitConverter.ToInt64(bytes, position);
            position += sizeof(long);
            return value;
        }

        private ulong GetULong()
        {
            if (!CheckBufferBoundaries(sizeof(ulong))) throw new ConstraintException("Failed to get ulong, reached end of buffer.");
            var value = BitConverter.ToUInt64(bytes, position);
            position += sizeof(ulong);
            return value;
        }

        private short GetShort()
        {
            if (!CheckBufferBoundaries(sizeof(short))) throw new ConstraintException("Failed to get short, reached end of buffer.");
            var value = BitConverter.ToInt16(bytes, position);
            position += sizeof(short);
            return value;
        }

        private ushort GetUShort()
        {
            if (!CheckBufferBoundaries(sizeof(ushort))) throw new ConstraintException("Failed to get short, reached end of buffer.");
            var value = BitConverter.ToUInt16(bytes, position);
            position += sizeof(ushort);
            return value;
        }

        private string GetString()
        {
            var localPosition = -1;
            var startPosition = position;
            for (var i = position; i <= bufferSize; i += sizeof(char))
            {
                char test = GetChar();
                if (test == '\0')
                {
                    localPosition = i;
                    break;
                }
            }
            position = startPosition;

            if (localPosition != -1)
            {
                var chars = new char[(localPosition - position) / sizeof(char)];
                System.Buffer.BlockCopy(bytes, position, chars, 0, localPosition - position);
                position = localPosition += sizeof(char);
                return new string(chars);
            }
            throw new ConstraintException("Failed to get string, reached end of buffer.");
        }
        #endregion

        #region private boundary checks
        private bool CheckBufferBoundaries(byte[] bytesToCheck)
        {
            var roomLeft = bytes.Length - position;
            return roomLeft >= bytesToCheck.Length;
        }

        private bool CheckBufferBoundaries(int numberOfBytes)
        {
            var roomLeft = bytes.Length - position;
            return roomLeft >= numberOfBytes;
        }
        #endregion

        #region private misc methods
        private static byte[] CreateCryptoKeyFromString(string stringToConvert, int keySize)
        {
            var initialConvert = ConvertToByteArray(stringToConvert);

            byte[] sanitizedConvert;

            if (initialConvert.Length > keySize)    //Truncate if greater than 256
                sanitizedConvert = initialConvert.Take(keySize).ToArray();
            else    //fill with repeats until 256
                RotateBytes(initialConvert, keySize, out sanitizedConvert);

            return sanitizedConvert;
        }

        private static void RotateBytes(byte[] source, int keySize, out byte[] destination)
        {
            var localPostion = 0;
            destination = new byte[keySize/8];
            do
            {
                foreach (var b in source)
                {
                    if (localPostion < destination.Length)
                        destination[localPostion] = b;
                    else
                        return;     //We have reached the end of the destination array and can short-circuit here
                    localPostion += 1;
                }
            } while (localPostion < destination.Length);
        }

        private RijndaelManaged CreateRijndael(string encryptKey, string initVector, int keySize, int vectorSize)
        {
            return new RijndaelManaged
            {
                KeySize = keySize,
                Key = CreateCryptoKeyFromString(encryptKey, keySize),
                IV = CreateCryptoKeyFromString(initVector, vectorSize),
                Padding = PaddingMode.None
            };
        }
        #endregion
    }
}
