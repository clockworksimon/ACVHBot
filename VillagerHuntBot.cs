using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACVillagerHuntBot
{
    public class VillagerHuntBot
    {
        public string Hunt { get; set; }
        public int HuntCounter { get; set; }
        public int HuntFoundIsland { get; set; }
        public bool IsHuntComplete {
            get {
                return HuntFoundIsland > 0;
            }
        }
        public bool IsHuntStarted {
            get {
                return !String.IsNullOrEmpty(Hunt) && HuntCounter > 0;
            }
        }

        private Task GuessFileSaving;
        private Task DataFileSaving;
        private ConfigurationBot ConfigBot;

        private Dictionary<int,string> GuessList = new Dictionary<int, string>();
        private Dictionary<string,long> CommandRates = new Dictionary<string, long>();
        private Dictionary<string,string> PerHuntData = new Dictionary<string, string>();
        private bool IsAllowedByRate(string command) {
            if (CommandRates.ContainsKey(command)) {
                long cmdTest = 240000000 + CommandRates[command];
                if (cmdTest < DateTime.Now.ToFileTime()) {
                    CommandRates[command] = DateTime.Now.ToFileTime();
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                CommandRates.Add(command, DateTime.Now.ToFileTime());
                return true;
            }
        }

        public VillagerHuntBot(ConfigurationBot PassedConfigBot) {
            ConfigBot = PassedConfigBot;
            Hunt = String.Empty;
            HuntCounter = 0;
            HuntFoundIsland = 0;
            Console.WriteLine(ConfigBot.MessagesText[ConfigurationBot.Messages.BotStarting]);
        }

        public string GetMessages(ConfigurationBot.Messages Msg) {
            string retVal = String.Empty;

            switch(Msg) {
                case ConfigurationBot.Messages.BeginHunt:
                    retVal = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.BeginHunt], Hunt, HuntCounter);
                    break;
                case ConfigurationBot.Messages.WillFindOnIslandAccordingTo:
                    if (GuessList.ContainsKey(HuntCounter)) {
                        retVal = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.WillFindOnIslandAccordingTo], Hunt, HuntCounter, GuessList[HuntCounter]);
                    }
                    break;
                case ConfigurationBot.Messages.OnTheWayTo:
                    retVal = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.OnTheWayTo], HuntCounter);
                    break;
            }

            return retVal;
        }

        public MsgMgr GetIdent() {
            MsgMgr retVal = new MsgMgr();

            retVal.Success = true;
            retVal.Message = String.Format("/me is running {0} v{1} | hunt status: {2} | for hunt: {3}", ConfigBot.AppDetails.ApplicationName, ConfigBot.AppDetails.ApplicaitonVersion, GetWhichIsland().Message.TrimStart("/me ".ToCharArray()), Hunt);

            return retVal;
        }

        public MsgMgr GetNextIslandMessaging(string ContinuingHuntDefaultText = "") {
            MsgMgr retVal = new MsgMgr();

            string strMRV = GetLastNVillagersFromFile(1);
            if (!string.IsNullOrEmpty(strMRV)) {
                string[] strMRVparts = strMRV.Split(".");
                if (strMRVparts.Length == 2) {
                    retVal.Success = true;
                    int iIslandTmp = 0;
                    if (string.IsNullOrEmpty(ContinuingHuntDefaultText)) {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.LeftHeadingTo], strMRVparts[1].Trim(), strMRVparts[0], HuntCounter);
                    }
                    else {
                        retVal.Message = ContinuingHuntDefaultText;
                    }
                    retVal.SecondMessage = GetMessages(ConfigurationBot.Messages.WillFindOnIslandAccordingTo);

                    //Override the above if the hunt already ended successfully
                    if (int.TryParse(strMRVparts[0], out iIslandTmp) && iIslandTmp >= HuntCounter) {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.FoundHuntEnded], strMRVparts[1].Trim(), strMRVparts[0]);
                        if (GuessList.ContainsKey(HuntCounter)) {
                            retVal.SecondMessage = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.FinalGuess], GuessList[HuntCounter]);
                        }
                    }
                }
            }

            return retVal;
        }

        public MsgMgr RenameHunt(string NewHunt) {
            MsgMgr retVal = new MsgMgr();

            retVal.Success = ConfigBot.RenameHunt(Hunt, NewHunt);
            if (retVal.Success == true) {
                retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.RenameHuntSuccess], Hunt, NewHunt);
                Hunt = NewHunt;
            }
            else {
                retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.RenameHuntFail], Hunt, NewHunt);
            }

            return retVal;
        }
       
        public MsgMgr FollowHunt(string hunt = "") {
            MsgMgr retVal = new MsgMgr();

            if (String.IsNullOrEmpty(hunt)) {
                if (!String.IsNullOrEmpty(ConfigBot.BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt]) && !ConfigBot.BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt].Equals("+off", StringComparison.CurrentCultureIgnoreCase)) {
                    retVal = SetHunt(ConfigBot.BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt]);
                }
            }
            else {
                if (!hunt.Equals("+off", StringComparison.CurrentCultureIgnoreCase) || !ConfigBot.BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt].Equals(hunt, StringComparison.CurrentCultureIgnoreCase)) {
                    retVal = SetHunt(hunt);
                }

                if (retVal.Success || hunt.Equals("+off", StringComparison.CurrentCultureIgnoreCase)) {
                    ConfigBot.BotSystemParams[ConfigurationBot.SystemParams.BotFollowHunt] = hunt;
                    ConfigBot.SaveSystemParams();
                }
            }

            return retVal;
        }

        public MsgMgr SetHunt(string hunt) {
            MsgMgr retVal = new MsgMgr();

            bool bAllowed = false;
            bAllowed = IsAllowedByRate("SetHunt");

            if (Hunt != null && !Hunt.Equals(hunt, StringComparison.CurrentCultureIgnoreCase)) {
                bAllowed = true;
            }

            if (bAllowed) {
                retVal.Success = true;

                HuntFoundIsland = 0;
                HuntCounter = 1;
                Hunt = hunt;

                (GuessList, HuntCounter, PerHuntData) = ConfigBot.BeginHunt(hunt);

                string strMRV = GetLastNVillagersFromFile(1);
                if (!string.IsNullOrEmpty(strMRV)) {
                    string[] strMRVparts = strMRV.Split(".");
                    if (strMRVparts.Length == 2) {
                        int iIslandTmp = 0;
                        if (int.TryParse(strMRVparts[0], out iIslandTmp) && iIslandTmp >= HuntCounter) {
                            HuntFoundIsland = iIslandTmp;
                        }
                    }
                }

                //Set Default Message
                retVal.Message = GetMessages(ConfigurationBot.Messages.BeginHunt);

                //Set Next Island Message (Overriding the start)
                MsgMgr msgNextIsland = GetNextIslandMessaging(retVal.Message);
                if (msgNextIsland.Success == true) {
                    retVal.Message = msgNextIsland.Message;
                    retVal.SecondMessage = msgNextIsland.SecondMessage;
                }
                else {
                    //Check edge case where someone guessed island 1 before the hunt began
                    retVal.SecondMessage = GetMessages(ConfigurationBot.Messages.WillFindOnIslandAccordingTo);
                }
            }
            
            return retVal;
        }

        public MsgMgr GetWhichIsland() {
            MsgMgr retVal = new MsgMgr();

            if (IsHuntStarted) {
                retVal = GetNextIslandMessaging();
                if (retVal.Success == false) {
                    retVal.Success = true;
                    retVal.Message = GetMessages(ConfigurationBot.Messages.OnTheWayTo);
                    //Check edge case where someone guessed island 1 before the hunt began
                    retVal.SecondMessage = GetMessages(ConfigurationBot.Messages.WillFindOnIslandAccordingTo);
                }
            }
            else {
                retVal.Message = ConfigBot.MessagesText[ConfigurationBot.Messages.UndefinedHunt];
            }

            return retVal;
        }

        public MsgMgr SetCounter(string CounterVal, bool SkipMsg = true, bool IgnoreRate = false) {
            MsgMgr retVal = new MsgMgr();

            if (IsAllowedByRate("SetCounter") || IgnoreRate == true) {
                int iNewCounter = 0;
                if (int.TryParse(CounterVal, out iNewCounter) && iNewCounter > 0) {
                    HuntCounter = iNewCounter;
                    ConfigBot.SaveHuntCounter(HuntCounter);
                    retVal.Success = true;
                    if (SkipMsg == false) {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.IslandCountSetTo], HuntCounter);
                    }
                }
            }

            return retVal;
        }

        public MsgMgr RecordVillager(string VillagerName, bool bForceEndHunt) {
            MsgMgr retVal = new MsgMgr();

            if (IsAllowedByRate("RecordVillager")) {
                if (IsHuntComplete) {
                    retVal = GetNextIslandMessaging(); //Since we know the hunt is over, getting the messaging for the next island will return the "congrats" messaging
                } else {
                    if (IsHuntStarted) {
                        int oldI = HuntCounter;
                        string strRawVillagerName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(VillagerName);
                        string strVillagerName = String.Format("{0}. {1}", HuntCounter, strRawVillagerName);
                        if (ConfigBot.SaveVillagerList(strVillagerName)) {
                            if (strRawVillagerName.Equals(Hunt, StringComparison.CurrentCultureIgnoreCase) || bForceEndHunt == true) {
                                retVal.Success = true;
                                retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.FoundOnCongrats], strRawVillagerName, oldI);
                                if (GuessList.ContainsKey(oldI)) {
                                    retVal.SecondMessage = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.FinalGuess], GuessList[oldI]);
                                }
                                else {
                                    retVal.SecondMessage = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.NoGuessForIsland], oldI);
                                }
                                HuntFoundIsland = oldI;
                            }
                            else {
                                HuntCounter++;
                                SetCounter(HuntCounter.ToString());
                                
                                retVal.Success = true;
                                retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.LeftHeadingTo], strRawVillagerName, oldI, HuntCounter);
                                retVal.SecondMessage = GetMessages(ConfigurationBot.Messages.WillFindOnIslandAccordingTo);
                            }
                        }
                    }
                    else {
                        retVal.Message = ConfigBot.MessagesText[ConfigurationBot.Messages.UndefinedHunt];
                    }
                }
            }

            return retVal;
        }

        public MsgMgr RecordGuess(string Guess, string User) {
            MsgMgr retVal = new MsgMgr();

            if (IsHuntComplete) {
                if (IsAllowedByRate("GuessReportWhenHuntComplete")) {
                    retVal.Success = true;
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.GuessNotRecordedBecauseHuntComplete], Hunt, HuntFoundIsland);
                }
            } else {
                int iGuess = 0;
                if (!string.IsNullOrEmpty(Guess) && int.TryParse(Guess, out iGuess)) {
                    if (GuessList.ContainsKey(iGuess)) {
                        if (!PersonGuessedNumberAlready(User, iGuess)) {
                            GuessList[iGuess] += $", {User}";
                        }
                    }
                    else {
                        GuessList.Add(iGuess, User);
                    }
                    SaveGuesses();
                }
                else {
                    if (!string.IsNullOrEmpty(Guess) && Guess.IndexOf(",") > 0) {
                        string[] guesses = Guess.Trim().Split(",", StringSplitOptions.TrimEntries);
                        for(int i=0; i < guesses.Length; i++) {
                            if (!string.IsNullOrEmpty(guesses[i]) && int.TryParse(guesses[i], out iGuess)) {
                                if (GuessList.ContainsKey(iGuess)) {
                                    if (!PersonGuessedNumberAlready(User, iGuess)) {
                                        GuessList[iGuess] += $", {User}";
                                    }
                                }
                                else {
                                    GuessList.Add(iGuess, User);
                                }
                                SaveGuesses();
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        public MsgMgr GetGuess(string Guess) {
            MsgMgr retVal = new MsgMgr();
            int iGuess = 0;

            if (string.IsNullOrEmpty(Guess)) {
                if (GuessList.ContainsKey(HuntCounter)) {
                    retVal.Success = true;
                    retVal.Message = GetMessages(ConfigurationBot.Messages.WillFindOnIslandAccordingTo);
                }
                else {
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.NoGuessForIsland], HuntCounter);;
                    retVal.SecondMessage = GetNextBiggestGuess(HuntCounter);
                }
            }
            else {
                if (int.TryParse(Guess, out iGuess)) {
                    if (GuessList.ContainsKey(iGuess)) {
                        retVal.Success = true;
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.WillFindOnIslandAccordingTo], Hunt, iGuess, GuessList[iGuess]);
                    }
                    else {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.NoGuessForIsland], iGuess);
                        retVal.SecondMessage = GetNextBiggestGuess(iGuess);
                    }
                }
                else {
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.GuessNotFound], Guess);

                    string strUGuesses = String.Empty;
                    string lazyTest = String.Empty;

                    foreach(int iGuessKey in GuessList.Keys.OrderBy(i => i)) {
                        if (PersonGuessedNumberAlready(Guess, iGuessKey)) {
                            strUGuesses += String.Format("{0}, ", iGuessKey);
                        }
                    }
                    strUGuesses = strUGuesses.Trim().TrimEnd(',');

                    if (!string.IsNullOrEmpty(strUGuesses)) {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.RecordedTheseGuesses], Guess, strUGuesses);
                    }
                }
            }

            return retVal;
        }

        internal bool PersonGuessedNumberAlready(string Person, int Island) {
            bool retVal = false;
            if (GuessList.ContainsKey(Island)) {
                List<string> strTestList = new List<string>(GuessList[Island].Split(',', StringSplitOptions.TrimEntries));
                foreach(string strListPerson in strTestList) {
                    if (!string.IsNullOrEmpty(strListPerson) && strListPerson.Equals(Person, StringComparison.CurrentCultureIgnoreCase)) {
                        retVal = true;
                        break;
                    }
                }
            }
            return retVal;
        }

        internal string GetNextBiggestGuess(int iGuess) {
            string strRetVal = String.Empty;

            List<int> guesskeys = new List<int>(GuessList.Keys);
            guesskeys.Sort();
            int kIndex = guesskeys.BinarySearch(iGuess);
            kIndex = kIndex >= 0 ? kIndex : ~kIndex;

            if (kIndex < guesskeys.Count && kIndex >= 0) {
                int iGkey = guesskeys[kIndex];
                strRetVal = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.AltWillFindOnIslandAccordingTo], Hunt, iGkey, GuessList[iGkey]);
            }

            return strRetVal;
        }

        public MsgMgr GetLastNVillagers(string NumVillagers) {
            MsgMgr retVal = new MsgMgr();

            if (string.IsNullOrEmpty(NumVillagers)) {
                NumVillagers = "5";
            }

            int iNumVillagers = -1;
            if (int.TryParse(NumVillagers, out iNumVillagers)) {
                if (iNumVillagers < 1) {
                    iNumVillagers = 5;
                }

                string strText = GetLastNVillagersFromFile(iNumVillagers);
                if (!String.IsNullOrEmpty(strText)) {
                    retVal.Success = true;
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.ListOfLastSeenVillagers], strText);
                }
            }

            return retVal;
        }

        public MsgMgr HuntBingo(TwitchBot.TwitchChatMessage chatMsg) {
            MsgMgr retVal = new MsgMgr();

            string strPerson = chatMsg.TextAt(2).TrimStart('@');
            string strIsland = chatMsg.ParamAt(3);
            int iIsland = 0;
            string strHuntBingoKey = ConfigBot.GetStorageName(ConfigurationBot.HuntStorage.Bingo);

            if (!string.IsNullOrEmpty(strPerson) && !string.IsNullOrEmpty(strIsland) && int.TryParse(strIsland, out iIsland)) {
                PerHuntData[strHuntBingoKey] = String.Format("{0},{1}", strPerson, iIsland);
                SaveData();
            }
            else if (!string.IsNullOrEmpty(strPerson)) {
                PerHuntData[strHuntBingoKey] = String.Format("{0},{1}", strPerson, HuntCounter);
                SaveData();
            }

            if (PerHuntData.ContainsKey(strHuntBingoKey) && !string.IsNullOrEmpty(PerHuntData[strHuntBingoKey])) {
                string[] strDisplay = PerHuntData[strHuntBingoKey].Split(',');
                if (strDisplay.Length == 2) {
                    retVal.Success = true;
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.BingoLastCalled], strDisplay[0], strDisplay[1]);
                }
            }

            return retVal;
        }

        public MsgMgr GetDescribe() {
            MsgMgr retVal = new MsgMgr();
            retVal.Success = true;
            retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.DescribeBot], Hunt);
            return retVal;
        }

        public void SaveGuesses() {
            if (GuessFileSaving == null) {
                GuessFileSaving = SaveGuessesAsync();
            }
        }

        internal async Task SaveGuessesAsync() {
            await Task.Delay(5000);
            ConfigBot.SaveGuessList(System.Text.Json.JsonSerializer.Serialize(GuessList));
            GuessFileSaving = null;
        }

        public void SaveData() {
            if (DataFileSaving == null) {
                DataFileSaving = SaveDataAsync();
            }
        }

        internal async Task SaveDataAsync() {
            await Task.Delay(5000);
            ConfigBot.SaveDataList(System.Text.Json.JsonSerializer.Serialize(PerHuntData));
            DataFileSaving = null;
        }

        public MsgMgr DoRewind(string RewindText) {
            MsgMgr retVal = new MsgMgr();

            if (!IsHuntComplete) {
                HuntCounter--;
            }
            
            retVal.Success = ConfigBot.RewindVillagerList(HuntCounter, RewindText);
            if (!string.IsNullOrEmpty(RewindText)) {
                HuntCounter++;
            }

            SetCounter(HuntCounter.ToString(), true, true);

            string strMRV = GetLastNVillagersFromFile(1);
            if (!string.IsNullOrEmpty(strMRV)) {
                string[] strMRVparts = strMRV.Split(".");
                if (strMRVparts.Length == 2) {
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.RewindHuntWithReplacement], strMRVparts[1].Trim(), strMRVparts[0], HuntCounter);
                }
            }
            else {
                retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.RewindHunt], HuntCounter);
            }

            return retVal;
        }

        public MsgMgr GetVillagerCountInHunt(string VillagerToFind) {
            MsgMgr retVal = new MsgMgr();

            if (!string.IsNullOrEmpty(VillagerToFind)) {
                var SeenIn = ConfigBot.GetCountOfVillagerSeenInHunt(VillagerToFind);
                if (SeenIn.VisitedCounter > 0) {
                    retVal.Success = true;
                    SeenIn.VisitedIslands = SeenIn.VisitedIslands.Trim().TrimEnd(',');
                    int iIslandNum = 0;
                    bool isNumber = int.TryParse(VillagerToFind, out iIslandNum);
                    if (isNumber == false) {
                        retVal.Message = String.Format(ConfigBot.GetMessageTextWithCorrectPlural(ConfigurationBot.Messages.TimesSeenInHunt, SeenIn.VisitedCounter), VillagerToFind, SeenIn.VisitedCounter, SeenIn.VisitedIslands, Hunt);
                    }
                    else {
                        retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.VillagerSeenOnIsland], SeenIn.VisitedIslands, VillagerToFind, Hunt);
                    }
                }
                else {
                    retVal.Message = String.Format(ConfigBot.MessagesText[ConfigurationBot.Messages.NotSeenInHunt], VillagerToFind, Hunt);
                }
            }

            return retVal;
        }

        internal string GetLastNVillagersFromFile(int iNumVillagers) {
            return ConfigBot.GetLastNVillagersFromFile(iNumVillagers);
        }
    }
}