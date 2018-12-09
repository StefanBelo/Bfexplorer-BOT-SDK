# JayBeeBot
 
This project shows how to use BfexplorerService in very simply desktop application programmed in C# and Windows Forms as user interface library.

There are presented three main features every betfair developer must manage:

1)	Login/logout and keeping login session alive

2)	Browsing events, loading markets data and updating them (prices, bets, and so on) 

3)	Implementing base bots

The application loads horse racing win markets and allows monitoring currently selected market/race in 200ms refresh interval. 

In advance bot application you can create different mechanism for monitoring a lot of markets in different refresh interval. 

You can use following methods BfexplorerService offers: GetMarkets, GetMarketsWithActiveBets, UpdateMarket, UpdateMarketBaseData, UpdateMarketData, UpdateMarkets, UpdateMarketStatus, UpdateMarketStatuses. 
All of those methods are used in my trading application:

http://bfexplorer.net/Products/Bfexplorer

Depending on bot type (for what purpose was implemented), betfair bot can place bets on more market selections or just operate on one market selection. You can execute as many bots on market as you want, or your strategy requires. 

You can simply put together different bots without programming just by setting bots entry parameters, watch this video to see how to construct spoofing bot without programming:

https://www.youtube.com/watch?v=QmS0i-7sRb0

In this simple application there is not implemented mechanism for saving and setting up different bot settings. I added just list of bot settings you can change and execute selected bot with its current settings. All these bots have default settings so you can just execute a bot and see what bets it will place. I added only bots which in this app context make sense.

When programming your own betting or trading strategy, you can use these bots just for base operations, your strategy will define time, selection, bet type and bot type (and so on) you want to execute.

By default JayBeeBot app is set to work in practice mode, so no bets are placed at betfair.

When you look on code, the code in Helpers and Models folders is just plumbing code not required when using BfexplorerService in app using WPF user interface library, also any async methods as my BOT SDK is programmed in F#, requires using of some wrappers code to interact with other .NET languages. If I programmed this simple app in F# it would be done by maybe 60% of code. You can see differences in other similar projects, just compare MyCsharpBot and MyFsharpBot.
