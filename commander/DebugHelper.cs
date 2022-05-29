using System;

namespace commander
{
    public static class DebugHelper
    {
        public static Action<string> ErrorHandler;
        public static void Error(string txt)
        {
            ErrorHandler?.Invoke(txt);
        }
    }
}
