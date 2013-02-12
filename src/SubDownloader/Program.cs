using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDownloader
{
    class Program
    {

        //From: http://www.fileinfo.com/filetypes/video
        private static string filesPattern = "*.264|*.3g2|*.3gp|*.3gp2|*.3gpp|*.3gpp2|*.3mm|*.3p2|*.60d|*.787|*.890|*.aaf|*.aec|*.aep|*.aepx|*.aet|*.aetx|*.ajp|*.ale|*.am|*.amc|*.amv|*.amx|*.anim|*.aqt|*.arcut|*.arf|*.asf|*.asx|*.avb|*.avc|*.avd|*.avi|*.avp|*.avs|*.avs|*.avv|*.axm|*.bdm|*.bdmv|*.bdt2|*.bdt3|*.bik|*.bin|*.bix|*.bmk|*.bnp|*.box|*.bs4|*.bsf|*.bvr|*.byu|*.camproj|*.camrec|*.camv|*.ced|*.cel|*.cine|*.cip|*.clpi|*.cmmp|*.cmmtpl|*.cmproj|*.cmrec|*.cpi|*.cst|*.cvc|*.cx3|*.d2v|*.d3v|*.dav|*.dce|*.dck|*.dcr|*.dcr|*.ddat|*.dif|*.dir|*.divx|*.dlx|*.dmb|*.dmsd|*.dmsd3d|*.dmsm|*.dmsm3d|*.dmss|*.dmx|*.dnc|*.dpa|*.dpg|*.dream|*.dsy|*.dv|*.dv-avi|*.dv4|*.dvdmedia|*.dvr|*.dvr-ms|*.dvx|*.dxr|*.dzm|*.dzp|*.dzt|*.edl|*.evo|*.eye|*.ezt|*.f4p|*.f4v|*.fbr|*.fbr|*.fbz|*.fcp|*.fcproject|*.ffd|*.flc|*.flh|*.fli|*.flv|*.flx|*.gfp|*.gl|*.gom|*.grasp|*.gts|*.gvi|*.gvp|*.h264|*.hdmov|*.hkm|*.ifo|*.imovieproj|*.imovieproject|*.ircp|*.irf|*.ism|*.ismc|*.ismv|*.iva|*.ivf|*.ivr|*.ivs|*.izz|*.izzy|*.jss|*.jts|*.jtv|*.k3g|*.kmv|*.ktn|*.lrec|*.lsf|*.lsx|*.m15|*.m1pg|*.m1v|*.m21|*.m21|*.m2a|*.m2p|*.m2t|*.m2ts|*.m2v|*.m4e|*.m4u|*.m4v|*.m75|*.mani|*.meta|*.mgv|*.mj2|*.mjp|*.mjpg|*.mk3d|*.mkv|*.mmv|*.mnv|*.mob|*.mod|*.modd|*.moff|*.moi|*.moov|*.mov|*.movie|*.mp21|*.mp21|*.mp2v|*.mp4|*.mp4v|*.mpe|*.mpeg|*.mpeg1|*.mpeg4|*.mpf|*.mpg|*.mpg2|*.mpgindex|*.mpl|*.mpl|*.mpls|*.mpsub|*.mpv|*.mpv2|*.mqv|*.msdvd|*.mse|*.msh|*.mswmm|*.mts|*.mtv|*.mvb|*.mvc|*.mvd|*.mve|*.mvex|*.mvp|*.mvp|*.mvy|*.mxf|*.mxv|*.mys|*.ncor|*.nsv|*.nut|*.nuv|*.nvc|*.ogm|*.ogv|*.ogx|*.osp|*.otrkey|*.pac|*.par|*.pds|*.pgi|*.photoshow|*.piv|*.pjs|*.playlist|*.plproj|*.pmf|*.pmv|*.pns|*.ppj|*.prel|*.pro|*.prproj|*.prtl|*.psb|*.psh|*.pssd|*.pva|*.pvr|*.pxv|*.qt|*.qtch|*.qtindex|*.qtl|*.qtm|*.qtz|*.r3d|*.rcd|*.rcproject|*.rdb|*.rec|*.rm|*.rmd|*.rmd|*.rmp|*.rms|*.rmv|*.rmvb|*.roq|*.rp|*.rsx|*.rts|*.rts|*.rum|*.rv|*.rvid|*.rvl|*.sbk|*.sbt|*.scc|*.scm|*.scm|*.scn|*.screenflow|*.sec|*.sedprj|*.seq|*.sfd|*.sfvidcap|*.siv|*.smi|*.smi|*.smil|*.smk|*.sml|*.smv|*.spl|*.sqz|*.ssf|*.ssm|*.stl|*.str|*.stx|*.svi|*.swf|*.swi|*.swt|*.tda3mt|*.tdx|*.thp|*.tivo|*.tix|*.tod|*.tp|*.tp0|*.tpd|*.tpr|*.trp|*.ts|*.tsp|*.ttxt|*.tvs|*.usf|*.usm|*.vc1|*.vcpf|*.vcr|*.vcv|*.vdo|*.vdr|*.vdx|*.veg|*.vem|*.vep|*.vf|*.vft|*.vfw|*.vfz|*.vgz|*.vid|*.video|*.viewlet|*.viv|*.vivo|*.vlab|*.vob|*.vp3|*.vp6|*.vp7|*.vpj|*.vro|*.vs4|*.vse|*.vsp|*.w32|*.wcp|*.webm|*.wlmp|*.wm|*.wmd|*.wmmp|*.wmv|*.wmx|*.wot|*.wp3|*.wpl|*.wtv|*.wvx|*.xej|*.xel|*.xesc|*.xfl|*.xlmv|*.xmv|*.xvid|*.y4m|*.yog|*.yuv|*.zeg|*.zm1|*.zm2|*.zm3|*.zmv|";
        
        private static string directory;
        private static string filePath;
        private static bool exactMatch = true;
        private static bool silentMode;
        private static string lang;

        static void Main(string[] args)
        {
            // Command line parsing
            Arguments CommandLine = new Arguments(args);
            directory = CommandLine["directory"];
            filePath = CommandLine["file"];
            Boolean.TryParse(CommandLine["silent"], out silentMode);
            Boolean.TryParse(CommandLine["exactmatch"], out exactMatch);
            lang = CommandLine["lang"];

            if (directory == null && filePath == null)
            {
                Console.WriteLine("Error: Need directory or file parameter!");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                return;
            }

            lang = lang ?? "spa";

            OSHelper osHelper = new OSHelper("Ovsleep", lang, exactMatch);

            string[] filesPath;
            if (!string.IsNullOrEmpty(directory))
                filesPath = getFiles(directory, filesPattern, SearchOption.TopDirectoryOnly);
            else
                filesPath = new string[] { filePath };

            foreach (var file in filesPath)
            {
                string filename = file.Substring(file.LastIndexOf("\\"));
                WriteToConsole("Searching: " + filename);
                if(osHelper.DownloadSubtitle(file))
                    WriteToConsole(" - OK!");
                else
                    WriteToConsole(" - FAILED!");
            }

            if (!silentMode)
            {
                WriteToConsole("Press any key to close...");
                Console.ReadKey();
            }
        }

        private static void WriteToConsole(string msg)
        {
            if(!silentMode)
                Console.WriteLine(msg);
        }

        /// <summary>
        /// Returns file names from given folder that comply to given filters
        /// http://www.beansoftware.com/ASP.NET-FAQ/Multiple-Filters-Directory.GetFiles-Method.aspx
        /// </summary>
        /// <param name="SourceFolder">Folder with files to retrieve</param>
        /// <param name="Filter">Multiple file filters separated by | character</param>
        /// <param name="searchOption">File.IO.SearchOption, 
        /// could be AllDirectories or TopDirectoryOnly</param>
        /// <returns>Array of FileInfo objects that presents collection of file names that 
        /// meet given filter</returns>
        private static string[] getFiles(string SourceFolder, string Filter,
         System.IO.SearchOption searchOption)
        {
            // ArrayList will hold all file names
            ArrayList alFiles = new ArrayList();

            // Create an array of filter string
            string[] MultipleFilters = Filter.Split('|');

            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
            }

            // returns string array of relevant file names
            return (string[])alFiles.ToArray(typeof(string));
        }

    }
}
