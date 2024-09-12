using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading;

namespace mkmd5sum
{
    public partial class Form1 : Form
    {
        List<string> Protocoll = new List<string>();
        Md5Sum Sums = new Md5Sum();

        bool validArgs = false;
        string[] Arguments = null;

        public Form1(string[] args)
        {
            InitializeComponent();

            if(args.Length > 1)
            {
                textBox1.Text = args[0];
                textBox2.Text = args[1];
                validArgs = true;
            }

            Arguments = args;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folder = textBox2.Text;
            progressBar1.Value = 0;
            Protocoll.Clear();

            if (textBox1.Text.ToLower() == "cre")
            {
                Thread t = new Thread(() => { StartCreate(folder); });
                t.Priority = ThreadPriority.Highest;
                t.Start();
                
            }
            else if(textBox1.Text.ToLower() == "ver")
            {
                Thread t = new Thread(() => { Verify(folder); });
                t.Priority = ThreadPriority.Highest;
                t.Start();
            }
            else
            {
                return;
            }
        }

        public static string GetFileMd5(string path)
        {
            var md5 = MD5.Create();
            string hashtext = "";

            using (var stream = File.OpenRead(path))
            {
                byte[] tmpHash = md5.ComputeHash(stream);
                foreach (byte x in tmpHash)
                {
                    hashtext += string.Format("{0:x2}", x);
                }
            }

            md5.Dispose();

            return hashtext;
        }

        public void Log(string msg, int state)
        {
            switch(state)
            {
                case 1:
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case 2:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case 3:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
            }

            Console.WriteLine(msg);
            Protocoll.Add(msg);
            Console.ResetColor();
        }

        public void StartCreate(string folder)
        {
            int files = CountFiles(folder);
            Log($"{files} files found.", 3);
            progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Maximum = files; }));
            Create(folder, folder);
            Sums.Save(folder + "\\md5sum.xml");
            Sums = new Md5Sum();
            File.AppendAllLines(folder + "\\mkmd5sum.log", Protocoll.ToArray());

            if (validArgs)
            {
                Application.Exit();
            }
        }

        public void Verify(string folder)
        {
            Md5Sum sums = Md5Sum.Load(folder + "\\md5sum.xml");
            int files = sums.Files.Count;
            progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Maximum = files; }));

            List<string> ChFiles = new List<string>();

            foreach(SFile file in sums.Files)
            {
                try
                {
                    string filename = folder + "\\" + file.Filename;

                    string fromfile = GetFileMd5(filename);
                    string fromsumfile = file.Checksum;

                    if(fromsumfile != fromfile)
                    {
                        Log($"File has changed: \"{file.Filename}\"", 2);
                        ChFiles.Add(file.Filename);
                    }
                    else
                    {
                        Log($"No changes in \"{file.Filename}\"", 1);
                    }
                }
                catch(Exception e)
                {
                    Log($"Error while processing \"{file.Filename}\": {e.Message}", 2);
                }
            }

            foreach(string s in ChFiles)
            {
                Log($"Changed file: \"{s}\"", 2);
            }

            Sums = new Md5Sum();
            File.AppendAllLines(folder + "\\mkmd5sum.log", Protocoll.ToArray());

            if (validArgs)
            {
                Application.Exit();
            }
        }

        public int CountFiles(string folder)
        {
            string[] subdirs = Directory.GetDirectories(folder);
            int tmp = Directory.GetFiles(folder).Length;

            foreach (string dir in subdirs)
            {
                tmp += CountFiles(dir);
                tmp++;
            }

            return tmp;
        }

        public void Create(string startfolder, string folder)
        {
            string[] files = Directory.GetFiles(folder);
            string[] directories = Directory.GetDirectories(folder);

            foreach (string file in files)
            {
                ProcessFile(startfolder, file);
                progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Value++; }));
            }

            foreach(string directory in directories)
            {
                Create(startfolder, directory);
                progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Value++; }));
                Log($"Processed directory: {directory}", 3);
            }
        }

        public void ProcessFile(string startfolder, string file)
        {
            try
            {
                string checksum = GetFileMd5(file);
                string short_name = file.Replace(startfolder, "").TrimStart('\\');

                SFile sfile = new SFile();
                sfile.Filename = short_name;
                sfile.Checksum = checksum;

                Sums.Files.Add(sfile);

                Log($"Processed file: {file}", 1);
            }
            catch (Exception e)
            {
                Log($"Error while processing \"{file}\": {e.Message}", 2);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(validArgs)
            {
                button1.Visible = false;
                button1_Click(null, null);
            }
        }
    }
}
