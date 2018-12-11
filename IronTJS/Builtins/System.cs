using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Actions;

namespace IronTJS.Builtins
{
    public static class KrSystem
    {
        public static void inform(object str) => Console.WriteLine(str);
    }
}
