// System
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Drawing;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.Win32;

// Custom
using Veylib;
using Veylib.CLIUI;
using Veylib.Authentication;

// Nuget
using Newtonsoft.Json;
using System.Reflection;

/*
 * Nuking com is shit
 *  - verlox 2.14.2022
 * */

namespace LunaNuker
{
    internal class Entry
    {
        // Setup CLIUI
        public static Core core = Core.GetInstance();

        // Parse entry point args
        private static void parseArgs(string[] args)
        {
            for (var x = 0;x < args.Length;x++)
            {
                bool succ;

                switch (args[x].ToLower())
                {
                    // Put this shit in debug
                    case "--debug":
                        Settings.Debug = true;
                        break;
                    // Set token on start
                    case "--token":
                        x++;
                        Settings.Token = args[x];
                        break;
                    // Set guild id
                    case "--guild":
                        x++;
                        succ = long.TryParse(args[x], out long lid);
                        if (!succ)
                            core.WriteLine(Color.Red, "--guild argument value invalid");
                        Settings.GuildId = lid;
                        break;
                    // Set threads
                    case "--threads":
                        x++;
                        succ = int.TryParse(args[x], out int threads);
                        if (!succ)
                            core.WriteLine(Color.Red, "--threads argument value invalid");
                        Settings.Threads = threads;
                        break;
                    // Set connection limit
                    case "--connection-limit":
                        x++;
                        succ = int.TryParse(args[x], out int connlimit);
                        if (!succ)
                            core.WriteLine(Color.Red, "--connection-limit argument value invalid");
                        Settings.ConnectionLimit = connlimit;
                        break;
                    // Means that there was an unparsed arg that is unknown
                    default:
                        core.WriteLine(Color.Red, $"Invalid argument: {args[x].ToLower()}");
                        break;
                }
            }
        }

        // Entry point
        static void Main(string[] args)
        {
            // No.
            #if DEBUG
            Settings.Debug = true;
            #endif

            #region Setting up the UI
            var props = new Core.StartupProperties { 
                MOTD = "fastest deletion of servers.", 
                ColorRotation = 260, 
                SilentStart = true, 
                LogoString = Settings.Logo,
                DebugMode = Settings.Debug, 
                Author = new Core.StartupAuthorProperties { 
                    Url = "discord.gg/Lunaaa", 
                    Name = "Luna"
                }, 
                Title = new Core.StartupConsoleTitleProperties { 
                    Text = "LunaNuker"
                } 
            };
            core.Start(props);
            #endregion

            // Parse the args
            parseArgs(args);

            /*
             * Fuck version shit
             * 
            // Check version
            var v = Vars.Get("loader_version", -1);

            if (v.State == Vars.VarState.Success)
            {
                var version = Version.Parse(v.Value);
                if (version > LithiumShared.GetVersion())
                {
                    core.WriteLine(new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Text = "fail" } }, "This client is outdated, download a new client from the Discord, press any key to close");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                core.WriteLine(new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Text = "fail" } }, "Failed to check version, press any key to close");
                Console.ReadKey();
                return;
            }
            */

            // Setup the stupid ass connection limits
            ServicePointManager.DefaultConnectionLimit = Settings.ConnectionLimit;
            ServicePointManager.Expect100Continue = false;

            // Open options
            Picker.Choose();
        }
    }
}
