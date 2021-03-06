﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Shell;

namespace EQEmu_Patcher
{
    public partial class MainForm : Form
    {

        string clientVersion;

       // TaskbarItemInfo tii = new TaskbarItemInfo();
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("In case you didn't realize it, this patcher doesn't work yet. At this time it's purely under development for testing.");
           // tii.ProgressState = TaskbarItemProgressState.Normal;
          //  tii.ProgressValue = (double)50 /100;
            
            //Read:
            /*
            var pv = JsonConvert.DeserializeObject<PatchVersion>(test);
            MessageBox.Show(pv.ServerName);
            */
            //Write:
            /*
            PatchVersion pv = new PatchVersion();
            pv.ServerName = "Test";
            pv.RootFiles.Add("eqgame.exe", "12345test");
            txtList.Text = JsonConvert.SerializeObject(pv);
            */
            try {

                var hash = UtilityLibrary.GetEverquestExecutableHash(AppDomain.CurrentDomain.BaseDirectory);
                if (hash == "") { 
                    MessageBox.Show("Please run this patcher in your Everquest directory.");
                    this.Close();
                    return;
                }
                switch (hash)
                {
                    case "85218FC053D8B367F2B704BAC5E30ACC":
                        txtList.Text = "You seem to have put me in a Secrets of Feydwer client directory";
                        clientVersion = "Secrets of Feydwer";
                        break;
                    case "859E89987AA636D36B1007F11C2CD6E0":
                        txtList.Text = "You seem to have put me in a Underfoot client directory";
                        clientVersion = "Underfoot";
                        break;
                    case "BB42BC3870F59B6424A56FED3289C6D4":
                        txtList.Text = "You seem to have put me in a Titanium client directory";
                        clientVersion = "Titanium";
                        break;
                    default:
                        txtList.Text = "I don't recognize the Everquest client in my directory, send this to Shin: " + hash;
                        break;
                }
                txtList.Text += "\r\n\r\nIf you wish to help out, press the scan button on the bottom left and wait for it to complete, then copy paste this data as an Issue on github!";
            }
            catch (UnauthorizedAccessException err)
            {
                MessageBox.Show("You need to run this program with Administrative Privileges" + err.Message);
                return;
            }
            
           
        }

        System.Diagnostics.Process process;
      

        System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();

        Dictionary<string, string> WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            var fileMap = new Dictionary<string, string>();
            try
            {
                 files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                txtList.Text += e.Message +"\n";
                return fileMap;
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                txtList.Text += e.Message + "\r\n";
                return fileMap;
            }

            if (files != null)
            {
                var count = 0;
                progressBar.Maximum = files.Length;
                
                foreach (System.IO.FileInfo fi in files)
                {
                    count++;
                    if (fi.Name.Contains(".ini"))
                    { //Skip INI files
                        continue;
                    }
                    if (fi.Name == System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                    { //Skip self EXE
                        continue;
                    }

                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    var md5 = UtilityLibrary.GetMD5(fi.FullName);
                    txtList.Text += fi.Name + ": " + md5 + "\r\n";
                    progressBar.Value = count;
                    fileMap[fi.Name] = md5;
                    txtList.Refresh();
                    updateTaskbarProgress();
                    Application.DoEvents();
                    
                }
                //One final update of data
                progressBar.Value = count;
                txtList.Refresh();
                updateTaskbarProgress();
                Application.DoEvents();
            }
            return fileMap;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            txtList.Text = "";
            var fileMap = WalkDirectoryTree(new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            PatchVersion pv = new PatchVersion();
            pv.ClientVersion = clientVersion;
            pv.RootFiles = fileMap;
            txtList.Text = JsonConvert.SerializeObject(pv);
        }

        private void updateTaskbarProgress()
        {
            
            if (Environment.OSVersion.Version.Major < 6)
            { //Only works on 6 or greater
                return;
            }
            
            
           // tii.ProgressState = TaskbarItemProgressState.Normal;            
           // tii.ProgressValue = (double)progressBar.Value / progressBar.Maximum;            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try {
                process = UtilityLibrary.StartEverquest();
            }
            catch  (Exception err) {
                MessageBox.Show("An error occured: "   + err.Message);
            }
        }
    }
}
