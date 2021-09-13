# Using IronPython3
# https://github.com/IronLanguages/ironpython3/releases/tag/v3.4.0-alpha1

import clr
import sys

sys.path.append(r"C:\Program Files (x86)\BeloSoft\Bfexplorer")

clr.AddReference("FSharp.Core")

clr.AddReference("BeloSoft.Data")
clr.AddReference("BeloSoft.Net")

clr.AddReference("BeloSoft.Betfair.API")
clr.AddReference("BeloSoft.Betfair.StreamingAPI")

clr.AddReference("BeloSoft.Bfexplorer.Domain")
clr.AddReference("BeloSoft.Bfexplorer.Service.Core")
clr.AddReference("BeloSoft.Bfexplorer.Service")

clr.AddReference("BfexplorerHost")

from System.Threading import CancellationToken 
from System.Threading.Tasks import TaskCreationOptions 

from Microsoft.FSharp.Core import FSharpOption
from Microsoft.FSharp.Control import FSharpAsync

from BeloSoft.Bfexplorer import BfexplorerHost
from BeloSoft.Bfexplorer.Service import BfexplorerService

# The function ExecuteAsyncTask executes F# async computation by converting it to Task. 
# I am not able to write python alternative, check the F#, C# or VB code to see how these programming languages use extension function to call F# async computations.
def ExecuteAsyncTask(task):
    task = FSharpAsync.StartAsTask(task, FSharpOption[TaskCreationOptions].get_None, FSharpOption[CancellationToken].get_None)
    return task.Result

bfexplorerService = BfexplorerService(initializeBotManager = FSharpOption[bool].Some(False))
bfexplorerService.UiApplication = BfexplorerHost()

loginResult = ExecuteAsyncTask(bfexplorerService.Login("your user name", "your password"))

if loginResult.IsSuccessResult:
    print("OK")