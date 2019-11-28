using System;
using System.Diagnostics;
using LiveScore.Common.Services;

namespace LiveScore.Common.Helpers
{
    public static class AsyncErrorHandler
    {
        public static void HandleException(Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}