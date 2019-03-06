﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace fbchat_sharp.API
{
    internal class Utils
    {
        // Default list of user agents
        public static readonly string[] USER_AGENTS = {
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_3) AppleWebKit/601.1.10 (KHTML, like Gecko) Version/8.0.5 Safari/601.1.10",
            "Mozilla/5.0 (Windows NT 6.3; WOW64; ; NCT50_AAP285C84A1328) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/22.0.1207.1 Safari/537.1",
            "Mozilla/5.0 (X11; CrOS i686 2268.111.0) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1092.0 Safari/536.6"
        };

        public static long now()
        {
            return (long)DateTimeOffset.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static string strip_to_json(string text)
        {
            try
            {
                return text.Substring(text.IndexOf("{"));
            }
            catch (Exception)
            {
                throw new FBchatException(string.Format("No JSON object found: {0}, {1}", text, text.IndexOf('{')));
            }
        }

        public static string get_decoded(byte[] content)
        {
            return Encoding.GetEncoding(ReqUrl.facebookEncoding).GetString(content, 0, content.Length);
        }

        public JToken parse_json(string content)
        {
            return JToken.Parse(content);
        }

        public JToken get_json(byte[] content)
        {
            return JToken.Parse(strip_to_json(get_decoded(content)));
        }

        public static string digitToChar(int digit)
        {
            if (digit < 10)
                return digit.ToString();
            return ((int)'a' + digit - 10).ToString();
        }

        public static string str_base(int number, int bs)
        {
            if (number < 0)
                return "-" + str_base(-number, bs);
            int d = number / bs;
            int m = number % bs;
            if (d > 0)
                return str_base(d, bs) + digitToChar(m);
            return digitToChar(m).ToString();
        }

        public static string generateMessageID(string client_id = null)
        {
            long k = now();
            long l = (long)(new Random().NextDouble() * 4294967295);
            return string.Format("<{0}:{1}-{2}@mail.projektitan.com>", k, l, client_id);
        }

        public static string getSignatureID()
        {
            return ((long)(new Random().NextDouble() * 2147483648)).ToString("X4");
        }

        public static string generateOfflineThreadingID()
        {
            long ret = now();
            long value = (long)(new Random().NextDouble() * 4294967295);
            string str = "0000000000000000000000" + Convert.ToString(value, 2);
            str = str.Substring(str.Length - 22);
            string msgs = Convert.ToString(ret, 2) + str;
            return (Convert.ToInt64(msgs, 2)).ToString();
        }

        public static void check_json(JToken j)
        {
            if (j["error"] != null && j["error"].Type != JTokenType.Null)
            {
                if (j["errorDescription"] != null)
                {
                    // "errorDescription" is in the users own language!
                    throw new FBchatFacebookError(string.Format("Error #{0} when sending request: {1}", j["error"], j["errorDescription"]), fb_error_code: j["error"].Value<string>(), fb_error_message: j["errorDescription"].Value<string>());
                }
                else if (j["error"]["debug_info"] != null)
                {
                    throw new FBchatFacebookError(string.Format("Error #{0} when sending request: {1}", j["error"]["code"].Value<int>(), j["error"]["debug_info"].Value<string>()), fb_error_code: j["error"]["code"].Value<string>(), fb_error_message: j["error"]["debug_info"].Value<string>());
                }
                else
                {
                    throw new FBchatFacebookError(string.Format("Error {0} when sending request", j["error"]), fb_error_code: j["error"].Value<string>());
                }
            }
        }

        public static async Task<object> checkRequest(HttpResponseMessage r, bool as_json = true)
        {
            if (!r.IsSuccessStatusCode)
                throw new FBchatFacebookError(string.Format("Error when sending request: Got {0} response", r.StatusCode), request_status_code: (int)r.StatusCode);

            var buffer = await r.Content.ReadAsByteArrayAsync();
            if (buffer == null || buffer.Length == 0)
                throw new FBchatFacebookError("Error when sending request: Got empty response");
            string content = get_decoded(buffer);

            if (as_json)
            {
                content = strip_to_json(content);
                try
                {
                    JToken j = JToken.Parse(content);
                    check_json(j);
                    return j;
                }
                catch (Exception)
                {
                    throw new FBchatFacebookError(string.Format("Error while parsing JSON: {0}", content));
                }
            }
            else
            {
                return content;
            }
        }

        public static JToken get_jsmods_require(JToken j, int index)
        {
            if (j["jsmods"] != null && j["jsmods"]["require"] != null)
            {
                try
                {
                    return j["jsmods"]["require"][0][index][0];
                }
                catch (Exception)
                {
                    // Debug.WriteLine("Error when getting jsmods_require: {0}. Facebook might have changed protocol", j);
                    return null;
                }
            }
            return null;
        }

        public static EmojiSize? get_emojisize_from_tags(JToken tags)
        {
            if (tags == null)
                return null;
            var tmp = tags.Where(tag => tag.Value<string>().StartsWith("hot_emoji_size:"));
            if (tmp.Any())
            {
                try
                {
                    return Constants.LIKES[tmp.First().Value<string>().Split(':')[1]];
                }
                catch (Exception)
                {
                    // Debug.WriteLine("Could not determine emoji size from {0} - {1}", tags, tmp);
                    return null;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class Constants
    {
        public static readonly Dictionary<string, EmojiSize> LIKES = new Dictionary<string, EmojiSize>() {
            { "large", EmojiSize.LARGE },
            { "medium", EmojiSize.MEDIUM },
            { "small", EmojiSize.SMALL },
            { "l", EmojiSize.LARGE },
            { "m", EmojiSize.MEDIUM },
            { "s", EmojiSize.SMALL }
        };

        public static readonly Dictionary<string, MessageReaction> REACTIONS = new Dictionary<string, MessageReaction>() {
            { "😍", MessageReaction.LOVE },
            { "😆", MessageReaction.SMILE },
            { "😮", MessageReaction.WOW },
            { "😢", MessageReaction.SAD },
            { "😠", MessageReaction.ANGRY },
            { "👍", MessageReaction.YES },
            { "👎", MessageReaction.NO }
        };

        /*
        public static readonly Dictionary<string, Tuple<string, string>> MessageReactionFix = new Dictionary<string, Tuple<string, string>>() {
            { "😍", new Tuple<string, string>("0001f60d", "%F0%9F%98%8D")},
            { "😆", new Tuple<string, string>("0001f606", "%F0%9F%98%86")},
            { "😮", new Tuple<string, string>("0001f62e", "%F0%9F%98%AE")},
            { "😢", new Tuple<string, string>("0001f622", "%F0%9F%98%A2")},
            { "😠", new Tuple<string, string>("0001f620", "%F0%9F%98%A0")},
            { "👍", new Tuple<string, string>("0001f44d", "%F0%9F%91%8D")},
            { "👎", new Tuple<string, string>("0001f44e", "%F0%9F%91%8E")}
        };
        */
    }

    internal class GENDER
    {
        public static readonly Dictionary<int, string> standard_GENDERS = new Dictionary<int, string>() {
            {0, "unknown"},
            {1, "female_singular"},
            {2, "male_singular"},
            {3, "female_singular_guess"},
            {4, "male_singular_guess"},
            {5, "mixed"},
            {6, "neuter_singular"},
            {7, "unknown_singular"},
            {8, "female_plural"},
            {9, "male_plural"},
            {10, "neuter_plural"},
            {11, "unknown_plural" },
        };

        public static readonly Dictionary<string, string> graphql_GENDERS = new Dictionary<string, string>()
        {
            { "UNKNOWN", "unknown" },
            { "FEMALE", "female_singular" },
            { "MALE", "male_singular" },
        };
    }

    /// <summary>
    /// A class containing all urls used by `fbchat-sharp`
    /// </summary>
    public class ReqUrl
    {
        // A class containing all urls used by `fbchat`
        public static readonly string SEARCH = "https://www.facebook.com/ajax/typeahead/search.php";
        public static readonly string LOGIN = "https://m.facebook.com/login.php?login_attempt=1";
        public static readonly string SEND = "https://www.facebook.com/messaging/send/";
        public static readonly string UNREAD_THREADS = "https://www.facebook.com/ajax/mercury/unread_threads.php";
        public static readonly string UNSEEN_THREADS = "https://www.facebook.com/mercury/unseen_thread_ids/";
        public static readonly string THREADS = "https://www.facebook.com/ajax/mercury/threadlist_info.php";
        public static readonly string MESSAGES = "https://www.facebook.com/ajax/mercury/thread_info.php";
        public static readonly string READ_STATUS = "https://www.facebook.com/ajax/mercury/change_read_status.php";
        public static readonly string DELIVERED = "https://www.facebook.com/ajax/mercury/delivery_receipts.php";
        public static readonly string MARK_SEEN = "https://www.facebook.com/ajax/mercury/mark_seen.php";
        public static readonly string BASE = "https://www.facebook.com";
        public static readonly string MOBILE = "https://m.facebook.com/";
        public static readonly string LISTEN = "https://0-edge-chat.facebook.com/";
        public static string STICKY = "https://0-edge-chat.facebook.com/pull";
        public static string PING = "https://0-edge-chat.facebook.com/active_ping";
        public static readonly string UPLOAD = "https://upload.facebook.com/ajax/mercury/upload.php";
        public static readonly string INFO = "https://www.facebook.com/chat/user_info/";
        public static readonly string CONNECT = "https://www.facebook.com/ajax/add_friend/action.php?dpr=1";
        public static readonly string REMOVE_USER = "https://www.facebook.com/chat/remove_participants/";
        public static readonly string LOGOUT = "https://www.facebook.com/logout.php";
        public static readonly string ALL_USERS = "https://www.facebook.com/chat/user_info_all";
        public static readonly string SAVE_DEVICE = "https://m.facebook.com/login/save-device/cancel/";
        public static readonly string CHECKPOINT = "https://m.facebook.com/login/checkpoint/";
        public static readonly string THREAD_COLOR = "https://www.facebook.com/messaging/save_thread_color/?source=thread_settings&dpr=1";
        public static readonly string THREAD_NICKNAME = "https://www.facebook.com/messaging/save_thread_nickname/?source=thread_settings&dpr=1";
        public static readonly string THREAD_EMOJI = "https://www.facebook.com/messaging/save_thread_emoji/?source=thread_settings&dpr=1";
        public static readonly string MESSAGE_REACTION = "https://www.facebook.com/webgraphql/mutation";
        public static readonly string TYPING = "https://www.facebook.com/ajax/messaging/typ.php";
        public static readonly string GRAPHQL = "https://www.facebook.com/api/graphqlbatch/";
        public static readonly string ATTACHMENT_PHOTO = "https://www.facebook.com/mercury/attachments/photo/";
        public static readonly string EVENT_REMINDER = "https://www.facebook.com/ajax/eventreminder/create";

        public int pull_channel = 0;

        public void change_pull_channel(int? channel = 0)
        {
            if (channel == null)
            {
                this.pull_channel = (this.pull_channel + 1) % 5;
            }
            else
            {
                this.pull_channel = (int)channel;
            }
            ReqUrl.STICKY = string.Format("https://{0}-edge-chat.facebook.com/pull", this.pull_channel);
            ReqUrl.PING = string.Format("https://{0}-edge-chat.facebook.com/active_ping", this.pull_channel);
        }

        public static readonly string facebookEncoding = "UTF-8";
    }
}
