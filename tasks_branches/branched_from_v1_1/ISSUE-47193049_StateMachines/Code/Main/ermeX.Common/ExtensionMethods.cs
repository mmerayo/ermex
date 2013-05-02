// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ermeX.Common
{
    public static class ExtensionMethods
    {
        public static byte[] ReadBytes(this FileInfo file)
        {
            byte[] buffer;
            var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            try
            {
                var length = (int) fileStream.Length;
                buffer = new byte[length];
                int count;
                int sum = 0;

                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }


        public static byte[] ReadBytes(this NetworkStream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                var readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;
                do
                {
                    if ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                    {
                        totalBytesRead += bytesRead;

                        if (totalBytesRead == readBuffer.Length)
                        {
                            int nextByte = stream.ReadByte();
                            if (nextByte != -1)
                            {
                                var temp = new byte[readBuffer.Length*2];
                                Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                                Buffer.SetByte(temp, totalBytesRead, (byte) nextByte);
                                readBuffer = temp;
                                totalBytesRead++;
                            }
                        }
                    }
                } while (stream.DataAvailable);

                if(totalBytesRead<readBuffer.Length)
                    Array.Resize(ref readBuffer,totalBytesRead);

                return readBuffer;
                //byte[] buffer = readBuffer;
                //if (readBuffer.Length != totalBytesRead)
                //{
                //    buffer = new byte[totalBytesRead];
                //    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                //}
                //return buffer;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception reading the data from the socket", ex); //TODO: REMOVE THIS
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public static T[] SubArray<T>(this T[] data, long index, long length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, long index)
        {
            return data.SubArray(index, data.Length - index);
        }

        public static string ToStringFromByteArray(this byte[] source)
        {
            var enc = new UTF8Encoding();
            string result = enc.GetString(source);
            return result;
        }

        public static byte[] ToByteArray(this string str)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static bool IsEmpty(this Guid source)
        {
            return source == Guid.Empty;
        }
    }
}