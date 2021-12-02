# ACVHBot
Are you a Twitch streamer, or a mod for a streamer, who does a lot of villager hunts on stream? Do you like to track those hunts and have your community guess the island you'll find the villager on, play villager bingo, keep track in your Discord of villagers you've found, and so on? If you do, you've probably already discovered the inconvenience of trying to maintain the island counter consistently, of making sure every villager gets tracked, keeping lists updated, and answering chat on who you saw three islands ago. It can get messy fast and is distracting for the streamer and the mods.

What if there was a custom-built bot designed to solve for these exact problems, while also making it super easy to see which island you found a villager on and how many times you've seen a villager during the hunt. What if that bot also allowed chat to register their island guesses and automatically announced who guessed on which island as the counter updated? What if that bot generated a text file of the island counter, that the streamer could add as a source in OBS, so that they never had to manually update it ever again?

ACVHBot is this bot! I built it to solve all these problems while modding for a streamer who's done multiple island hunts from 600 tickets to over 1,100 tickets. The feature set has evolved from everything we discovered we needed along the way, and has been tried and tested over many hunts.

How does this bot work? In brief: you, the streamer, download the app and install it on your PC and then your mods (or you!) control it via Twitch chat commands. Tracking the villagers is done by the mods (or you) and your chat can register their guesses with a simple command as well. It's best if the streamer installs the bot, so that they have access to the counter text file, but a mod can also own and operate the bot and all the functions aside from this will still work.

## Getting Started
1. In order to run the app on your PC you'll need to install it. Since I'm an unknown developer without company backing, there's a few things you should know before clicking the download link:
 - When you download the installer your browser may let you know that it isn't commonly downloaded and could be a security risk. You'll need to choose to keep/accept the download and then run it to launch the installer.
 -  When you run the installer, Windows will let you know that the app is unrecognized. You may have to click More Info and then Run Anyway or otherwise accept a Windows popup to run the installer.
 -  If I was popular and willing to spend $700 - $1,200 annually on a security certificate, I might be able to solve for these problems, but sadly both are outside my reach.
 -  The **ACVHBot Installer can be downloaded** from the Releases section in the sidebar.
 -  The bot runs on Microsoft's .NET technology. If you haven't run other .NET apps on your PC before, the bot may stop with a fatal error on launch. If this happens, install .NET 5.0 from Microsoft: https://dotnet.microsoft.com/download/dotnet 
 -  Click on the .NET 5.0 option on that page, and then scroll down to .NET Runtime 5 on the right side, and you'll most likely want the x64 or x86 download. If you're not sure which one, try the x64 download first. Close the bot's window and reopen it to relaunch it after installing .NET 5.
2. When you first successfully launch the bot, it'll invite you to open the config. You'll need to set some basic options, such as the user the bot will log into Twitch as and the channel it should join on login. The config will open in a web browser. It's connected to a temporary server on your PC that the bot opens for the purpose of easy configuration and then closes again. Follow the instructions on screen and remember to click the save or cancel buttons as appropriate.
3. After that basic setup is complete, the bot will be present in the chosen Twitch channel and you'll see it report for duty. If you open that Twitch stream yourself as a mod or the streamer, you can type !vh ident in chat to see if the bot responds.
4. Continue reading below for instructions on how to use the bot.

## First Steps Before Your First Hunt
+ Since the bot runs on your PC, you'll need to launch it whenever you're going villager hunting and make sure it reports for duty in your stream chat.
+ To begin your first hunt, you or a moderator would issue this command in your stream's chat: !vh hunt follow Villager Name
++ *Note: you only need to do this once per villager hunt. The bot will automatically load into the last followed hunt each time you launch it.*
+ If you, the streamer, are running the bot on your PC, you would then go into the bot's console window (the black box that opens when you launch the bot) type path at the prompt and press enter. This will show you the path to the island counter text file.
+ With the above information you can add a Text (GDI) element to OBS. Check the box Read from file on the Text source and then use the browse button to pick the file mentioned in the path console command. You can customize the font/size/etc to your preference in OBS.
++ *Note: you only need to do this once per villager hunt. Since the counter file is named for the hunt, each time you begin a new hunt for a new villager, you'll need to change the counter source after the hunt follow command is issued.*

With the above steps completed, you're ready to easily track this hunt!

## The Hunt Begins
Now that you've got those once-per-hunt items out of the way, to track your journey through the hunt and use the bot to its full advantage, here's some recommended ways to use it:

+ Register your guess: Anyone in chat can issue the !myguess number command (or !myguess number,number,number for multiples) to guess where they think you'll find the villager. Examples: !myguess 69 or !myguess 69,169,420
++ *Note: the bot does not give any chat feedback when guesses are recorded, to prevent excessive spam.*
+ Record the villager: When you arrive on an island and discover the villager, your mods (or you) can say !vh Villager Name in stream chat and the bot will record the name and advance the counter. Example: !vh Sherb
++ *Note: the !vh Villager Name command has a 30 second internal cooldown to prevent accidental multiple entry if several mods try to record the villager at the same time.*
+ Fix mistakes: If you make a mistake adding a villager you can fix it with the !vh rewind command: The command on its own will delete the last villager entry and roll the counter back by one. !vh rewind Villager Name will replace the last entry in the file with the new villager name, useful for those blundered typos! Note that if mistakes are spotted several islands later, there's currently no command to replace a villager name at a specific island number.

These are the main ways to get information into the bot, but what about reporting back on various stats? I'm glad you asked...

## Questions About the Hunt Answered Easily
The bot has a number of commands to make answering questions super easy. To cut down on chat spam and ensure you're able to maintain control, only mods (or the you, the streamer) can issue these commands:

+ !vh list will show you the five most recently visited islands along with the villagers on those islands. Add a number to it, !vh list 15 for example, and it will report the most recent 15 islands with the villager names. This is useful for when folks ask about previous villagers, and for updating Discord, since you can copy and paste the report from stream chat into the relevant channel.
+ !vh seen with a villager name or island number will report which villager was on which island, or which islands that villager was seen on. For example !vh seen Octavian or !vh seen 7.
+ !vh island will report the current island number and any registered guesses for the island.
+ !vh reportg will report registered guesses for the current island. Add a name or number and it will report guesses for that specific island or chat user. For example !vh reportg 17 or !vh reportg zhyv0n.
+ !vh bingo helps you track bingo. Use !vh bingo username number to set who last won bingo and when, and use !vh bingo to report this information in chat.

By now you have the bulk of the commands explained, the most frequently used ones during your typical hunt. To further clarify how best to use the bot, here's some more advanced situations to consider:

## Every Hunt After the First
Hunting for another villager: so you hunted for and found Antonio? Congratulations! Now you've kicked out another villager and you're on the hunt for Nan. When you launch the bot, it will be following the last hunt so it'll congratulate you for finding Antonio. To begin the hunt for Nan, you would issue the !vh hunt follow Nan command, which will switch the bot over to a new hunt for Nan. Note that none of the previous data is lost, it's still there in the Antonio hunt files on your PC.

Hunting for the same villager again: it's six months down the line and you've kicked out Antonio and now you miss him and decide to hunt for him again. When you issue !vh hunt follow Antonio, le gasp, the bot remembers your old hunt and congratulates you for finding him! In this case, all you need to do is issue !vh hunt rename Antonio Old Hunt 1 and then !vh hunt follow Antonio again. This will rename all the data files for the first hunt to "Antonio Old Hunt 1" and then when you follow an Antonio hunt again, it'll be starting over from fresh.

Closing out the hunt correctly: If you're hunting for one villager, and you named the hunt for that villager, then the bot will automatically complete the hunt when that villager is found with !vh. For example: you begin a hunt for Antonio with !vh hunt follow Antonio. 157 islands in, there he is and a mod issues !vh Antonio and there is much rejoicing. The bot puts the hunt into a mode where it knows the hunt has ended. But let's say you were hunting for Kiki or Rodeo and started your hunt with !vh hunt follow Kiki or Rodeo. When you find one or the other of those villagers the bot won't know to automatically end the hunt. In that case you or your mods should record the last villager found with the !vh recordlast Villager Name command to let the bot know that it's time to end the hunt.

## All the Commands
The bot has several other features that come in useful in certain niche situations. Here is the complete Twitch chat command list for your reference:

### Mod Commands
Everything prefixed with !vh is mod only, of course the streamer can issue them as well, but chat at large cannot:

+ !vh hunt follow villager name -- start a hunt for the named villager. Note that you can technically put any text here, but it's best to use the villager name to make things make the most sense.
+ !vh hunt rename new name -- takes the currently active hunt and renames it. Note: the streamer will need to change the counter text file source in OBS after this command is used.
+ !vh list or !vh list number  -- show the most recent villagers. If num is left off then it defaults to 5. For example !vh list for the last 5 seen or !vh list 10 for the last 10. This command isn't capped but Twitch will truncate long strings so don't request too far back (no more than 20 or 25 probably)
+ !vh seen villager name or !vh seen island number -- shows the islands that a villager has been found on during this hunt (e.g. !vh seen Octavian) or shows the villager found on a specific island (e.g. !vh seen 69)
+ !vh island -- reports back on what the current island counter is. Additionally, if there are any guesses for this island number, it'll report those at the same time
+ !vh reportg -- reports any user guesses for the current island number
+ !vh reportg username or !vh reportg island number  -- reports any user guesses for the island number, e.g. !vh reportg 469 to see if anyone guessed on that num. Also !vh reportg zhyv0n to see what guesses zhyv0n entered.
+ !vh Villager Name  -- records a villager sighting and automatically advances the island counter. Example: !vh Vic
+ !vh recordlast Villager Name  -- records the last villager in the hunt. If we're searching for a single villager then this isn't necessary since the bot will match the name from a regular !vh addition to the hunt and end automatically. but if the search is for multiple possibilities (or the hunt name isn't a single villager name) then using this command will tell the bot the hunt is over/won.
+ !vh rewind or !vh rewind Villager Name -- to fix a mistake on the last island. !vh rewind will delete the last entry and roll the counter back one, while !vh rewind name will replace the last recorded villager with the new name.
+ !vh bingo username island  -- tell the bot who last called bingo and on which island they found their last villager, e.g. !vh bingo zhyv0n 127
+ !vh bingo -- Reports the bingo info set with !vh bingo username island back into chat
+ !vh describe -- outputs some basic instructions for general usage
+ !vh ident -- outputs some info about the bot (version number and any active hunt)

### General Commands
All of chat can use these commands:

+ !myguess number or !myguess number, number, number, etc -- this command allows everyone in chat to log their guess with the bot. For example, !myguess 69 or !myguess 69,169,269,369

At this time there aren't any other general chat commands. Almost all of the features are behind mod and streamer permissions to keep things organized.

## Attribution
A long time ago in a now-obsolete programming language I wrote an IRC bot. Twitch chat is based on the IRC protocol, and when I started to write this bot, I didn't want to revisit all my old work and redo it, so I found some very basic Twitch/IRC connection code, in the form of the SimpleTwitchBot by Bradley Saunders: https://medium.com/swlh/writing-a-twitch-bot-from-scratch-in-c-f59d9fed10f3

I've expanded and rewritten a lot of that code, but Bradley's article got me quickstarted in the beginning. I've also used community resources to ensure the most efficient way of doing things, such as:

Count File Lines by Nima Ara: https://www.nimaara.com/counting-lines-of-a-text-file/
The more robust file handling discussion by various contributors: https://stackoverflow.com/questions/4264117/how-to-delete-last-line-in-a-text-file
In the spirit of community, the full source code to the bot is available in this GitHub project.

## FAQ and Technical Details
Q: Sometimes the bot crashes on startup with an error "Unable to read data from the transport connection" or similar wording. A: This is an unfortunate side effect of how Twitch chat works. I've tried to include error handling to smooth this over but it's a hard problem to solve because of its intermittent nature. It's something I keep working on, but just know that since it does only happen occasionally, closing the bot and opening it again usually works.

Q: The bot quits with a Framework error. A: The bot is written in the C# .NET programming language using VS Code / .NET Core / Microsoft .NET technology. Microsoft offers .NET Framework installation instructions for Windows 10 over here, and for older versions of Windows over here. You can also typically find .NET as an optional component if you open Windows Update in your Windows settings.

Q: Why is the bot not available on Mac? A: I don't own a Mac and therefore can't test and build the bot for MacOS. I also don't know anything about developing on that OS and wouldn't feel comfortable claiming to have produced an app for a platform that I don't use and am not familiar with.

Q: Can I get this or that feature added? A: Maybe. For the most part the bot works "as is" because it's something I wrote to solve a specific problem and I don't want to promise to make it all things to all people. That said, if you suggest a feature and it makes sense to me and isn't hugely difficult to implement, it could happen!

If you read all the way to the end, thank you for being here. I hope you have many happy and far less stressful villager hunts. May you find the villager you seek in a reasonable number of Nook Miles Tickets.
