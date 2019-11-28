using System;
using LiveScore.Common.Services;
using Refit;

namespace LiveScore.Core.Services
{
    public class BaseService
    {
        public BaseService()
        {
        }

        protected virtual void HandleException(Exception ex)
        {
            if (ex is ApiException apiException)
            {
                var message = $"Response: {apiException?.Content} \r\nRequest URL: {apiException?.RequestMessage?.RequestUri}";
            }
            else
            {
            }
        }
    }
}