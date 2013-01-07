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
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ermeX.Common
{
    //TODO: INJECTABLE
    public static class ObjectSerializer
    {
        public static void SerializeObject<TSerialize>(string fileName, TSerialize source)
        {
            if (source == null) throw new ArgumentNullException("source");
            using (var fs = new FileStream(fileName, FileMode.CreateNew))
            {
                Serializer.Serialize(fs, source);
                fs.Close();
            }
        }

        public static TResult DeserializeObject<TResult>(string fileName)
        {
            TResult result;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                result = Serializer.Deserialize<TResult>(fs);
                fs.Close();
            }
            return result;
        }

        public static void SerializeObjectWithoutOptimization<TSerialize>(string fileName, TSerialize source)
        {
            if (source == null) throw new ArgumentNullException("source");
            using (var fs = new StreamWriter(fileName))
            {
                string json = JsonSerializer.SerializeObjectToJson(source);
                fs.Write(json);
                fs.Close();
            }
        }

        public static TResult DeserializeObjectWithoutOptimization<TResult>(string fileName)
        {
            TResult result;

            using (var fs = new StreamReader(fileName))
            {
                string json = fs.ReadToEnd();
                result = JsonSerializer.DeserializeObjectFromJson<TResult>(json);
                fs.Close();
            }
            return result;
        }


        //TODO: ISSUE-16 REMOVE THE FOLLOWINg 2 METHODS
        public static byte[] SerializeObjectToByteArray<TSerializable>(TSerializable source) where TSerializable : class
        {
            byte[] result;
            using (var memoryStream = SerializeObjectToStream(source))
            {
                result = memoryStream.ToArray();
            }
            return result;
        }


        public static TResult DeserializeObject<TResult>(byte[] source)
        {
            TResult result;
            using (var memoryStream = new MemoryStream(source))
            {
                memoryStream.Position = 0;
                result = Serializer.Deserialize<TResult>(memoryStream);
            }

            return result;
        }

        public static MemoryStream SerializeObjectToStream<TSerializable>(TSerializable source) where TSerializable : class
        {
            var memoryStream = new MemoryStream();

            Serializer.Serialize(memoryStream, source);
            memoryStream.Position = 0;
            return memoryStream;
        }


        public static TResult DeserializeObject<TResult>(MemoryStream sourceStream)
        {
            TResult result;
            result = DeserializeObject<TResult>(sourceStream.ToArray());
            return result;
        }


        private static readonly object SyncLock = new object();

        public static bool AppendTypeToSerializator(Type containerType, Type typeToAppend)
        {
            bool result = false;
            MetaType metaType = RuntimeTypeModel.Default[containerType];

            if (metaType.GetSubtypes() == null || metaType.GetSubtypes().Length==0 )
            {
                lock (SyncLock)
                    if (metaType.GetSubtypes() == null || metaType.GetSubtypes().Length == 0)
                    {
                        metaType.AddSubType(100, typeToAppend);
                        result = true;
                    }
            }
            else if (!metaType.GetSubtypes().Any(x => x.DerivedType.Type == typeToAppend))
            {
                lock (SyncLock)
                    if (!metaType.GetSubtypes().Any(x => x.DerivedType.Type == typeToAppend))
                    {
                        int max = metaType.GetSubtypes().Max(x => x.FieldNumber);
                        metaType.AddSubType(max + 100, typeToAppend);
                        result = true;
                    }
            }

            return result;
        }
    }
}