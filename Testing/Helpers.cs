using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace Testing
{
    class Helpers
    {
        private static List<User> users = new List<User>()
        {
            new User() { nick = "Adrian", uid = "unknow", author = "1833362494" },
            new User() { nick = "Tobiasz", uid = "unknow", author = "100000839220751" },
            new User() { nick = "Bialy", uid = "unknow", author = "625353328" },
            new User() { nick = "Kitek", uid = "unknow", author = "1661885777" },
            new User() { nick = "Me", uid = "unknow", author = "100002909168135" },
            new User() { nick = "Tomek", uid = "unknow", author = "100000033516982" },
        };

        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                FileStream writer = new FileStream(fileName, FileMode.Create);
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(writer, serializableObject);
                writer.Close();
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR while serializing object!");
                Console.WriteLine(ex.Message);
            }
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(T));

                objectOut = (T)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR while DEserializing object!");
                Console.WriteLine(ex.Message);
            }

            return objectOut;
        }

        public static string GetUser(string author)
        {
            User result = users.FirstOrDefault(user => user.author.Equals(author));

            if(result == null)
            {
                return author;
            }
            else
            {
                return result.nick;
            }
        }
    }
}
