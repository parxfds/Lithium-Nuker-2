using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// All of the settings can be changed from here

namespace LithiumNukerV2
{
    public class Settings
    {
        public static bool Debug = false;
        
        public static readonly string Logo = @"
  ██╗     ██╗   ██╗███╗  ██╗ █████╗
  ██║     ██║   ██║████╗ ██║██╔══██╗ 
  ██║     ██║   ██║██╔██╗██║███████║
  ██║     ██║   ██║██║╚████║██╔══██║
  ███████╗╚██████╔╝██║ ╚███║██║  ██║
  ╚══════╝ ╚═════╝ ╚═╝  ╚══╝╚═╝  ╚═╝";
        public static string Token;
        public static long? GuildId = null;
        public static int Threads = 40;
        public static int ConnectionLimit = 25; // 10 concurrent connections
        public static readonly string WebhookName = "LunaNuker";
        public static readonly string AvatarUrl = "";
    }
}
