namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;
    using UnityEngine;

    public class ArrayE : MonoBehaviour
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        struct UIntFloat
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public System.Single FloatValue;
            [System.Runtime.InteropServices.FieldOffset(0)] public System.Int32 IntValue;
        }

        public static void WriteBuffer_Byte(NativeArray<byte> buffer, ref int index, byte value)
        {
            buffer[index++] = value;
        }

        public static byte ReadBuffer_Byte(NativeArray<byte> buffer, ref int index)
        {
            return buffer[index++];
        }

        public static void WriteBuffer_Int32(NativeArray<byte> buffer, ref int index, int value)
        {
            buffer[index++] = (byte)(value >> 00);
            buffer[index++] = (byte)(value >> 08);
            buffer[index++] = (byte)(value >> 16);
            buffer[index++] = (byte)(value >> 24);
        }

        public static int ReadBuffer_Int32(NativeArray<byte> buffer, ref int index)
        {
            int byte0 = (int)buffer[index++] << 0;
            int byte1 = (int)buffer[index++] << 8;
            int byte2 = (int)buffer[index++] << 16;
            int byte3 = (int)buffer[index++] << 24;

            int result = byte0 | byte1 | byte2 | byte3;
            return result;
        }

        public static void WriteBuffer_Float(NativeArray<byte> buffer, ref int index, float x)
        {
            var val = new UIntFloat();
            val.FloatValue = x;
            WriteBuffer_Int32(buffer, ref index, val.IntValue);
        }

        public static float ReadBuffer_Float(NativeArray<byte> buffer, ref int index)
        {
            var uf = new UIntFloat();
            uf.IntValue = ReadBuffer_Int32(buffer, ref index); 
            return uf.FloatValue;
        }

        /// <summary>
        /// the end result of compressing [ data ] should be [ length, compressed data.. ]
        /// so compressed data would look like [ length, value, count, value, count, etc ... ]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="temp"></param>
        /// <param name="start_index"></param>
        /// <param name="end_index"></param>
        /// <param name="index"></param>
        public static void CompressWrittenBuffer_Floats(NativeArray<byte> buffer, NativeArray<byte> compressed, int start_index, int end_index, ref int index)
        {
            // 
            // the end result of compressing [ data ] should be [ length, compressed data.. ]
            // so compressed data would look like [ length, value, count, value, count, etc ... ]
            // 

            int working_index = -1;
            float working_value = 0;
            int working_count = 0;

            // write out a 0 for the length, to reserve the space, keep note of the index 
            int compressed_length_index = index;
            WriteBuffer_Int32(compressed, ref index, 0);

            var tuple_count = 0;

            for (var i = start_index; i < end_index;)
            {
                float value = ReadBuffer_Float(buffer, ref i);

                if (working_index == -1)
                {
                    working_index = i;
                    working_value = value;
                    working_count = 1;
                    continue;
                }

                // we'll only compress in chunks of size MaxValue, to avoid an overflow in our byte storage 
                if (value != working_value || (i - working_index) > float.MaxValue - 1)
                {
                    // store 
                    WriteBuffer_Int32(compressed, ref index, working_count);
                    WriteBuffer_Float(compressed, ref index, working_value);

                    // keep going 
                    working_index = i;
                    working_value = value;
                    working_count = 1;

                    tuple_count++;
                }
                else
                {
                    working_count++;
                }
            }

            // then again for the final value 
            if (working_index != -1)
            {
                // store 
                WriteBuffer_Int32(compressed, ref index, working_count);
                WriteBuffer_Float(compressed, ref index, working_value);
                working_count = 0;

                tuple_count++;
            }

            // jump backwards and write out what the length ended up being 
            var compressed_length = (index - compressed_length_index);
            WriteBuffer_Int32(compressed, ref compressed_length_index, tuple_count);
        }

        /// <summary>
        /// Decompresses buffer into temp.
        /// The format looks like this: 
        /// [ length, value, count, value, count, value, count, etc.. ]
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="temp"></param>
        /// <param name="compressedIndex"></param>
        /// <param name="decompresedIndex"></param>
        public static void DecompressReadBuffer_float(NativeArray<byte> buffer, NativeArray<byte> decompressed, ref int compressedIndex, ref int decompresedIndex)
        {
            var tuple_count = ReadBuffer_Int32(buffer, ref compressedIndex);

            for (var i = 0; i < tuple_count; ++i)
            {
                
                int store_count = ReadBuffer_Int32(buffer, ref compressedIndex);
                float store_value = ReadBuffer_Float(buffer, ref compressedIndex);
                for (var s = 0; s < store_count; ++s)
                {
                    WriteBuffer_Float(decompressed, ref decompresedIndex, store_value);
                }
            }
        }
    }
}