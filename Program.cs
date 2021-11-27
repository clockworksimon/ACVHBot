using System;
using System.Threading.Tasks;

namespace ACVillagerHuntBot
{
    class Program
    {
        public static ConfigurationBot configBot = new ConfigurationBot();

        static async Task Main(string[] args)
        {
            VillagerHuntBot huntBot = new VillagerHuntBot(configBot);
            TwitchBot twitchBot = new TwitchBot(huntBot, configBot);
            

            Console.WriteLine("\n\nWelcome to the bot.");

            if (configBot.CanConnect) {
                await twitchBot.Connect();
            }
            else {
                Console.WriteLine("You need to provide connection details for the bot to log into Twitch. Press enter to open the config options.");
            }

            string strUserInput = "";
            bool bFireConfig = false;
            while (true) {
                Console.Write("> ");
                strUserInput = Console.ReadLine();
                if (string.IsNullOrEmpty(strUserInput)) {
                    if (!configBot.CanConnect) {
                        bFireConfig = true;
                    }
                }
                else {
                    strUserInput = strUserInput.Trim().Replace("\"", "").Replace("!", "").Trim();
                    if (strUserInput.StartsWith("quit", true, null) || strUserInput.StartsWith("exit", true, null) || strUserInput.StartsWith("q", true, null)) {
                        break;
                    }
                    else if (strUserInput.StartsWith("config", true, null) || strUserInput.StartsWith("setup", true, null)) {
                        bFireConfig = true;
                    }
                    else if (strUserInput.StartsWith("path", true, null)) {
                        Console.WriteLine($"Path: {configBot.OBSPathForIslandCounter(huntBot.Hunt)}");
                    }
                }

                if (bFireConfig) {
                    bFireConfig = false;
                    Console.WriteLine("Opening local config in your default web browser, please refer to open webpage to configure.");
                    configBot.EnterConfig();
                    if (twitchBot.ConnectionState)
                    {
                        if (!string.IsNullOrEmpty(twitchBot.CurrentChannel) && !twitchBot.CurrentChannel.Equals(configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchChannel])) {
                            await twitchBot.ChangeChannels(twitchBot.CurrentChannel, configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchChannel]);
                        }
                    }
                    else {
                        if (configBot.CanConnect) {
                            await twitchBot.Connect();
                        }
                        else {
                            Console.WriteLine("Still missing connection details. Press enter to try again or type \"quit\" and press enter to exit the bot.");
                        }
                    }
                }
            }
        }
    }
}