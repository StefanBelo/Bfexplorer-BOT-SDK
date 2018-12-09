# Betfair BOT SDK for Bfexplorer

Betfair BOT SDK is a set of .NET libraries you can use to develop betfair trading applications or betfair bots. It offers betfair api client and application domain and service used by bfexplorer:

http://bfexplorer.net/

In seven sample projects you can find following:

Using betfair api in C# or F# programming (sample code can be ported to other .NET languages as well). Two projects BetfairApiConsole and BetfairApiConsoleFSharp, show how to login/logout to betfair and how to load market catalogs.

BfexplorerServiceConsole project shows how to use bfexplorer application service to login/logout, and to retrieve market catalog data (10 currently open football match odds markets at in-play), retrieving first market and updating it every 5 seconds. 

BfexplorerService implements application domain and base service required to build a trading application like bfexplorer. For instance when you want to open a betfair market, just by using BetfairApi client you need to call following api methods: listMarketCatalogue and listMarketBook. BfexplorerService offers GetMarket or GetMarkets methods, so in one call you get everything necessary.

In MyBotTriggers project you can find more than 30 bot triggers to execute action bots, place bets, check football match score and so on.

If you want to integrate your bot with bfexplorer Bot Editor, then have a look on MyCsharpBot or MyFsharpBot project.

If you need to access bfexplorer data or automate user interface interaction, then use scripts from MyConsoleScripts project.

How to use it:

Install bfexplorer and login to betfair to activate your betfair api access. 

http://bfexplorer.net/Products/Bfexplorer

http://bfexplorer.net/Articles/Content/3#BfexplorerBotSDK
