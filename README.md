# Betfair BOT SDK for Bfexplorer

Betfair BOT SDK is a set of .NET libraries you can use to develop betfair trading applications or betfair bots. It offers betfair api client and application domain and service used by bfexplorer:

http://bfexplorer.net/

In seven sample projects you can find following:

Using betfair api in C#, VB or F# programming languges (sample code can be ported to other .NET languages as well). Two projects BetfairApiConsole and BetfairApiConsoleFSharp, show how to login/logout to betfair and how to load market catalogs.

BfexplorerServiceConsole project shows how to use bfexplorer application service to login/logout, and to retrieve market catalog data (10 currently open football match odds markets at in-play), retrieving first market and updating it every 5 seconds. 

BfexplorerService implements application domain and base service required to build a trading application like bfexplorer. For instance when you want to open a betfair market, just by using BetfairApi client you need to call following api methods: listMarketCatalogue and listMarketBook. BfexplorerService offers GetMarket or GetMarkets methods, so in one call you get everything necessary.

In MyBotTriggers project you can find more than 30 bot triggers to execute action bots, place bets, check football match score and so on.

If you want to integrate your bot with bfexplorer Bot Editor, then have a look on MyCsharpBot or MyFsharpBot project.

If you need to access bfexplorer data or automate user interface interaction, then use scripts from MyConsoleScripts project.

How to use it:

Install bfexplorer and login to betfair to activate your betfair api access. 

http://bfexplorer.net/Products/Bfexplorer

http://bfexplorer.net/Articles/Content/3#BfexplorerBotSDK

# Betfair Tutorials

4 projects showing how to use betfair rest api and betfair streaming api in following .net programming languages: F#, C# and Visual Basic.

https://github.com/StefanBelo/Bfexpl...stStreamingAPI

https://github.com/StefanBelo/Bfexpl...amingApiCSharp

https://github.com/StefanBelo/Bfexpl...StreamingApiVB

https://github.com/StefanBelo/Bfexpl...er/TestRestAPI

What is advantage of this framework to other ones?

There is one code base betfair framework which can be used by different programming languages just utilizing the fact that code is written in .net programming language.

There is all already implemented client code to consume betfair rest api and/or streaming api, framework to execute strategies and monitor markets and your bets. There are already implemented base strategies to place bets on set conditions, trading strategies, and so on.

All you need to do is to add your own code in your favorite programming langue (C#, Visual Basic or F#) just to trigger any prebuilt strategy.

There are implemented many different data providers including base score data providers for football/soccer or tennis matches.

If you have any questions, or just want to activate betfair api access for your framework test, you can join to slack group:

https://join.slack.com/t/bfexplorer/...mbV7PNbtz3iqDg