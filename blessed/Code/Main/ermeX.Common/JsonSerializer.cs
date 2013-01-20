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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ermeX.Common
{
    public static class JsonSerializer
    {
        private static JsonSerializerSettings _jsonSerializerSettings;

        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_jsonSerializerSettings == null)
                {
                    var contractResolver = new SerializerCustomContractResolver();

                    _jsonSerializerSettings = new JsonSerializerSettings
                                                  {
                                                      ContractResolver = contractResolver,
                                                      TypeNameHandling = TypeNameHandling.All,
                                                      TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                                                      ConstructorHandling =
                                                          ConstructorHandling.AllowNonPublicDefaultConstructor,
                                                      ObjectCreationHandling = ObjectCreationHandling.Auto
                                                  };
                }
                return _jsonSerializerSettings;
            }
        }

        public static TResult DeserializeObjectFromJson<TResult>(string json)
        {
            return (TResult)DeserializeObjectFromJson(json, typeof(TResult));
        }

        public static object DeserializeObjectFromJson(string json, Type type)
        {
            try
            {
                if (type == null) throw new ArgumentNullException("type");
                object res = JsonConvert.DeserializeObject(json, type, SerializerSettings);

                return res;
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException(String.Format("The received JSON: {0}", json), ex); //TODO: REMOVE THIS
            }
        }


        public static string SerializeObjectToJson<TObject>(TObject source)
        {
            if (source == null)
                return null;

#if DEBUG
            const Formatting formatting = Formatting.Indented;
#else
            const Formatting formatting = Formatting.None;
#endif
            string json = JsonConvert.SerializeObject(source, formatting, SerializerSettings);

//            Debug.WriteLine(String.Format("SerializeObjectToJson: {0}{1}", Environment.NewLine, json));
            return json;
        }

        #region Nested type: SerializerCustomContractResolver

        private class SerializerCustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable)
                {
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }
        }

        #endregion
    }
}