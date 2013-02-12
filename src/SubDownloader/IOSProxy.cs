using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDownloader
{
    [XmlRpcUrl("http://api.opensubtitles.org/xml-rpc")]
    public interface IOSProxy : IXmlRpcProxy
    {
        [XmlRpcMethod("ServerInfo")]
        XmlRpcStruct ServerInfo();

        [XmlRpcMethod("LogIn")]
        XmlRpcStruct LogIn(string user, string pass, string lang, string userAgent);

        [XmlRpcMethod("SearchSubtitles")]
        XmlRpcStruct SearchSubtitles(string token, Object[] parameters);

        [XmlRpcMethod("DownloadSubtitles")]
        XmlRpcStruct DownloadSubtitles(string token, Object[] parameters);

        [XmlRpcMethod("LogOut")]
        XmlRpcStruct LogOut(string token);
    }
}
