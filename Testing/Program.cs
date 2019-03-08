using fbchat_sharp.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static FBClient client;
        static List<FB_Thread> Threads { get; set; }
        static string CurrentThreadID { get; set; }
        static StringBuilder builder;

        static void Main(string[] args)
        {
            StartMessanger();
        }

        private async static void StartMessanger()
        {
            bool running = true;
            bool firstRun = true;
            builder = new StringBuilder();

            LogToMessanger();

            Helpers.PopulateUsers(client);

            client.UpdateEvent += Client_UpdateEvent;
            client.StartListening();

            while (running)
            {
                string answer = "2";

                if (!firstRun)
                {
                    Console.WriteLine("What do you want to do next?");
                    Console.WriteLine("1. Log In");
                    Console.WriteLine("2. Get last Threads");
                    Console.WriteLine("9. Exit");

                    answer = Console.ReadLine();
                }

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
            if (msg != null)
            {
                string fromGroup = String.Empty;
                if (!msg.thread_id.Equals(CurrentThreadID))
                {
                    fromGroup = $"{Threads.FirstOrDefault(t => t.uid.Equals(msg.thread_id)).name} ";
                }
                ClearCurrentLine();
                Console.WriteLine($"\r{fromGroup}{PrintMSG(msg)}");
                Console.Write(builder.ToString());
            }
        }

        private static string PrintMSG(FB_Message msg)
        {
            string restult = string.Empty;

            double timestamp = double.Parse(msg.timestamp);
            var time = new System.DateTime(1970, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
                foreach (var thread in threads.Take(5))
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

                    ConsoleKeyInfo input;
                    string message = String.Empty;

                    bool chatting = true;
                    while (chatting)
                    {
                        input = Console.ReadKey(intercept: true);
                        while(input.Key != ConsoleKey.Enter)
                        {
                            if (input.Key == ConsoleKey.Tab)
                            {
                                HandleTabInput(builder, Threads);
                            }
                            else if(input.Key == ConsoleKey.Escape)
                            {
                                Console.WriteLine("Leaving chat");
                                chatting = false;
                                break;
                            }
                            else
                            {
                                HandleKeyInput(builder, input);
                            }

                            input = Console.ReadKey(intercept: true);
                        }

                        message = builder.ToString();
                        builder.Clear();
                        var response = client.SendMessage(message, thread_id: CurrentThreadID, thread_type: threads[uid].type);
                        response.Wait();
                    }
                }
            }
        }

        private static void HandleKeyInput(StringBuilder builder, ConsoleKeyInfo input)
        {
            var currentInput = builder.ToString();
            if (input.Key == ConsoleKey.Backspace && currentInput.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
                ClearCurrentLine();

                currentInput = currentInput.Remove(currentInput.Length - 1);
                Console.Write(currentInput);
            }
            else
            {
                var key = input.KeyChar;
                builder.Append(key);
                Console.Write(key);
            }
        }

        private static void HandleTabInput(StringBuilder builder, List<FB_Thread> data)
        {
            return;

            var currentInput = builder.ToString();
            var match = String.Empty;

            if (string.IsNullOrEmpty(match))
            {
                return;
            }

            ClearCurrentLine();
            builder.Clear();

            Console.Write(match);
            builder.Append(match);
        }

        private static void ClearCurrentLine()
        {
            var currentLine = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLine);
        }

        private static List<FB_Thread> GetLastThreads()
        {
            var threads = client.fetchThreadList();
            threads.Wait();

            return threads.Result;
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
