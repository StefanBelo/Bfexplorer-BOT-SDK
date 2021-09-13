# Using IronPython3
# https://github.com/IronLanguages/ironpython3/releases/tag/v3.4.0-alpha1

import clr
clr.AddReference("System.Windows.Forms")

from System.Windows.Forms import MessageBox, MessageBoxButtons

MessageBox.Show("Hello World!", "Greetings", MessageBoxButtons.OKCancel)