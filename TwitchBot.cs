using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace ACVillagerHuntBot
{
    public class TwitchBot
    {
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private TcpClient tcpClient = new TcpClient();
        private ConfigurationBot configBot;
        private int _connCount = 0;
        public bool ConnectionState {
            get { return tcpClient.Connected; }
        }
        public event TwitchChatEventHandler OnMessage = delegate { };
        public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);
        private string _currentChannel = "";
        public string CurrentChannel {
            get { return _currentChannel; }
        }

        public class TwitchChatMessage : EventArgs
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
            public string[] MessageTokens { get; set; }
            public bool HasOper { get; set; }

            public bool HasCommand(string command) {
                return HasParam(0, command);
            }

            public bool HasParam(int pos, string param) {
                if (MessageTokens != null && MessageTokens.Length > pos && MessageTokens[pos].Equals(param, StringComparison.CurrentCultureIgnoreCase)) {
                    return true;
                }
                else {
                    return false;
                }
            }

            public bool HasParams() {
                return (MessageTokens != null && MessageTokens.Length > 1);
            }

            public string ParamAt(int pos) {
                return TextAt(pos).ToLower();
            }

            public string TextAt(int pos) {
                if (MessageTokens != null && MessageTokens.Length > pos) {
                    return MessageTokens[pos];
                }
                else {
                    return String.Empty;
                }
            }

            public string TextFrom(int pos) {
                if (MessageTokens != null && MessageTokens.Length > pos) {
                    return String.Join(" ", MessageTokens, pos, MessageTokens.Length - pos);
                }
                else {
                    return String.Empty;
                }
            }
        }

        private VillagerHuntBot huntBot;

        public TwitchBot(VillagerHuntBot hb, ConfigurationBot cb)
        {
            huntBot = hb;
            configBot = cb;
             _connCount = 0;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        public async Task ChangeChannels(string OldChan, string NewChan) {
            await streamWriter.WriteLineAsync($"PART #{OldChan}");
            await streamWriter.WriteLineAsync($"JOIN #{NewChan}");
        }

        public async Task Connect() {

            if (tcpClient.Connected) {
                tcpClient.Close();
            }
            
            Console.WriteLine($"Connecting to {configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchServer]}");
            await tcpClient.ConnectAsync(configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchServer], Convert.ToInt32(configBot.BotSystemParams[ConfigurationBot.SystemParams.TiwtchPort]));
            SslStream sslStream = new SslStream(
                tcpClient.GetStream(),
                false,
                ValidateServerCertificate,
                null
            );
            await sslStream.AuthenticateAsClientAsync(configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchServer]);
            streamReader = new StreamReader(sslStream);
            streamWriter = new StreamWriter(sslStream) {NewLine = "\r\n", AutoFlush = true};

            await streamWriter.WriteLineAsync($"PASS {configBot.OAuthPass}");
            await streamWriter.WriteLineAsync($"NICK {configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchUsername]}");
            await streamWriter.WriteLineAsync($"JOIN #{configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchChannel]}");
            await streamWriter.WriteLineAsync($"CAP REQ :twitch.tv/tags");

            Console.WriteLine($"JOINing channel #{configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchChannel]}");

            

            OnMessage += async (sender, twitchChatMessage) =>
            {
                if (twitchChatMessage.Message == "JOINED") {
                    Console.WriteLine($"JOIN complete. Bot fully connected to Twitch chat and reporting for duty.");
                    Console.Write("> ");
                    _currentChannel = twitchChatMessage.Channel;
                    MsgMgr msgHunt = huntBot.FollowHunt();
                    await SendCommandResponse(msgHunt, twitchChatMessage.Channel);
                    msgHunt = null;
                }
                else {
                    if (twitchChatMessage.HasOper && twitchChatMessage.HasCommand(configBot.BotSystemParams[ConfigurationBot.SystemParams.BotChatCommand]) && twitchChatMessage.HasParams()) {
                        MsgMgr msgResponse = new MsgMgr();

                        switch (twitchChatMessage.ParamAt(1)) {
                            case "hunt":
                                if (twitchChatMessage.ParamAt(2).Equals("set", StringComparison.CurrentCultureIgnoreCase)){
                                    msgResponse = huntBot.SetHunt(twitchChatMessage.TextFrom(3));
                                }
                                else if (twitchChatMessage.ParamAt(2).Equals("follow", StringComparison.CurrentCultureIgnoreCase)){
                                    msgResponse = huntBot.FollowHunt(twitchChatMessage.TextFrom(3));
                                }
                                else if (twitchChatMessage.ParamAt(2).Equals("rename", StringComparison.CurrentCultureIgnoreCase)){
                                    msgResponse = huntBot.RenameHunt(twitchChatMessage.TextFrom(3));
                                }
                                break;
                            case "setcount":
                                msgResponse = huntBot.SetCounter(twitchChatMessage.TextAt(2), false);
                                break;
                            case "list":
                                msgResponse = huntBot.GetLastNVillagers(twitchChatMessage.ParamAt(2));
                                break;
                            case "island":
                                msgResponse = huntBot.GetWhichIsland();
                                break;
                            case "describe":
                                msgResponse = huntBot.GetDescribe();
                                break;
                            case "rewind":
                                msgResponse = huntBot.DoRewind(twitchChatMessage.TextFrom(2));
                                break;
                            case "record":
                                msgResponse = huntBot.RecordVillager(twitchChatMessage.TextFrom(2), false);
                                break;
                            case "recordlast":
                                msgResponse = huntBot.RecordVillager(twitchChatMessage.TextFrom(2), true);
                                break;
                            case "seen":
                                msgResponse = huntBot.GetVillagerCountInHunt(twitchChatMessage.TextFrom(2));
                                break;
                            case "reportg":
                                msgResponse = huntBot.GetGuess(twitchChatMessage.ParamAt(2));
                                break;
                            case "bingo":
                                msgResponse = huntBot.HuntBingo(twitchChatMessage);
                                break;
                            case "ident":
                                msgResponse = huntBot.GetIdent();
                                break;
                            default:
                                msgResponse = huntBot.RecordVillager(twitchChatMessage.TextFrom(1), false);
                                break;
                        }

                        await SendCommandResponse(msgResponse, twitchChatMessage.Channel);
                        msgResponse = null;
                    }

                    if (twitchChatMessage.HasCommand(configBot.BotSystemParams[ConfigurationBot.SystemParams.BotRegisterGuessCommand]) && twitchChatMessage.HasParams()) {
                        huntBot.RecordGuess(twitchChatMessage.TextFrom(1), twitchChatMessage.Sender);
                    }
                }
            };

            Start(configBot.BotSystemParams[ConfigurationBot.SystemParams.TwitchChannel], configBot.MessagesText[ConfigurationBot.Messages.BotJoined]).SafeFireAndForget();
        }

        public async Task Start(string primaryJoinChannel, string channelJoinMsg)
        {
            try
            {
                while (true)
                {
                    string line = "";

                    try
                    {
                        if (tcpClient.Connected) {
                            line = await streamReader.ReadLineAsync();
                        }
                        
                    }
                    catch (Exception exGeneral) {
                        _connCount++;
                        Console.WriteLine($"Connection Error. Retrying {_connCount}/3. Msg: {exGeneral.Message}");
                        if (_connCount < 3) {
                            await Task.Delay(1500);
                            await Connect();
                            break;
                        }
                        else {
                            throw new Exception("Tried reconnecting three times and Twitch ain't having it...");
                        }
                    }

                    if (!string.IsNullOrEmpty(line)) {
                        if (line.IndexOf($"JOIN #{primaryJoinChannel}") > 0) {
                            await SendMessage(primaryJoinChannel, channelJoinMsg);
                            OnMessage(this, new TwitchChatMessage
                            {
                                Message = "JOINED",
                                Sender = null,
                                Channel = primaryJoinChannel,
                                HasOper = false,
                                MessageTokens = null
                            });
                        }
                        
                        string[] split = line.Split(" ");

                        if (line.StartsWith("PING"))
                        {
                            await streamWriter.WriteLineAsync($"PONG {split[1]}");
                        }

                        int iPrvMsgPos = line.IndexOf("PRIVMSG");
                        if (split.Length > 2 && iPrvMsgPos > 0)
                        {
                            bool bHasOper = false;
                            if (split[0].StartsWith("@")) {
                                if (split[0].Contains("badges=", StringComparison.CurrentCultureIgnoreCase) && (split[0].Contains("moderator/", StringComparison.CurrentCultureIgnoreCase) || split[0].Contains("broadcaster/", StringComparison.CurrentCultureIgnoreCase))) {
                                    bHasOper = true;
                                }
                            }

                            for(int i=0;i<split.Length;i++) {
                                if (split[i] == "PRIVMSG" && i > 0) {
                                    int iUser = i - 1;
                                    int iCh = i + 1;

                                    int exclamationPointPosition = split[iUser].IndexOf("!");
                                    string username = split[iUser].Substring(1, exclamationPointPosition - 1);
                                    string channel = split[iCh].TrimStart('#');
                                    
                                    int secondColonPosition = line.IndexOf(':', iPrvMsgPos + 3 + channel.Length);
                                    string message = line.Substring(secondColonPosition + 1);

                                    OnMessage(this, new TwitchChatMessage
                                    {
                                        Message = message.Trim(),
                                        Sender = username,
                                        Channel = channel,
                                        HasOper = bHasOper,
                                        MessageTokens = message.Trim().Split(" ")
                                    });

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Something unexpected went wrong: {ex.Message}");
            }
        }

        public async Task SendCommandResponse(MsgMgr mgrm, string channel)
        {
            if (mgrm.HasMessage == true) {
                await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{mgrm.Message}");
                if (!string.IsNullOrEmpty(mgrm.SecondMessage)) {
                    await Task.Delay(1500);
                    await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{mgrm.SecondMessage}");
                }
            }
        }

        public async Task SendMessage(string channel, string message)
        {
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
        }
    }
}