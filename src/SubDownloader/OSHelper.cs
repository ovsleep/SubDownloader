using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDownloader
{
    public struct MovieHashQueryParameter
    {
        public string sublanguageid;
        public string moviehash;
        public double moviebytesize;
    }

    public class OSHelper
    {
        private IOSProxy proxy;
        private string Lang;
        private string UserAgent;
        private bool ExactMatch;

        public OSHelper(string userAgent, string lang, bool exactMatch)
        {
            Lang = lang;
            UserAgent = userAgent;
            proxy = XmlRpcProxyGen.Create<IOSProxy>();
            ExactMatch = exactMatch;
        }

        public bool DownloadSubtitle(string filePath)
        {
            var token = LogIn();

            try
            {
                string idSubtitleFile = SearchSubtitles(token, filePath);
                DownloadSubtitle(idSubtitleFile, token, filePath);
            }
            catch
            {
                return false;
            }
            finally
            {
                LogOut(token);
            }

            return true;
        }

        private string LogIn()
        {
            //Login
            XmlRpcStruct ret = proxy.LogIn("ovsleep", "ovsleep", Lang, UserAgent);
            return ret["token"].ToString();
        }

        private void LogOut(string token)
        {
            //LogOut
            XmlRpcStruct retOut = proxy.LogOut(token.ToString());
        }

        private string SearchSubtitles(string token, string filename)
        {
            //Get Movie Hash
            double size;
            string hash = ToHexadecimal(ComputeMovieHash(filename, out size));
            //Console.WriteLine("The hash of the movie-file is: {0}", hash);
            //Console.WriteLine("The size of the movie-file is: {0}", size);

            MovieHashQueryParameter parameters;
            parameters.sublanguageid = Lang;
            parameters.moviehash = hash;
            parameters.moviebytesize = size;

            XmlRpcStruct retOut = proxy.SearchSubtitles(token, new object[] { parameters });

            //TODO: check status

            Object[] dataArray = retOut["data"] as Object[];
            string idSubFile = "-1";

            foreach (var data in dataArray)
            {
                XmlRpcStruct result = data as XmlRpcStruct;
                idSubFile = result["IDSubtitleFile"].ToString();
            }

            return idSubFile;
        }

        private void DownloadSubtitle(string idSubtitleFile, string token, string filename)
        {
            XmlRpcStruct retOut = proxy.DownloadSubtitles(token, new object[] { idSubtitleFile });

            Object[] data = retOut["data"] as Object[];
            string strBase64 = "-1";
            foreach (var result in data)
            {
                XmlRpcStruct res = result as XmlRpcStruct;
                strBase64 = res["data"].ToString();

                byte[] compressedBytes;
                compressedBytes = Convert.FromBase64String(strBase64);
                byte[] subBytes = Decompress(compressedBytes);

                string subName = filename.Substring(0, filename.LastIndexOf('.')) + ".srt";
                FileStream fs = new FileStream(subName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(subBytes);
            }
        }

        #region MovieHash
        private static byte[] ComputeMovieHash(string filename, out double size)
        {
            byte[] result;
            FileInfo f = new FileInfo(filename);

            using (Stream input = f.OpenRead())
            {
                result = ComputeMovieHash(input);
            }

            size = f.Length;

            return result;
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long lhash, streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }
        private static string ToHexadecimal(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }
        #endregion

        #region GZip
        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
        #endregion
    }
}
