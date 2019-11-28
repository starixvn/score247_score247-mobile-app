using LiveScore.Core;

namespace LiveScore.Configurations
{
    public class Configuration : IConfiguration
    {
        public string AssetsEndPoint { get; }
        public string AppCenterSecret { get; }
        public string SentryDsn { get; }
        public string Enviroment { get; }
        public bool Debug { get; }
        public string ApiEndPoint { get; }
        public string SignalRHubEndPoint { get; }
    }
}