
namespace Growthstories.UI.WindowsPhone
{
    public class BootstrapMachine : Bootstrap
    {


        private const string BACKENDHOST = "dennari-macbook.lan";
        private const int BACKENDPORT = 8080;
        private const string LOGHOST = "dennari-macbook.lan";
        private const int LOGPORT = 28777;



        public BootstrapMachine(App app)
            : base(app)
        {

        }

        protected override void LogConfiguration()
        {
            GSRemoteLog.Host = LOGHOST;
            GSRemoteLog.Port = LOGPORT;

            RxUIResolver.Register(() => GSRemoteLog.Instance, typeof(ILogger));
            LogFactory.BuildLogger = type => new GSRemoteLog(type);
            GSLogFactory.BuildLogger = type => new GSRemoteLog(type);
        }

        protected override void HttpConfiguration(string host = "default.lan", int port = 80)
        {
            base.HttpConfiguration(BACKENDHOST, BACKENDPORT);
        }

    }
}
