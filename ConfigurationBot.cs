using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ACVillagerHuntBot
{
    public class BotCommand {
        public string Name {get;set;}
        public string Message {get;set;}
        public string Params {get;set;}

        private string _perms {get;set;}
    }

    public class ApplicationDetails {
        private System.Reflection.Assembly _currentAssembly;
        public string ApplicationName { get { return _currentAssembly.GetName().Name; } }
        public string ApplicaitonVersion { get { return _currentAssembly.GetName().Version.ToString(); } }
        public ApplicationDetails() {
             _currentAssembly = System.Reflection.Assembly.GetEntryAssembly();
            if (_currentAssembly == null) {
                _currentAssembly = System.Reflection.Assembly.GetCallingAssembly();
            }
        }
    }

    public class DataProtection {
        private IDataProtectionProvider _dataProtectionProvider;
        private IDataProtector _protector;
        public DataProtection(string AppName) {
            _dataProtectionProvider = DataProtectionProvider.Create(AppName);
            _protector = _dataProtectionProvider.CreateProtector("Program.No-DI");
        }

        public string Encrypt(string Plaintext) {
            return _protector.Protect(Plaintext);
        }
        public string Decrypt(string ProtectedText) {
            return _protector.Unprotect(ProtectedText);
        }
    }
    public class ConfigurationBot
    {
        public Dictionary<string, BotCommand> BotCommands = new Dictionary<string, BotCommand>();
        public enum SystemParams {
            DocsBotSystemConfigName,
            DocsBotCommandsConfigname,
            DocsBotFolderName,
            DocsVillagerListSuffix,
            DocsGuessListSuffix,
            DocsCounterListSuffix,
            DocsPerHuntDataSuffix,
            DocsBotMessagesConfigname,
            TwitchServer,
            TiwtchPort,
            TwitchUsername,
            TwitchOAuth,
            TwitchChannel,
            BotChatCommand,
            BotRegisterGuessCommand,
            BotFollowHunt,
            DocsConfigHTMLSuffix
        }
        public struct UserConfigItem
        {
            public Enum ToConfigure;
            public int Order;
            public string FormFieldType;
            public string FormLabel;
            public string FormHelpText;

        }
        public List<UserConfigItem> UserConfig = new List<UserConfigItem>();
        public Dictionary<SystemParams, string> BotSystemParams = new Dictionary<SystemParams, string>();
        private string FileNameHuntVillagerList = String.Empty;
        private string FileNameHuntGuessList = String.Empty;
        private string FileNameHuntCounterList = String.Empty;
        private string FileNamePerHuntDataList = String.Empty;
        private Task SystemParamsFileSaving;

        public enum Messages {
            BeginHunt,
            WillFindOnIslandAccordingTo,
            LeftHeadingTo,
            FoundHuntEnded,
            FoundOnCongrats,
            FinalGuess,
            OnTheWayTo,
            UndefinedHunt,
            NoGuessForIsland,
            GuessNotFound,
            RecordedTheseGuesses,
            AltWillFindOnIslandAccordingTo,
            DescribeBot,
            RewindHuntWithReplacement,
            RewindHunt,
            TimesSeenInHunt,
            NotSeenInHunt,
            ListOfLastSeenVillagers,
            IslandCountSetTo,
            GuessNotRecordedBecauseHuntComplete,
            BotStarting,
            BingoLastCalled,
            BotJoined,
            RenameHuntSuccess,
            RenameHuntFail,
            VillagerSeenOnIsland
        }

        public ApplicationDetails AppDetails = new ApplicationDetails();
        public DataProtection Protect;

        public enum HuntStorage {
            Bingo
        }
        public Dictionary<Messages, string> MessagesText = new Dictionary<Messages, string>();

        public static HttpListener hConfigServer;

        public bool CanConnect {
            get {
                return BotSystemParams != null
                    && BotSystemParams.ContainsKey(SystemParams.TwitchServer) && !string.IsNullOrEmpty(BotSystemParams[SystemParams.TwitchServer])
                    && BotSystemParams.ContainsKey(SystemParams.TwitchOAuth) && !string.IsNullOrEmpty(BotSystemParams[SystemParams.TwitchOAuth])
                    && BotSystemParams.ContainsKey(SystemParams.TwitchUsername) && !string.IsNullOrEmpty(BotSystemParams[SystemParams.TwitchUsername])
                    && BotSystemParams.ContainsKey(SystemParams.TwitchChannel) && !string.IsNullOrEmpty(BotSystemParams[SystemParams.TwitchChannel]);
            }
        }

        public ConfigurationBot() {
            Protect = new DataProtection(AppDetails.ApplicationName);

            BotSystemParams.Add(SystemParams.DocsBotSystemConfigName, "acvhbot-system-config.txt");
            BotSystemParams.Add(SystemParams.DocsBotCommandsConfigname, "acvhbot-commands-config.txt");
            BotSystemParams.Add(SystemParams.DocsBotMessagesConfigname, "acvhbot-messages-config.txt");
            BotSystemParams.Add(SystemParams.DocsConfigHTMLSuffix, "acvhbot-config-template.txt");
            BotSystemParams.Add(SystemParams.DocsBotFolderName, "ACVHBot");
            BotSystemParams.Add(SystemParams.DocsVillagerListSuffix, "-VillagerList.txt");
            BotSystemParams.Add(SystemParams.DocsGuessListSuffix, "-GuessList.txt");
            BotSystemParams.Add(SystemParams.DocsCounterListSuffix, "-Counter.txt");
            BotSystemParams.Add(SystemParams.DocsPerHuntDataSuffix, "-Data.txt");
            BotSystemParams.Add(SystemParams.TwitchServer, "irc.chat.twitch.tv");
            BotSystemParams.Add(SystemParams.TiwtchPort, "6697");
            BotSystemParams.Add(SystemParams.TwitchUsername, "");
            BotSystemParams.Add(SystemParams.TwitchOAuth, "");
            BotSystemParams.Add(SystemParams.TwitchChannel, "");
            BotSystemParams.Add(SystemParams.BotChatCommand, "!vh");
            BotSystemParams.Add(SystemParams.BotRegisterGuessCommand, "!myguess");
            BotSystemParams.Add(SystemParams.BotFollowHunt, "+off");

            MessagesText.Add(Messages.BeginHunt, "/me the hunt for {0} commences this day on island {1}");
            MessagesText.Add(Messages.WillFindOnIslandAccordingTo, "/me we'll find {0} on island {1} according to: {2}");
            MessagesText.Add(Messages.LeftHeadingTo, "/me we left {0} on island {1} and are heading to island {2}");
            MessagesText.Add(Messages.FoundHuntEnded, "/me we found {0} on island {1} and that's where the hunt ended. Congratulations!");
            MessagesText.Add(Messages.FoundOnCongrats, "/me we found {0} on island {1}!! Congratulations!");
            MessagesText.Add(Messages.FinalGuess, "/me the final island was the guess of: {0}");
            MessagesText.Add(Messages.OnTheWayTo, "/me we are on the way to island {0}.");
            MessagesText.Add(Messages.UndefinedHunt, "/me no hunt is defined. issue !vh sethunt <name> to configure.");
            MessagesText.Add(Messages.NoGuessForIsland, "/me nobody has a guess for island {0}");
            MessagesText.Add(Messages.GuessNotFound, "/me ope-- {0} was not found anywhere in the list of guesses 4Head");
            MessagesText.Add(Messages.RecordedTheseGuesses, "/me recorded these guesses for {0}: {1}");
            MessagesText.Add(Messages.AltWillFindOnIslandAccordingTo, "/me but we'll find {0} on island {1} according to: {2}");
            MessagesText.Add(Messages.DescribeBot, "/me on which island will we find {0}? Guess with !myguess number, ex: !myguess 69 or !myguess 420,469,1102 for multiples!");
            MessagesText.Add(Messages.RewindHuntWithReplacement, "/me rewinding the hunt: we last saw {0} on island {1} and are heading to island {2}. It's like your mistake never happened...");
            MessagesText.Add(Messages.RewindHunt, "/me rewinding the hunt: we're now heading to island {0}. It's like your mistake never happened...");
            MessagesText.Add(Messages.TimesSeenInHunt, "/me we've seen {0} {1} time|times [{2}] during this hunt for {3}");
            MessagesText.Add(Messages.VillagerSeenOnIsland, "/me we saw {0} on island {1} during this hunt for {2}");
            MessagesText.Add(Messages.NotSeenInHunt, "/me we haven't seen {0} yet during this hunt for {1}!");
            MessagesText.Add(Messages.ListOfLastSeenVillagers, "/me {0}");
            MessagesText.Add(Messages.IslandCountSetTo, "/me the island count has been set to {0}");
            MessagesText.Add(Messages.GuessNotRecordedBecauseHuntComplete, "/me the hunt for {0} ended on island {1}. Guesses are no longer being recorded.");
            MessagesText.Add(Messages.BotStarting, "AC Villager Hunt Bot Starting...");
            MessagesText.Add(Messages.BingoLastCalled, "/me bingo was last called by {0} on island {1}");
            MessagesText.Add(Messages.BotJoined, "/me reporting for duty");
            MessagesText.Add(Messages.RenameHuntSuccess, "/me the hunt for {0} has been renamed to the hunt for {1}");
            MessagesText.Add(Messages.RenameHuntFail, "/me the hunt for {0} was not renamed (Problem creating files under {1}, data may exist from a past hunt)");

            UserConfig.Add(new UserConfigItem { ToConfigure = SystemParams.TwitchUsername, Order = 1, FormFieldType = "text", FormLabel = "Bot Username", FormHelpText = "The Twitch Account that the bot will connect as. You may want to create a separate account for the bot but you can run it under an existing one (Such as your main channel bot or your own account) if you want." } );
            UserConfig.Add(new UserConfigItem { ToConfigure = SystemParams.TwitchOAuth, Order = 3, FormFieldType = "password", FormLabel = "Bot OAUTH Token", FormHelpText = "<p>To get an OAUTH token visit <a href=\"https://twitchapps.com/tmi/\" target=\"_blank\">this page</a> while logged in as the Twitch user you want the bot to run as: <a href=\"https://twitchapps.com/tmi/\" target=\"_blank\">Twitchapps Token Generator</a>. Note that you can right-click on the link and open with Incognito/Private browsing to avoid linking the bot to your main Twitch account.</i></p><p><i>After visiting the above page and following the instructions, copy the whole oauth token (including the \"oauth:\" text) and paste it into the above field.</i></p>" } );
            UserConfig.Add(new UserConfigItem { ToConfigure = SystemParams.TwitchChannel, Order = 5, FormFieldType = "text", FormLabel = "Stream to Chat In", FormHelpText = "The name of the Twitch Stream that the bot will join." } );
            UserConfig.Add(new UserConfigItem { ToConfigure = SystemParams.BotChatCommand, Order = 7, FormFieldType = "text", FormLabel = "Bot Mod Control Command", FormHelpText = "The command you or your moderators will use to interact with the bot. Is set to <strong>!vh</strong> by default." } );
            UserConfig.Add(new UserConfigItem { ToConfigure = SystemParams.BotRegisterGuessCommand, Order = 9, FormFieldType = "text", FormLabel = "User Guess Command", FormHelpText = "The command chat uses to record their guess for the island number(s) that the hunt will end on. Is set to <strong>!myguess</strong> by default." } );

            LoadConfig();
        }

        public void EnterConfig() {
            string strFields = "";
            string strTmpValue = "";
            string strName = "";
            int iTmpL = 0;

            UserConfig.OrderBy(x => x.Order).ToList<UserConfigItem>().ForEach(uci => {
                strName = Enum.GetName(uci.ToConfigure.GetType(), uci.ToConfigure);
                strTmpValue = BotSystemParams[(ConfigurationBot.SystemParams)Enum.Parse(typeof(ConfigurationBot.SystemParams), strName)];
                iTmpL = (int)(strTmpValue.Length * 1.5);
                if (iTmpL > 60) {
                    iTmpL = 60;
                }
                strFields += String.Format("<div class=\"row\">\n <div class=\"label\">{0}</div>\n <div class=\"field\"><input type=\"{1}\" name=\"{2}\" value=\"{3}\"  size=\"{4}\" /> <i>{5}</i></div>\n </div>\n", uci.FormLabel, uci.FormFieldType, strName, strTmpValue, iTmpL, uci.FormHelpText);
            });

            StartConfigByWeb(strFields);
        }

        public string GetStorageName(object Param) {
            return Enum.GetName(Param.GetType(), Param);
        }

        public string GetMessageTextWithCorrectPlural(Messages Msg, int NumberCount) {
            string retVal = MessagesText[Msg];

            string[] strTokens = retVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach(string strToken in strTokens) {
                if (strToken.IndexOf('|') > 0) {
                    string[] strPs = strToken.Split('|');
                    if (strPs.Length == 2) {
                        if (NumberCount == 1) {
                            retVal = retVal.Replace(strToken, strPs[0]);
                        }
                        else {
                            retVal = retVal.Replace(strToken, strPs[1]);
                        }
                    }
                }
            }

            return retVal;
        }

        public static void OpenBrowser(string url)
        {
            //https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
            //hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Process.Start(url);
            }
        }

        public void StartConfigByWeb(string HTMLCode) {
            //https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
            hConfigServer = new HttpListener();
            hConfigServer.Prefixes.Add("http://localhost:8089/");

            try
            {
                hConfigServer.Start();
                Task ServerMsgs =  HandleIncomingConnections(HTMLCode);
                ConfigurationBot.OpenBrowser("http://localhost:8089/");
                ServerMsgs.GetAwaiter().GetResult();
                hConfigServer.Close();
            }
            catch
            {
                hConfigServer.Close();
            }

        }

        public string OAuthPass {
            get {
                string strOApass = BotSystemParams[SystemParams.TwitchOAuth];
                if (strOApass.StartsWith("oauth", true, null)) {
                    return strOApass;
                }
                else {
                    return Protect.Decrypt(strOApass);
                }
            }
        }

        public async Task HandleIncomingConnections(string HTMLPage) {
            bool IsRunning = true;
            while (IsRunning) {
                HttpListenerContext hConfigContext = await hConfigServer.GetContextAsync();

                HttpListenerRequest hccListenerRequest = hConfigContext.Request;
                HttpListenerResponse hccListenerResponse = hConfigContext.Response;

                string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
                string strConfigHTMLPath = Path.Combine(strConfBasePath, BotSystemParams[SystemParams.DocsConfigHTMLSuffix]);

                string strPageOut = "<html>\n<head>\n<title>ACVHBot Config Page</title>\n<style type=\"text/css\">\n div.row { width: 100%; min-height: 1.8rem; margin: 0 0 0.4rem 0; clear: both; float: none; }\n div.row div.label { float: left; width: 11rem; padding-right: 16px; }\n div.row div.field { float: left; width: 81%; }\n</style>\n</head>\n<body>\n<form method=\"POST\" action=\"/\">\n<h1>Config for ACVHBot</h1>\n<p><strong>Caution: Please use the \"cancel\" or \"Save and Exit\" buttons to leave this page.</strong> The bot waits for configuration to complete, so it will hang if you close the tab before it's ready. This page will update when it's safe to close the tab.</p>\n\n[[FormBody]]\n\n<input type=\"submit\" name=\"btnCancel\" value=\"Cancel\"> &nbsp; &nbsp; <input type=\"submit\" name=\"btnSubmit\" value=\"Save and Exit Config Mode\">\n</form>\n\n</body>\n</html>\n";
                if (File.Exists(strConfigHTMLPath)) {
                    strPageOut = File.ReadAllText(strConfigHTMLPath);
                }

                strPageOut = strPageOut.Replace("[[FormBody]]", HTMLPage);

                if (hccListenerRequest.Url.AbsolutePath == "/" && hccListenerRequest.HttpMethod == "POST") {
                    IsRunning = false;
                    strPageOut = "<html><head><title>ACVHBot Config Complete</title></head><body><p>please close this tab and refer back to the app window.</p></body></html>";

                    string strFieldData = "";
                    if (hccListenerRequest.HasEntityBody) {
                        using (System.IO.Stream body = hccListenerRequest.InputStream) {
                            using (var reader = new System.IO.StreamReader(body, hccListenerRequest.ContentEncoding)) {
                                strFieldData = reader.ReadToEnd();
                            }
                        }

                        System.Collections.Specialized.NameValueCollection nvcParams = HttpUtility.ParseQueryString(strFieldData);
                        if (nvcParams.AllKeys.Contains("btnSubmit")) {
                            bool bSaveNeeded = false;
                            string[] straSPs = Enum.GetNames(typeof(ConfigurationBot.SystemParams));
                            foreach(string strKey in nvcParams.AllKeys) {
                                if (strKey.Equals(Enum.GetName(ConfigurationBot.SystemParams.TwitchOAuth)) && nvcParams[strKey].ToString().StartsWith("oauth", true, null)) {
                                    nvcParams[strKey] = Protect.Encrypt(nvcParams[strKey]);
                                }
                                Console.WriteLine(String.Format("Key: {0} Value: {1}", strKey, nvcParams[strKey]));
                                if (straSPs.Contains(strKey) && (!BotSystemParams[(ConfigurationBot.SystemParams)Enum.Parse(typeof(ConfigurationBot.SystemParams), strKey)].Equals(nvcParams[strKey]))) {
                                    BotSystemParams[(ConfigurationBot.SystemParams)Enum.Parse(typeof(ConfigurationBot.SystemParams), strKey)] = nvcParams[strKey];
                                    bSaveNeeded = true;
                                }
                            }
                            if (bSaveNeeded) {
                                SaveSystemParams();
                            }
                        }

                    }
                }

                byte[] baPageData = Encoding.UTF8.GetBytes(strPageOut);
                hccListenerResponse.ContentType = "text/html";
                hccListenerResponse.ContentEncoding = Encoding.UTF8;
                hccListenerResponse.ContentLength64 = baPageData.LongLength;

                hccListenerResponse.OutputStream.Write(baPageData, 0, baPageData.Length);
                hccListenerResponse.Close();

                if (!IsRunning) {
                    hConfigContext = await hConfigServer.GetContextAsync();
                }
            }
            
        }

        public void LoadConfig() {
            string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
            string strConfigSystemFilename = Path.Combine(strConfBasePath, BotSystemParams[SystemParams.DocsBotSystemConfigName]);
            string strConfigCommandsFilename = Path.Combine(strConfBasePath, BotSystemParams[SystemParams.DocsBotCommandsConfigname]);
            string strConfigMessagesFilename = Path.Combine(strConfBasePath, BotSystemParams[SystemParams.DocsBotMessagesConfigname]);
            string strConfigFileText = String.Empty;
            if (Directory.Exists(strConfBasePath) && File.Exists(strConfigSystemFilename)) {
                strConfigFileText = File.ReadAllText(strConfigSystemFilename);
                if (!string.IsNullOrEmpty(strConfigFileText)) {
                    try {
                        Dictionary<SystemParams, string> botPT = new Dictionary<SystemParams, string>();
                        botPT = System.Text.Json.JsonSerializer.Deserialize<Dictionary<SystemParams, string>>(strConfigFileText);
                        foreach(SystemParams sKey in BotSystemParams.Keys) {
                            if (!botPT.ContainsKey(sKey)) {
                                botPT.Add(sKey, BotSystemParams[sKey]);
                            }
                        }
                        BotSystemParams = botPT;
                    }
                    catch (System.Text.Json.JsonException ex) {
                        Console.WriteLine($"System Config didn't load because error: {ex.Message}");
                    }
                }
            }
            if (Directory.Exists(strConfBasePath) && File.Exists(strConfigCommandsFilename)) {
                strConfigFileText = File.ReadAllText(strConfigCommandsFilename);
                if (!string.IsNullOrEmpty(strConfigFileText)) {
                    try {
                        BotCommands = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, BotCommand>>(strConfigFileText);
                    }
                    catch (System.Text.Json.JsonException ex) {
                        Console.WriteLine($"Commands Config didn't load because error: {ex.Message}");
                    }
                }
            }
            if (Directory.Exists(strConfBasePath) && File.Exists(strConfigMessagesFilename)) {
                strConfigFileText = File.ReadAllText(strConfigMessagesFilename);
                if (!string.IsNullOrEmpty(strConfigFileText)) {
                    try {
                        MessagesText = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Messages, string>>(strConfigFileText);
                    }
                    catch (System.Text.Json.JsonException ex) {
                        Console.WriteLine($"Messages Config didn't load because error: {ex.Message}");
                    }
                }
            }
            strConfigFileText = null;
        }

        public string OBSPathForIslandCounter(string ForHunt) {
            string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
            string strHuntAsFilename = ForHunt;
            foreach(var c in Path.GetInvalidFileNameChars()) {
                strHuntAsFilename = strHuntAsFilename.Replace(c.ToString(), "");
            }
            return Path.Combine(strConfBasePath, strHuntAsFilename + BotSystemParams[SystemParams.DocsCounterListSuffix]);
        }

        public bool RenameHunt(string CurrentHuntName, string NewHuntName) {
            bool retVal = false;

            string strNewHuntAsFilename = NewHuntName;
            foreach(var c in Path.GetInvalidFileNameChars()) {
                strNewHuntAsFilename = strNewHuntAsFilename.Replace(c.ToString(), "");
            }

            string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
            if (!Directory.Exists(strConfBasePath)) {
                Directory.CreateDirectory(strConfBasePath);
            }

            string NewFileNameHuntVillagerList = Path.Combine(strConfBasePath, strNewHuntAsFilename + BotSystemParams[SystemParams.DocsVillagerListSuffix]);
            string NewFileNameHuntGuessList = Path.Combine(strConfBasePath, strNewHuntAsFilename + BotSystemParams[SystemParams.DocsGuessListSuffix]);
            string NewFileNameHuntCounterList = Path.Combine(strConfBasePath, strNewHuntAsFilename + BotSystemParams[SystemParams.DocsCounterListSuffix]);
            string NewFileNamePerHuntDataList = Path.Combine(strConfBasePath, strNewHuntAsFilename + BotSystemParams[SystemParams.DocsPerHuntDataSuffix]);

            if (File.Exists(FileNameHuntVillagerList) && !File.Exists(NewFileNameHuntVillagerList)) {
                File.Move(FileNameHuntVillagerList, NewFileNameHuntVillagerList);
            }

            if (File.Exists(FileNameHuntGuessList) && !File.Exists(NewFileNameHuntGuessList)) {
                File.Move(FileNameHuntGuessList, NewFileNameHuntGuessList);
            }

            if (File.Exists(FileNameHuntCounterList) && !File.Exists(NewFileNameHuntCounterList)) {
                File.Move(FileNameHuntCounterList, NewFileNameHuntCounterList);
            }

            if (File.Exists(FileNamePerHuntDataList) && !File.Exists(NewFileNamePerHuntDataList)) {
                File.Move(FileNamePerHuntDataList, NewFileNamePerHuntDataList);
            }

            if (File.Exists(NewFileNameHuntVillagerList) && File.Exists(NewFileNameHuntGuessList) && File.Exists(NewFileNameHuntCounterList) && File.Exists(NewFileNamePerHuntDataList)) {
                FileNameHuntVillagerList = NewFileNameHuntVillagerList;
                FileNameHuntGuessList = NewFileNameHuntGuessList;
                FileNameHuntCounterList = NewFileNameHuntCounterList;
                FileNamePerHuntDataList = NewFileNamePerHuntDataList;

                if (BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt].Equals(CurrentHuntName, StringComparison.CurrentCultureIgnoreCase)) {
                    BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt] = NewHuntName;
                    SaveSystemParams();
                }

                retVal = true;
            }

            return retVal;
        }

        public (Dictionary<int, string> Guesses, int Islands, Dictionary<string,string> PerHuntData) BeginHunt(string Hunt) {
            Dictionary<int, string> returnGuesses = new Dictionary<int, string>();
            int returnHuntCounter = 1;
            Dictionary<string,string> returnPerHuntData = new Dictionary<string, string>();

            string strHuntAsFilename = Hunt;
            foreach(var c in Path.GetInvalidFileNameChars()) {
                strHuntAsFilename = strHuntAsFilename.Replace(c.ToString(), "");
            }

            string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
            if (!Directory.Exists(strConfBasePath)) {
                Directory.CreateDirectory(strConfBasePath);
            }

            FileNameHuntVillagerList = Path.Combine(strConfBasePath, strHuntAsFilename + BotSystemParams[SystemParams.DocsVillagerListSuffix]);
            FileNameHuntGuessList = Path.Combine(strConfBasePath, strHuntAsFilename + BotSystemParams[SystemParams.DocsGuessListSuffix]);
            FileNameHuntCounterList = Path.Combine(strConfBasePath, strHuntAsFilename + BotSystemParams[SystemParams.DocsCounterListSuffix]);
            FileNamePerHuntDataList = Path.Combine(strConfBasePath, strHuntAsFilename + BotSystemParams[SystemParams.DocsPerHuntDataSuffix]);

            if (!File.Exists(FileNameHuntVillagerList)) {
                using (File.CreateText(FileNameHuntVillagerList)) {};
            }

            if (File.Exists(FileNameHuntGuessList)) {
                string strGuessText = File.ReadAllText(FileNameHuntGuessList);
                if (!string.IsNullOrEmpty(strGuessText)) {
                    try {
                        returnGuesses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(strGuessText);
                    }
                    catch (System.Text.Json.JsonException ex) {
                        Console.WriteLine($"GuessList didn't load because error: {ex.Message}");
                    }
                }
                strGuessText = null;
            }
            else {
                using (File.CreateText(FileNameHuntGuessList)) {};
            }

            if (File.Exists(FileNameHuntCounterList)) {
                int iFCount = 0;
                string[] strFlines = File.ReadAllLines(FileNameHuntCounterList);
                if (strFlines != null && strFlines.Length > 0 && int.TryParse(strFlines[0], out iFCount)) {
                    returnHuntCounter = iFCount;
                }
            }
            else {
                File.WriteAllText(FileNameHuntCounterList, returnHuntCounter.ToString());
            }

            if (File.Exists(FileNamePerHuntDataList)) {
                string strDataText = File.ReadAllText(FileNamePerHuntDataList);
                if (!string.IsNullOrEmpty(strDataText)) {
                    try {
                        returnPerHuntData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(strDataText);
                    }
                    catch (System.Text.Json.JsonException ex) {
                        Console.WriteLine($"PerHuntData didn't load because error: {ex.Message}");
                    }
                }
                strDataText = null;
            }
            else {
                using (File.CreateText(FileNamePerHuntDataList)) {};
            }

            return (returnGuesses, returnHuntCounter, returnPerHuntData);
        }

        public bool SaveHuntCounter(int HuntCounter) {
            if (!string.IsNullOrEmpty(FileNameHuntCounterList) && File.Exists(FileNameHuntCounterList)) {
                File.WriteAllText(FileNameHuntCounterList, HuntCounter.ToString());
                return true;
            }
            return false;
        }

        public bool SaveVillagerList(string VillagerName) {
            if (!string.IsNullOrEmpty(FileNameHuntVillagerList) && File.Exists(FileNameHuntVillagerList)) {
                File.AppendAllLines(FileNameHuntVillagerList, new string[] {VillagerName});
                return true;
            }
            return false;
        }

        public bool SaveGuessList(string SeralizedGuessList) {
            if (!string.IsNullOrEmpty(FileNameHuntGuessList)) {
                File.WriteAllText(FileNameHuntGuessList, SeralizedGuessList);
                return true;
            }
            return false;
        }

        public bool SaveDataList(string SeralizedDataList) {
            if (!string.IsNullOrEmpty(FileNamePerHuntDataList)) {
                File.WriteAllText(FileNamePerHuntDataList, SeralizedDataList);
                return true;
            }
            return false;
        }

        public void SaveSystemParams() {
            if (SystemParamsFileSaving == null) {
                SystemParamsFileSaving = SaveSystemParamsAsync();
            }
        }

        internal async Task SaveSystemParamsAsync() {
            await Task.Delay(5000);
            string strConfBasePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BotSystemParams[SystemParams.DocsBotFolderName]);
            string strConfigSystemFilename = Path.Combine(strConfBasePath, BotSystemParams[SystemParams.DocsBotSystemConfigName]);
            File.WriteAllText(strConfigSystemFilename, System.Text.Json.JsonSerializer.Serialize(BotSystemParams));
            SystemParamsFileSaving = null;
        }

        public bool RewindVillagerList(int HuntCounter, string RewindText) {
            //https://stackoverflow.com/questions/4264117/how-to-delete-last-line-in-a-text-file
            string line = null;
            Random r = new Random();
            string strTempFilename = Path.Combine(Path.GetDirectoryName(FileNameHuntVillagerList), String.Format("{0}-t{1}.txt", Path.GetFileNameWithoutExtension(FileNameHuntVillagerList), r.Next(10000, 999999)));
            File.Copy(FileNameHuntVillagerList, strTempFilename);
            File.Delete(FileNameHuntVillagerList);
            Queue<string> deferredLines = new Queue<string>();
            using (TextReader inputReader = new StreamReader(strTempFilename))
            using (TextWriter outputReader = new StreamWriter(FileNameHuntVillagerList))
            {
                while ((line = inputReader.ReadLine()) != null)
                {
                    if (deferredLines.Count == 1)
                    {
                        outputReader.WriteLine(deferredLines.Dequeue());
                    }

                    deferredLines.Enqueue(line);
                }
                if (!string.IsNullOrEmpty(RewindText)) {
                    outputReader.WriteLine(String.Format("{0}. {1}", HuntCounter, RewindText));
                }
            }
            File.Delete(strTempFilename);

            return true;
        }

        public (int VisitedCounter, string VisitedIslands) GetCountOfVillagerSeenInHunt(string VillagerToFind) {
            int iCounter = 0;
            string strIslands = String.Empty;
            int iIslandNum = 0;
            bool isNumber = int.TryParse(VillagerToFind, out iIslandNum);
            if (!string.IsNullOrEmpty(VillagerToFind)) {
                string line = null;
                using (TextReader inputReader = new StreamReader(FileNameHuntVillagerList))
                {
                    while ((line = inputReader.ReadLine()) != null)
                    {
                        if (isNumber == false && line.EndsWith($". {VillagerToFind}", StringComparison.CurrentCultureIgnoreCase)) {
                            iCounter++;
                            strIslands += String.Format("{0}, ", line.Substring(0, line.IndexOf(".")));
                        }
                        else if (isNumber == true && line.StartsWith($"{VillagerToFind}.", StringComparison.CurrentCultureIgnoreCase)) {
                            iCounter++;
                            strIslands += String.Format("{0}, ", line.Substring(line.IndexOf("."), line.Length - line.IndexOf(".")).TrimStart('.').Trim());
                            break;
                        }
                    }
                }
            }
            return (iCounter, strIslands);
        }

        public string GetLastNVillagersFromFile(int iNumVillagers) {
            //https://stackoverflow.com/questions/4264117/how-to-delete-last-line-in-a-text-file
            string retVal = String.Empty;

            string line = null;
            string strFinal = String.Empty;
            Queue<string> deferredLines = new Queue<string>();
            using (TextReader inputReader = new StreamReader(FileNameHuntVillagerList))
            {
                while ((line = inputReader.ReadLine()) != null)
                {
                    if (deferredLines.Count == iNumVillagers)
                    {
                        deferredLines.Dequeue();
                    }
                    if (!string.IsNullOrEmpty(line)) {
                        deferredLines.Enqueue(line);
                    }
                }
                retVal = String.Join(", ", deferredLines.ToArray());
            }

            return retVal;
        }
    }
}