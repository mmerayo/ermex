// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
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