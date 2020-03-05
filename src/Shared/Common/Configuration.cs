namespace LiveScore.Common
{
    public interface IConfiguration
    {
        string ApiEndPoint { get; }

        string ImageEndPoint { get; }

        string SignalRHubEndPoint { get; }

        string AssetsEndPoint { get; }

        string AppCenterSecret { get; }

        string SentryDsn { get; }

        string Environment { get; }

        bool Debug { get; }

        string EncryptKey { get; }

        string OneSignalAppId { get; }
    }
}