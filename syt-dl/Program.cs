using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace syt_dl {
    class Program {
        //Define variables
        public static string version = "420.73";
        public static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sytdl";
        public static string currentdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool suppression = true;
        public static int guiformatindex = 0;
        public static string guiformat = "mp4";
        public static gui gui = new gui();

        //Does stuff lennyface
        [STAThread]
        static void Main(string[] args) {
            Console.Title = "Syt-dl";
            String[] arguments = Environment.GetCommandLineArgs();
            List<string> commands = Calls.getMethods(typeof(void));

            //If there are no arguments
            if (arguments.Length < 2) {
                Console.WriteLine("Syt-dl version " + version);
                gui.ShowDialog();
            
            //If there is a command being called
            } else if (commands.Contains(arguments[1].Replace("--", "")) && arguments[1].Contains("--")) {
                Calls.callMethod(arguments[1].Replace("--", ""), arguments);
            
            //If it's a URL
            } else if (Calls.urlExists(arguments[1])) {
                download(arguments[1], arguments);
            
            //If it's a file
            } else if (arguments[1].Contains('.')) {
                convertVideo(arguments[1], arguments);
            
            //If nothing makes sense
            } else {
                Console.WriteLine("Usage: syt-dl [URL] [FLAGS]");
                Console.WriteLine("Type --help to see a list of all commands.");
            }
        }

        //Converts the video
        public static void convertVideo(string filename, string[] args) {
            //Define variables
            string format = "mp4";
            suppression = true;
            string name;
            if (filename.Contains('\\')) {
                string[] namee = filename.Split('\\');
                name = namee[namee.Length - 1].Split('.')[0];
            } else {
                name = filename.Split('.')[0];
            }

            //If Directory doesn't exist, update
            if (Directory.Exists(filepath) == false) {
                update();
            }

            ////Flags
            //Format
            if (args.Contains("-f")) {
                int anumber = Array.IndexOf(args, "-f");
                format = args[anumber + 1];
            } else {
                Console.WriteLine("Format not chosen. Defaulting to mp4.");
            }
            //Invisible
            if (args.Contains("-i")) {
                suppression = false;
            }

            //Convert to format
            Calls.writeColor("Converting to ." + format + "...", ConsoleColor.Cyan);
            try {
                sendCommand("ffmpeg", "-y -i \"" + filename + "\" -c:v libx264 " + name + "." + format);
            } catch (Exception ex) {
                Console.WriteLine("Error with ffmpeg: " + ex.ToString());
                return;
            }

            if(!File.Exists(currentdir + "\\" + name + "." + format)) {
                Calls.writeColor("ERROR: FFMPEG Can't Convert That :(", ConsoleColor.Red);
                return;
            }

            //Finished.
            Console.WriteLine("");
            suppression = true;
            Calls.writeColor("Done.", ConsoleColor.Cyan);
        }

        //Downloads the url
        public static void download(string url, string[] args) {
            //Define variables
            string audformats = Calls.getText("http://pastebin.com/raw/FANfg0JD");
            string format = "mp4";
            suppression = true;

            //If Directory doesn't exist, update
            if (Directory.Exists(filepath) == false) {
                update();
            }

            ////Flags
            //Format
            if (args.Contains("-f")) {
                int anumber = Array.IndexOf(args, "-f");
                try {
                    format = args[anumber + 1];
                } catch (IndexOutOfRangeException ex) {
                    Console.WriteLine("Use -f properly idiot.");
                    return;
                }
            } else {
                Console.WriteLine("Format not chosen. Defaulting to mp4.");
            }
            //Invisible
            if (args.Contains("-i")) {
                suppression = false;
            }

            //Checks if it wants a video or just audio
            if (audformats.Contains("." + format)) {
                downloadAudio(url, format);
            } else {
                downloadVideo(url, format);
            }
        }

        //Downloads the video in a differnet way
        public static void downloadVideo(string url, string format) {
            //Define variables
            Random rnd = new Random();
            int rand = rnd.Next(10000);

            //Downloads the video with youtube-dl
            Calls.writeColor("Downloading video...", ConsoleColor.Cyan);
            try {
                sendCommand("youtube-dl", "--no-playlist -f bestvideo  -o \"" + filepath + "\\vid" + rand + "\" " + url);
            } catch (Exception ex) {
                Console.WriteLine("Error with youtube-dl: " + ex.ToString());
                return;
            }
            string vidname = "vid" + rand;

            //Check and see if the video exists before downloading anything else
            if (!File.Exists(filepath + "\\" + vidname)) {
                Calls.writeColor("ERROR: YouTube-DL Couldn't Find The Video.", ConsoleColor.Red);
                Console.WriteLine("");
                return;
            }

            //Downlaods the audio with youtube-dl
            Console.WriteLine("");
            Calls.writeColor("Downloading audio...", ConsoleColor.Cyan);
            try {
                sendCommand("youtube-dl", "--no-playlist -f bestaudio  -o \"" + filepath + "\\aud" + rand + "\" " + url);
            } catch (Exception ex) {
                Console.WriteLine("Error with youtube-dl: " + ex.ToString());
                return;
            }
            string audname = "aud" + rand;

            //Tests if it's a webm
            string videoinfo = sendCommandOutput("ffprobe", "-i \"" + filepath + "\\" + vidname + "\" -hide_banner", true);

            //Merges or converts the audio and video
            Console.WriteLine("");
            try {
                if (videoinfo.Contains("Video: vp9")) {
                    Calls.writeColor("Converting to ." + format + "...", ConsoleColor.Cyan);
                    sendCommand("ffmpeg", "-y -i \"" + filepath + "\\" + vidname + "\" -i \"" + filepath + "\\" + audname + "\" -c:v libx264 -c:a aac -strict experimental " + Calls.getVideoID(url) + "." + format);
                } else {
                    Calls.writeColor("Merging to ." + format + "...", ConsoleColor.Cyan);
                    sendCommand("ffmpeg", "-y -i \"" + filepath + "\\" + vidname + "\" -i \"" + filepath + "\\" + audname + "\" -c:v copy -c:a aac -strict experimental " + Calls.getVideoID(url) + "." + format);
                }
            } catch (Exception ex) {
                Console.WriteLine("Error with ffmpeg: " + ex.ToString());
                return;
            }

            //Finished.
            File.Delete(filepath + "\\" + vidname);
            File.Delete(filepath + "\\" + audname);
            Console.WriteLine("");
            Calls.writeColor("Done.", ConsoleColor.Cyan);
        }

        //Downloads the audio
        public static void downloadAudio(string url, string format) {
            //Define variables
            Random rnd = new Random();
            int rand = rnd.Next(10000);

            //Downlaods the audio with youtube-dl
            Calls.writeColor("Downloading audio...", ConsoleColor.Cyan);
            try {
                sendCommand("youtube-dl", "--no-playlist -f bestaudio  -o \"" + filepath + "\\aud" + rand + "\" " + url);
            } catch (Exception ex) {
                Console.WriteLine("Error with youtube-dl: " + ex.ToString());
                return;
            }
            string audname = "aud" + rand;

            //Check and see if the video exists before downloading anything else
            if (!File.Exists(filepath + "\\" + audname)) {
                Calls.writeColor("ERROR: YouTube-DL Couldn't Find The Video.", ConsoleColor.Red);
                Console.WriteLine("");
                return;
            }

            //converts the audio
            Console.WriteLine("");
            try {
                Calls.writeColor("Converting to ." + format + "...", ConsoleColor.Cyan);
                sendCommand("ffmpeg", "-y -i \"" + filepath + "\\aud" + rand + "\" -acodec libmp3lame -aq 4 " + Calls.getVideoID(url) + "." + format);
            } catch (Exception ex) {
                Console.WriteLine("Error with ffmpeg: " + ex.ToString());
                return;
            }

            //Finished.
            File.Delete(filepath + "\\" + audname);
            Console.WriteLine("");
            Calls.writeColor("Done.", ConsoleColor.Cyan);
        }

        //Updates ffmpeg and youtube-dl program
        public static void update() {
            Calls.writeColor("Updating syt-dl...", ConsoleColor.Cyan);

            //IF FIRST TIME RUNNING
            if (Directory.Exists(filepath) == false) {
                Console.WriteLine("Creating file path...");
                Directory.CreateDirectory(filepath);
                File.WriteAllBytes(filepath + "//youtube-dl.exe", syt_dl.Properties.Resources.youtube_dl);
                Console.WriteLine("Path created.");
            }

            //UPDATE YOUTUBE-DL
            Console.WriteLine("Updating youtube-dl...");
            Program.sendCommand("youtube-dl", "--update");
            Console.WriteLine("Updated.");

            //UPDATE FFMPEG
            WebClient downloader = new WebClient();
            Uri url = new Uri("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-latest-win64-static.zip");
            Console.WriteLine("Downloading ffmpeg...");
            downloader.DownloadFileAsync(url, filepath + "//static.zip");
            while (downloader.IsBusy) { }
            Console.WriteLine("Download Completed.");
            Console.WriteLine("Extracting ffmpeg...");
            ZipFile.ExtractToDirectory(filepath + "//static.zip", filepath + "//stuff");
            string ffmpegfp = filepath + "//stuff//ffmpeg-latest-win64-static//bin";
            if (File.Exists(filepath + "//ffmpeg.exe")) {
                File.Delete(filepath + "//ffmpeg.exe");
                File.Delete(filepath + "//ffplay.exe");
                File.Delete(filepath + "//ffprobe.exe");
            }
            File.Move(ffmpegfp + "//ffmpeg.exe", filepath + "//ffmpeg.exe");
            File.Move(ffmpegfp + "//ffplay.exe", filepath + "//ffplay.exe");
            File.Move(ffmpegfp + "//ffprobe.exe", filepath + "//ffprobe.exe");
            Console.WriteLine("Extracted.");
            Console.WriteLine("Deleting excess files...");
            Directory.Delete(filepath + "//stuff", true);
            File.Delete(filepath + "//static.zip");
            Console.WriteLine("Deleted.");

            //DONE
            Calls.writeColor("syt-dl is up-to-date", ConsoleColor.Cyan);
        }
        
        //Downlaods the video, but slower
        public static void legacydownloadVideo(string url, string[] args) {
            //Define variables
            string format = "mp4";
            Random rnd = new Random();
            int rand = rnd.Next(10000);

            //If Directory doesn't exist, update
            if (Directory.Exists(filepath) == false) {
                update();
            }

            ////Flags
            //Format
            if (args.Contains("-f")) {
                int anumber = Array.IndexOf(args, "-f");
                try {
                    format = args[anumber + 1];
                } catch (IndexOutOfRangeException ex) {
                    Console.WriteLine("Use -f properly idiot.");
                    return;
                }
            } else {
                Console.WriteLine("Format not chosen. Defaulting to mp4.");
            }
            //Invisible
            if (args.Contains("-i")) {
                suppression = false;
            }

            //Start the download with youtube-dl
            Calls.writeColor("Downloading video...", ConsoleColor.Cyan);
            try {
                sendCommand("youtube-dl", "--no-playlist -f bestvideo+bestaudio  -o \"" + filepath + "\\meme" + rand + "\" " + url);
            } catch (Exception ex) {
                Console.WriteLine("Error with youtube-dl: " + ex.ToString());
                return;
            }

            //See if you need to convert
            string filename = Calls.getFile("meme" + rand);
            if (filename == null) {
                Calls.writeColor("ERROR: YouTube-DL Couldn't Find The Video.", ConsoleColor.Red);
            } else if (filename.Split('.')[1] == format) {
                Console.WriteLine(""); //It was originally after sendCommand, but then it would put a space after things, and that's annoying
                string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!File.Exists(loc + "\\" + Calls.getVideoID(url) + "." + format)) {
                    File.Move(filename, loc + "\\" + Calls.getVideoID(url) + "." + format);
                } else {
                    Calls.writeColor("ERROR: File already exists!", ConsoleColor.Red);
                }
                File.Delete(filename);

                //Convert to format
            } else {
                Console.WriteLine("");
                Calls.writeColor("Converting to ." + format + "...", ConsoleColor.Cyan);
                try {
                    sendCommand("ffmpeg", "-y -i " + filename + " -c:v libx264 " + Calls.getVideoID(url) + "." + format);
                    Console.WriteLine("");
                } catch (Exception ex) {
                    Console.WriteLine("Error with ffmpeg: " + ex.ToString());
                    return;
                }
                File.Delete(filename);
            }

            //Finished.
            suppression = true;
            Calls.writeColor("Done.", ConsoleColor.Cyan);
            Console.WriteLine("");
        }

        //send invis console command
        public static void sendCommand(string program, string argss) {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = filepath + "//" + program + ".exe";
            startInfo.Arguments = argss;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.OutputDataReceived += new DataReceivedEventHandler(Calls.displayPerc);
            process.ErrorDataReceived += new DataReceivedEventHandler(Calls.displayPerc);
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            //return "meme";
        }

        //send console command with output
        public static string sendCommandOutput(string program, string argss, bool error) {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = filepath + "//" + program + ".exe";
            startInfo.Arguments = argss;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string errorout = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (error) {
                return errorout;
            } else {
                return output;
            }
        }

        //Updates syt-dl only
        public static void sytdlUpdate() {
            string text = Calls.getText("http://ayyn.us/memes/version.txt");
            string[] vars = text.Split('|');
            if(vars[0] != version) {

                //Downloads the new version of syt-dl
                WebClient downloader = new WebClient();
                Uri url = new Uri(vars[1]);
                Console.WriteLine("Downloading the new version of Syt-dl...");
                downloader.DownloadFile(url, filepath + "//sytdl.zip");
                Console.WriteLine("Download Completed.");

                //Extracting
                Console.WriteLine("Extracting sytdl...");
                ZipFile.ExtractToDirectory(filepath + "//sytdl.zip", filepath + "//memes");
                if (File.Exists(filepath + "//sytdlupdating.exe")) {
                    File.Delete(filepath + "//sytdlupdating.exe");
                }
                if (File.Exists(filepath + "//batcrap.bat")) {
                    File.Delete(filepath + "//batcrap.bat");
                }
                File.Move(filepath + "//memes//syt-dl.exe", filepath + "//sytdlupdating.exe");

                //Delete stuff
                Directory.Delete(filepath + "//memes", true);
                File.Delete(filepath + "//sytdl.zip");

                //Run batch file
                Calls.writeBatch();
            } else {
                Console.WriteLine("Syt-dl is up to date");
            }
        }
    }

    //Make new commands here
    class Commands {
        public static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sytdl";

        public static void echo(string[] args) {
            Console.WriteLine(string.Join(" ", args));
        }
        public void help(string[] args) {
            Calls.writeColor("SYNTAX: ", ConsoleColor.DarkYellow);
            Console.WriteLine("syt-dl [URL] [FLAGS]");
            Console.WriteLine("");
            Calls.writeColor("COMMANDS: ", ConsoleColor.DarkYellow);
            List<string> commands = Calls.getMethods(typeof(void));
            foreach(string command in commands) {
                Console.WriteLine("--" + command);
            }
        }
        public void update(string[] args) {
            Program.update();
        }
        public void flags(string[] args) {
            Calls.writeColor("FLAGS: ", ConsoleColor.DarkYellow);
            Console.WriteLine("-s: Un-suppresses data (For bug testing)");
            Console.WriteLine("-f [FORMAT]: Selects the output format for the video (Default mp4)");
        }
        public void ianisdumb(string[] args) {
            Calls.writeColor("en is dumb", ConsoleColor.Red);
        }
        public void sytdlupdate(string[] args) {
            Program.sytdlUpdate();
        }
    }

    class Calls {
        //Gets the video id given a url
        public static string getVideoID(string big) {
            if (big.Contains("youtu.be/")) {
                string first = "youtu.be/";
                string result = big.Substring(big.IndexOf(first) + first.Length);
                string[] meme = result.Split('?');
                return meme[0];
            } else {
                string first = "youtube.com/watch?v=";
                string result = big.Substring(big.IndexOf(first) + first.Length);
                string[] meme = result.Split('&');
                return meme[0];
            }
        }
        
        //Gets all methods of a certain type
        public static List<string> getMethods(Type type) {
            Type tpe = typeof(Commands);
            List<string> methods = new List<string>();
            foreach (var method in tpe.GetMethods()) {
                if (method.ReturnType == type) {
                    methods.Add(method.Name);
                }
            }
            return methods;
        }
        
        //Gets and executes the desired method
        public static void callMethod(String mymethod, string[] args) {
            try {
                Type type = Type.GetType("syt_dl.Commands");
                Object obj = Activator.CreateInstance(type);
                MethodInfo methodInfo = type.GetMethod(mymethod);
                methodInfo.Invoke(obj, new[] { args });
            } catch (TargetInvocationException ex) {
                if (ex.ToString().Contains("System.IndexOutOfRangeException")) {
                    Console.WriteLine("Input requires more data. Pls try again.");
                } else {
                    Console.WriteLine(ex);
                }
            }
        }

        //Finds a file given a partial name
        public static string getFile(string partialName) {
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(Program.filepath);
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles(partialName + ".*");

            foreach (FileInfo foundFile in filesInDir) {
                string fullName = foundFile.FullName;
                return fullName;
            }
            return null;
        }
        
        //Tests if URL exists
        public static bool urlExists(string url) {
            try {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            } catch {
                //Any exception will returns false.
                return false;
            }
        }
        
        //Gets the percentage till the download is done. Also outputs everyting from sendCommand
        public static void displayPerc(object sendingProcess, DataReceivedEventArgs outLine) {
            string output = outLine.Data + " ";
            if (output.StartsWith("[download]") && output.Contains('%')) {
                string cat = output.Replace("[download] ", "");
                Console.Write("\r" + cat);
            } else if (output.StartsWith("frame")) {
                Console.Write("\r" + output);
            } else if (output.StartsWith("size")) {
                Console.Write("\r" + output);
            } else if (Program.suppression == false) {
                Console.WriteLine(output);
            }
        }

        //Write a new line with a certain color
        public static void writeColor(string message, ConsoleColor meme) {
            Console.ForegroundColor = meme;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        //Tests if program is minimized
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        private struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
        public static bool isMinimized(string application) {
            var processes = Process.GetProcesses();
            foreach(Process p in processes) {
                if (p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle.Contains(application)) {
                    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                    GetWindowPlacement(p.MainWindowHandle, ref placement);
                    switch (placement.showCmd) {
                        case 1:
                            return false;
                        case 2:
                            return true;
                    }
                }
            }
            //Console.WriteLine("Error");
            return false;
        }

        //Get text from url
        public static string getText(string url) {
            var webRequest = WebRequest.Create(@url);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content)) {
                var strContent = reader.ReadToEnd();
                return strContent;
            }
        }

        //Writes a batch file to delete stydl and run it again
        public static void writeBatch() {
            string currentdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] lines = { "@echo off",
                "PING 1.1.1.1 -n 1 -w 5000 >NUL",
                "del /f /q \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"",
                "cd \"" + Program.filepath + "\"",
                "ren sytdlupdating.exe \"" + AppDomain.CurrentDomain.FriendlyName + "\"",
                "move /Y \"" + Program.filepath + "\\" + AppDomain.CurrentDomain.FriendlyName + "\" \"" + currentdir + "\""};
            File.WriteAllLines(Program.filepath + "\\batcrap.bat", lines);
            Process.Start(Program.filepath + "\\batcrap.bat");
            Application.Exit();
            Environment.Exit(1);
        }
    }
}