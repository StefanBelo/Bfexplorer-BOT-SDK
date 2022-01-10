# Using IronPython3
# https://github.com/IronLanguages/ironpython3/releases/tag/v3.4.0-alpha1
# http://pythonnet.github.io/

# https://github.com/pythonnet/pythonnet/issues/1039

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

from Microsoft.FSharp.Core import FSharpOption
from Microsoft.FSharp.Control import FSharpAsync

#from BeloSoft.Bfexplorer import BfexplorerHost, FSharpExtensions
from BeloSoft.Bfexplorer import BfexplorerHost
from BeloSoft.Bfexplorer.Service import BfexplorerService

def ExecuteAsyncTask(task):
    task = FSharpAsync.StartAsTask(task, FSharpOption[TaskCreationOptions].get_None, FSharpOption[CancellationToken].get_None)
    return task.Result

for assembly in clr.ListAssemblies(True):
    print(assembly)

bfexplorerService = BfexplorerService(initializeBotManager = FSharpOption[bool].Some(False))
bfexplorerService.UiApplication = BfexplorerHost()

loginTask = FSharpExtensions.ExecuteAsyncTask(bfexplorerService.Login("your user name", "your password"))
loginResult = loginTask.Result

if loginResult.IsSuccessResult:
    print("OK")