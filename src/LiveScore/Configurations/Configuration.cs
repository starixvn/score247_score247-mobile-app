using LiveScore.Common;

namespace LiveScore.Configurations
{
    public class Configuration : IConfiguration
    {
#if DEBUG
        public string ApiEndPoint => "https://score247-api1.nexdev.net/dev2/api";
        public string ImageEndPoint => "https://score247-api1.nexdev.net/dev2/news/images";
        public string SignalRHubEndPoint => "https://score247-api2.nexdev.net/dev2/hubs";
        public string AssetsEndPoint => "https://assets-dev.nexdev.net/";
        public string AppCenterSecret => "ios=b08e2753-b596-44c9-bed9-701d8dd8be8c;";
        public string SentryDsn => "https://4cf636af675645359c573f1b923693f8@sentry.nexdev.net/88";
        public string Environment => "Local";
        public bool Debug => true;
        public string EncryptKey => "e6e42fa2cd8db55e10c2b7b4833e0f8e";

        public string OneSignalAppId => "e7988708-b94d-4177-962d-ee23999ba9c1";

#elif TEST
        public string ApiEndPoint => "https://score247-api1.nexdev.net/dev/api";
        public string ImageEndPoint => "https://score247-api1.nexdev.net/test/news/images";
        public string SignalRHubEndPoint => "https://score247-api2.nexdev.net/dev/hubs";
        public string AssetsEndPoint => "https://assets-dev.nexdev.net/";
        public string AppCenterSecret => "ios=b08e2753-b596-44c9-bed9-701d8dd8be8c;";
        public string SentryDsn => "https://4cf636af675645359c573f1b923693f8@sentry.nexdev.net/88";
        public string Environment => "Local";
        public bool Debug => true;
        public string EncryptKey => "e6e42fa2cd8db55e10c2b7b4833e0f8e";
        public string OneSignalAppId => "e7988708-b94d-4177-962d-ee23999ba9c1";

        //DataGenerator: http://ha.nexdev.net:7208/test
#elif LocalRelease
        public string ApiEndPoint => "https://score247-api3.nexdev.net/main/api";
        public string ImageEndPoint => "https://score247-api3.nexdev.net/test/news/images";
        public string SignalRHubEndPoint => "https://score247-api4.nexdev.net/main/hubs";
        public string AssetsEndPoint => "https://assets-dev.nexdev.net/main/";
        public string AppCenterSecret => "ios=b08e2753-b596-44c9-bed9-701d8dd8be8c;";
        public string SentryDsn => "https://a75e3e7b51ea4de8baa2c27b67bbede3@sentry.nexdev.net/34";
        public string Environment => "Local";
        public bool Debug => true;
        public string EncryptKey => "350cd642f6764a154a9e4f03eb3121b6";
        public string OneSignalAppId => "e7988708-b94d-4177-962d-ee23999ba9c1";

#else

        public string ApiEndPoint => "https://api.score247.net/api";
        public string ImageEndPoint => "https://api.score247.net/news/images";
        public string SignalRHubEndPoint => "https://publisher.score247.net/hubs";
        public string AssetsEndPoint => "https://assets.score247.net/";
        public string AppCenterSecret => "ios=34adf4e9-18dd-4ef0-817f-48bce4ff7159;";
        public string SentryDsn => "https://a75e3e7b51ea4de8baa2c27b67bbede3@sentry.nexdev.net/34";
        public string Environment => "Testflight";
        public bool Debug => false;
        public string EncryptKey => "350cd642f6764a154a9e4f03eb3121b6";
        public string OneSignalAppId => "e7988708-b94d-4177-962d-ee23999ba9c1";

#endif
    }
}