using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static FBClient client;
        static List<FB_Thread> Threads { get; set; }
        static string CurrentThreadID { get; set; }

        static void Main(string[] args)
        {
            StartMessanger();
        }

        private async static void StartMessanger()
        {
            bool running = true;

            LogToMessanger();

            client.UpdateEvent += Client_UpdateEvent;
            client.StartListening();

            while (running)
            {
                Console.WriteLine("What do you want to do next?");
                Console.WriteLine("1. Log In");
                Console.WriteLine("2. Get last Threads");
                Console.WriteLine("9. Exit");

                string answer = Console.ReadLine();

                switch (answer)
                {
                    case "1":
                        LogToMessanger();
                        break;
                    case "2":
                        Threads = GetLastThreads();
                        ReadMessages(Threads);
                        break;
                    case "9":
                        LogOut();
                        running = false;
                        break;
                    default:
                        Console.WriteLine("I don't understand you!");
                        break;
                }
            }
        }

        private static void Client_UpdateEvent(object sender, UpdateEventArgs e)
        {
            FB_Message msg = e.Payload as FB_Message;
            if(msg != null && !Helpers.GetUser(msg.author).Equals("Me"))
            {
                string fromGroup = String.Empty;
                if(!msg.thread_id.Equals(CurrentThreadID))
                {
                    fromGroup = $"{Threads.FirstOrDefault(t => t.uid.Equals(msg.thread_id)).name} ";
                }

                Console.WriteLine($"\r{fromGroup}{PrintMSG(msg)}");
            }
        }

        private static string PrintMSG(FB_Message msg)
        {
            string restult = string.Empty;

            double timestamp = double.Parse(msg.timestamp);
            var time = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var msgTime = time.AddMilliseconds(timestamp).ToShortTimeString();
            var from = Helpers.GetUser(msg.author);

            restult = $"{msgTime} {from}: {msg.text}";

            return restult;
        }

        private static void ReadMessages(List<FB_Thread> threads)
        {
            bool viewGorup = true;
            int groupId;

            while (viewGorup)
            {
                groupId = 0;
                foreach (var thread in threads.Take(3))
                {
                    Console.WriteLine($"{groupId}: {thread.name}");
                    groupId++;
                }
                Console.WriteLine("Which grop messages you want to view?");
                var id = Console.ReadLine();

                if (String.Equals(id, "q", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("exiting chat");
                    viewGorup = false;
                    break;
                }

                if (int.TryParse(id, out int uid))
                {
                    CurrentThreadID = threads[uid].uid;
                    var result = client.FetchThreadMessages(CurrentThreadID);
                    result.Wait();
                    List<FB_Message> messages = result.Result;
                    messages.Reverse();

                    foreach (FB_Message msg in messages)
                    {
                        Console.WriteLine(PrintMSG(msg));
                    }

                    bool chatting = true;
                    while (chatting)
                    {
                        string message = Console.ReadLine();

                        if (String.Equals(message, "q", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("exiting chat");
                            chatting = false;
                            break;
                        }

                        var response = client.SendMessage(message, thread_id: CurrentThreadID, thread_type: ThreadType.GROUP);
                        response.Wait();
                    }
                }
            }
        }
        private static List<FB_Thread> GetLastThreads()
        {
            var threads = client.fetchThreadList();
            threads.Wait();
            var result = threads.Result;

            

            return result;
        }

        private async static void LogOut()
        {
            await client.DoLogout();
        }

        private static bool LogToMessanger()
        {
            if (client == null)
            {
                client = new FBClient();
            }

            string email = "filip.tyborowski@gmail.com";
            string password = "fifiit82.";

            // Login with username and password
            Task<bool> logged_in = client.DoLogin(email, password);
            logged_in.Wait();

            if (logged_in.Result)
            {
                Console.WriteLine("Successfully loged into the messanger!");
            }
            {
                Console.WriteLine("ERROR: Couldn't connect to messanger");
            }

            return logged_in.Result;
        }

        private async static void InstantiateFBClient()
        {
            FBClient client = new FBClient();

            string email = "filip.tyborowski@gmail.com";
            string password = "wforemce82.";

            // Login with username and password
            var logged_in = await client.DoLogin(email, password);

            // Check login was successful
            if (logged_in)
            {
                List<FB_Thread> threads = await client.fetchThreadList();

                List<FB_Message> messages = await client.FetchThreadMessages(threads.First().uid);


                //Send a message to myself
                var msg_uid = await client.SendMessage("Hi me!", thread_id: client.GetUserUid());

                if (msg_uid != null)
                {
                    Console.WriteLine("Message sent: {0}", msg_uid);
                }

                // Do logout
                await client.DoLogout();
            }
            else
            {
                Console.WriteLine("Error logging in");
            }
        }

    }
}
