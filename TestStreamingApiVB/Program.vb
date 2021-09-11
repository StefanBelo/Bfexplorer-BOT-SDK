Imports System.ComponentModel
Imports BeloSoft.Betfair.StreamingAPI
Imports BeloSoft.Betfair.StreamingAPI.Models
Imports BeloSoft.Bfexplorer
Imports BeloSoft.Bfexplorer.Domain
Imports BeloSoft.Bfexplorer.Service
Imports Microsoft.FSharp.Collections

Module Program
    Private Function GetUserNameAndPassword() As (UserName As String, Password As String)?
        Dim entryParameters = Command.Split(" ")

        If entryParameters.Length = 2 Then
            Return (entryParameters(0), entryParameters(1))
        Else
            Return Nothing
        End If
    End Function

    Public Sub OnMarketPropertyChanged(sender As Object, args As PropertyChangedEventArgs)
        If args.PropertyName = "TotalMatched" Then
            Dim market = CType(sender, Market)

            Console.WriteLine($"{market}: {market.TotalMatched:N2}")
        End If
    End Sub

    Public Sub OnMarketsOpened(sender As Object, markets As FSharpList(Of Market))
        For Each market In markets
            Console.WriteLine($"{market}")
            AddHandler CType(market, INotifyPropertyChanged).PropertyChanged, AddressOf OnMarketPropertyChanged
        Next
    End Sub

    Async Function Execute() As Task
        Dim result = GetUserNameAndPassword()

        If Not result.HasValue Then
            Throw New Exception("Please enter your betfair user name and password!")
        End If

        Dim bfexplorerService = New BfexplorerService(initializeBotManager:=False) With {
            .UiApplication = New BfexplorerHost()
        }

        Dim loginResult = Await bfexplorerService.Login(result?.UserName, result?.Password).ExecuteAsyncTask()

        If loginResult.IsSuccess Then
            Dim marketUpdateService = New MarketUpdateService(bfexplorerService, StreamingData.MarketDataFilterForPassiveMarkets)

            AddHandler marketUpdateService.OnMarketsOpened, AddressOf OnMarketsOpened

            Dim startResult = Await marketUpdateService.Start().ExecuteAsyncTask()

            If startResult.IsSuccessResult Then
                ' Horse Racing
                Dim filter = ListModule.OfArray(New BetEventFilterParameter() {
                        BetEventFilterParameter.NewBetEventTypeIds(New Integer() {7}),
                        BetEventFilterParameter.NewMarketTypeCodes(New String() {"WIN"}),
                        BetEventFilterParameter.NewCountries(New String() {"GB"})
                    })

                Dim subscribeResult = Await marketUpdateService.Subscribe(filter).ExecuteAsyncTask()

                Dim message As String

                If subscribeResult.IsSuccessResult Then
                    message = If(marketUpdateService.ConnectionHasSuccessStatus, "Successfully subscribed.", $"Failed to subscribe: {marketUpdateService.ErrorMessage}")
                Else
                    message = subscribeResult.FailureMessage
                End If

                Console.WriteLine($"{message}\nPress any key to exit.")
                Console.ReadKey()
            End If

            Await bfexplorerService.Logout().ExecuteAsyncTask()
        End If
    End Function

    Sub Main()
        Execute().Wait()
    End Sub
End Module