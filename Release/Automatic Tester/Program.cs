using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Tester
{
    class Program
    {
        struct NNAVProcess
        {
            public NNAVProcess(int id, string tupel, string resultsFilename)
            {
                ID = id;
                Tupel = tupel;
                ResultsFilename = resultsFilename;
                Finished = false;
            }
            public int ID;
            public string Tupel;
            public string ResultsFilename;
            public bool Finished;
        }

        static List<NNAVProcess> NNAVProcesses = new List<NNAVProcess>();

        private static XmlElement GetXMLElement(XmlElement xmlElement, string name)
        {
            foreach (XmlElement child in xmlElement.ChildNodes)
                if (child.Name == name)
                    return child;
            return null;
        }

        static void NNAVExit(object sender, EventArgs e)
        {
            if (!Directory.Exists("NetworkGenerations\\Finished"))
            {
                System.IO.Directory.CreateDirectory("NetworkGenerations\\Finished");
            }
            Thread.Sleep(1000);
            for (int i = 0; i<NNAVProcesses.Count; i++)
            {
                if(((System.Diagnostics.Process)sender).Id == NNAVProcesses[i].ID)
                {
                    Console.WriteLine($"Tupel {NNAVProcesses[i].Tupel} Finished!");
                    Console.WriteLine("Copying Results...");
                    File.Copy($"NetworkGenerations\\{NNAVProcesses[i].ResultsFilename}", $"NetworkGenerations\\Finished\\{NNAVProcesses[i].ResultsFilename}");
                    Console.WriteLine("Waiting to delete...");
                    Thread.Sleep(500);
                    File.Delete($"NetworkGenerations\\{NNAVProcesses[i].ResultsFilename}");
                    Console.WriteLine("File Deleted!");
                }
            }
        }

        static void StartNNAV(string tupel, string testResultFilename)
        {
            SetSettings(tupel, testResultFilename);
            
            System.Diagnostics.Process execute = new System.Diagnostics.Process();
            execute.StartInfo.FileName = "NNAV.exe";
            execute.EnableRaisingEvents = true;
            execute.Exited += NNAVExit;
            
            execute.Start();
            Console.WriteLine("Started ID = " + execute.Id + " | Tupel = " + tupel);

            NNAVProcesses.Add(new NNAVProcess(id: execute.Id, tupel, testResultFilename));
        }

        static void SetSettings(string tupel, string testResultFilename)
        {
            string settingsFilename = "settings.xml";

            XmlDocument settings = new XmlDocument();
            settings.Load("settings.xml");
            XmlNode root = settings.FirstChild;

            XmlElement general = GetXMLElement((XmlElement)root, "General");

            XmlElement tupelXml = GetXMLElement(general, "Tupel");
            tupelXml.InnerText = tupel;
            general.AppendChild(tupelXml);

            XmlElement autoStart = GetXMLElement(general, "AutoStart");
            autoStart.InnerText = "true";
            general.AppendChild(autoStart);


            XmlElement fileHandler = GetXMLElement((XmlElement)root, "FileHandler");
            XmlElement saveFile = GetXMLElement(fileHandler, "SaveFile");
            saveFile.InnerText = testResultFilename;
            fileHandler.AppendChild(saveFile);
            root.AppendChild(fileHandler);

            settings.AppendChild(root);
            settings.Save(settingsFilename);
        }

        static void Main(string[] args)
        {
            if(!File.Exists("NNAV.exe"))
            {
                Console.WriteLine("Failed to find NNAV.exe\nPlease place this executable in the same folder as NNAV.exe");
                Console.ReadLine();
                return;
            }
            else if (!File.Exists("settings.xml"))
            {
                Console.WriteLine("Failed to find settings.xml");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Filename (Filename is going to be the name you input  + tupel) ");
            string networkFilename = Console.ReadLine();

            Console.WriteLine("Enter Tupel, skriv $ på den variabel som ska varieas: ");

            string tupelen = Console.ReadLine();
            Console.WriteLine("Variable Start:");
            int start= int.Parse(Console.ReadLine());

            Console.WriteLine("Variable End: ");
            int end = int.Parse(Console.ReadLine());

            for (int i = start; i <= end; i++)
            {
                string varTupel = tupelen.Replace("$", i.ToString());
                StartNNAV(varTupel, $"{networkFilename}-{varTupel}.xml");
                Console.WriteLine("Waiting 10s before starting next test.");
                Thread.Sleep(10000);
            }
            Console.WriteLine("All tests are now running");
            for (;;)
                Console.ReadLine();
        }
    }
}
