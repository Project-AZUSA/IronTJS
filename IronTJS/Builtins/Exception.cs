﻿using Microsoft.Scripting.Actions;

namespace IronTJS.Builtins
{
    public static class Exception
	{
		public static readonly ExtensionPropertyTracker messageProperty = new ExtensionPropertyTracker("message", typeof(System.Exception).GetMethod("get_Message"), null, null, typeof(System.Exception));
	}
}
