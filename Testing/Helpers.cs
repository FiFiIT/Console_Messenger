using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using fbchat_sharp.API;
using System.Linq;

namespace Testing
{
    class Helpers
    {
        public static string CookiesFile = "cookies.xml";
        private static List<FB_User> users;
            
        //    new List<FB_User>()
        //{
        //    new FB_User() { nick = "Adrian", uid = "unknow", author = "1833362494" },
        //    new FB_User() { nick = "Tobiasz", uid = "unknow", author = "100000839220751" },
        //    new FB_User() { nick = "Bialy", uid = "unknow", author = "625353328" },
        //    new FB_User() { nick = "Kitek", uid = "unknow", author = "1661885777" },
        //    new FB_User() { nick = "Me", uid = "unknow", author = "100002909168135" },
        //    new FB_User() { nick = "Tomek", uid = "unknow", author = "100000033516982" },
        //};

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
        public static void PopulateUsers(MessengerClient client)
        {
            var fbUsers = client.fetchAllUsers();
            fbUsers.Wait();
            if(fbUsers.Result.Any())
            {
                users = fbUsers.Result;
            }

            var self = client.FetchProfile();
            self.Wait();
            users.Add(self.Result);
        }
        public static string GetUser(string author)
        {
            FB_User result = users.FirstOrDefault(user => user.uid.Equals(author));

            if(result == null)
            {
                return author;
            }
            else
            {
                return result.first_name == null ? result.last_name : result.first_name;
            }
        }
    }
}
