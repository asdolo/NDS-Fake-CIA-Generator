using ds_fake_cia_cretor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ds_fake_cia_creator
{
    public partial class frmMain : Form
    {
        public string ndsFile;
        public frmMain()
        {
            InitializeComponent();
        }
        

        private void btnConvert_Click(object sender, EventArgs e)
        {
            OpenFileDialog asd = new OpenFileDialog();
            asd.Filter = "NDS Files (*.nds)|*.nds|All Files|*";

            asd.Multiselect = true;

            if (asd.ShowDialog() == DialogResult.OK)
            {
                string fileFolder = asd.FileName.Substring(0, asd.FileName.Length - asd.SafeFileName.Length);

                List<string> doneCIAs = new List<string>();

                progressBar1.Maximum = asd.FileNames.Length;
                progressBar1.Value = 0;

                for (int i = 0; i < asd.FileNames.Length; ++i)
                {
                    label2.Text = "Processing: " + asd.SafeFileNames[i];
                    ndsFile = asd.FileNames[i];

                    string safeFileNameWithOutExension = asd.SafeFileNames[i].Substring(0, asd.SafeFileNames[i].Length - 4);
                    string extension = asd.SafeFileNames[i].Substring(asd.SafeFileNames[i].Length - 3);



                    if (extension == "nds")
                    {


                        if (!Directory.Exists(Application.StartupPath + "\\temp"))
                        {
                            Directory.CreateDirectory(Application.StartupPath + "\\temp");
                        }
                        
                        File.WriteAllBytes(Application.StartupPath + "\\temp\\make_cia.exe", Properties.Resources.make_cia);
                        File.WriteAllBytes(Application.StartupPath + "\\temp\\ndstool.exe", Properties.Resources.ndstool);
                        File.WriteAllBytes(Application.StartupPath + "\\temp\\libstdc++-6.dll", Properties.Resources.libstdc);

                        int ExitCode;
                        string error;
                        ProcessStartInfo ProcessInfo;
                        Process process;


                        if (File.Exists(Application.StartupPath + "\\temp\\banner.bin"))
                        {
                            File.Delete(Application.StartupPath + "\\temp\\banner.bin");
                        }
                        if (File.Exists(Application.StartupPath + "\\temp\\header.bin"))
                        {
                            File.Delete(Application.StartupPath + "\\temp\\header.bin");
                        }


                        //dsiware check
                        byte[] temp = File.ReadAllBytes(ndsFile);
                        bool isDsiEnhanced = false;

                        if (SubArray(temp, 0x12, 1).SequenceEqual(new byte[]{ 0x02 }))
                        {
                            isDsiEnhanced = true;
                        }

                        // if it's a normal ds game...
                        if (!isDsiEnhanced)
                        {

                            ProcessInfo = new ProcessStartInfo(Application.StartupPath + "\\temp\\ndstool.exe");
                            ProcessInfo.CreateNoWindow = true;
                            ProcessInfo.WorkingDirectory = Application.StartupPath + "\\temp";
                            ProcessInfo.UseShellExecute = false;
                            ProcessInfo.RedirectStandardError = true;
                            ProcessInfo.Arguments = "-x \"" + ndsFile + "\" -t banner.bin -h header.bin";
                            process = Process.Start(ProcessInfo);
                            process.WaitForExit();
                            error = process.StandardError.ReadToEnd();
                            ExitCode = process.ExitCode;
                            process.Close();


                            temp = File.ReadAllBytes(Application.StartupPath + "\\temp\\header.bin");
                            byte[] internalTitle = SubArray(temp, 0, 12);

                            byte[] productCode = SubArray(temp, 0xC, 4);

                            File.Delete(Application.StartupPath + "\\temp\\header.bin");

                            temp = File.ReadAllBytes(Application.StartupPath + "\\temp\\banner.bin");

                            byte[] bannerHeader = SubArray(temp, 0, 0x20);
                            byte[] icon = SubArray(temp, 0x20, 0x200);
                            byte[] pallete = SubArray(temp, 0x220, 0x20);
                            byte[] gameNameMulti5 = SubArray(temp, 0x240, 0x600);

                            File.Delete(Application.StartupPath + "\\temp\\banner.bin");
                          
                            if (radioButton1.Checked)
                            {
                                // Supercard DSTwo
                                temp = Properties.Resources.baseNDS;
                                temp = ReplaceByOffset(temp, internalTitle, 0x0);
                                temp = ReplaceByOffset(temp, bannerHeader, 0x38000);
                                temp = ReplaceByOffset(temp, icon, 0x38020);
                                temp = ReplaceByOffset(temp, pallete, 0x38220);
                                temp = ReplaceByOffset(temp, gameNameMulti5, 0x38240);
                            }
                            else
                            {
                                // R4i-SDHC 3DS RTS
                                temp = Properties.Resources.base2NDS;
                                temp = ReplaceByOffset(temp, internalTitle, 0x0);
                                temp = ReplaceByOffset(temp, bannerHeader, 0x4110);
                                temp = ReplaceByOffset(temp, icon, 0x4130);
                                temp = ReplaceByOffset(temp, pallete, 0x4330);
                                temp = ReplaceByOffset(temp, gameNameMulti5, 0x4350);
                            }
                            
                            
                            
                            File.WriteAllBytes(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.nds", temp);

                            File.Delete(Application.StartupPath + "\\temp\\ndstool.exe");
                            File.Delete(Application.StartupPath + "\\temp\\libstdc++-6.dll");


                            ProcessInfo = new ProcessStartInfo(Application.StartupPath + "\\temp\\make_cia.exe");
                            ProcessInfo.CreateNoWindow = true;
                            ProcessInfo.WorkingDirectory = Application.StartupPath + "\\temp";
                            ProcessInfo.UseShellExecute = false;
                            ProcessInfo.RedirectStandardError = true;
                            ProcessInfo.Arguments = "--srl=\"" + safeFileNameWithOutExension + "_injected.nds" + "\"";
                            process = Process.Start(ProcessInfo);
                            process.WaitForExit();
                            error = process.StandardError.ReadToEnd();
                            ExitCode = process.ExitCode;
                            process.Close();

                            // Inject Title ID directly in CIA
                            temp = File.ReadAllBytes(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.cia");

                            temp = ReplaceByOffset(temp, productCode, 0x2C20);
                            temp = ReplaceByOffset(temp, productCode, 0x2F50);


                            File.WriteAllBytes(fileFolder + safeFileNameWithOutExension + ".cia", temp);

                            File.Delete(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.nds");
                            File.Delete(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.cia");
                            File.Delete(Application.StartupPath + "\\temp\\make_cia.exe");
                            Directory.Delete(Application.StartupPath + "\\temp");

                            

                        }
                        else
                        {
                            //DS-i enhanced game
                            byte[] internalTitle = SubArray(temp, 0, 12);
                            byte[] productCode = SubArray(temp, 0xC, 4);


                            //Animated banner
                            byte[] bannerOffset = SubArray(temp, 0x68, 0x4);
                            long formattedBannerOffset = (long)BitConverter.ToUInt32(bannerOffset, 0);

                            
                            byte[] bannerSize = SubArray(temp, 0x208, 0x4);
                            long formattedBannerSize = (long)BitConverter.ToUInt32(bannerSize, 0);

                            if (formattedBannerSize == 0)
                            {
                                formattedBannerSize = 0x23C0;
                            }

                            byte[] wholeBanner = SubArray(temp, formattedBannerOffset, formattedBannerSize);


                            if (radioButton1.Checked)
                            {
                                // Supercard DSTwo
                                temp = Properties.Resources.baseNDS;
                                temp = ReplaceByOffset(temp, internalTitle, 0x0);
                                temp = ReplaceByOffset(temp, wholeBanner, 0x038000);
                                temp = ReplaceByOffset(temp, new byte[2] { 0xC0, 0x23 }, 0x208); //banner size
                            }
                            else
                            {
                                // R4i-SDHC 3DS RTS
                                temp = Properties.Resources.base2NDS;
                                temp = ReplaceByOffset(temp, internalTitle, 0x0);
                                temp = ReplaceByOffset(temp, wholeBanner, 0x4110);
                                temp = ReplaceByOffset(temp, new byte[2] { 0xC0, 0x23 }, 0x208); //banner size
                            }
                            
                            

                            File.WriteAllBytes(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.nds", temp);

                            File.Delete(Application.StartupPath + "\\temp\\ndstool.exe");
                            File.Delete(Application.StartupPath + "\\temp\\libstdc++-6.dll");

                            ProcessInfo = new ProcessStartInfo(Application.StartupPath + "\\temp\\make_cia.exe");
                            ProcessInfo.CreateNoWindow = true;
                            ProcessInfo.WorkingDirectory = Application.StartupPath + "\\temp";
                            ProcessInfo.UseShellExecute = false;
                            ProcessInfo.RedirectStandardError = true;
                            ProcessInfo.Arguments = "--srl=\"" + safeFileNameWithOutExension + "_injected.nds" + "\"";
                            process = Process.Start(ProcessInfo);
                            process.WaitForExit();
                            error = process.StandardError.ReadToEnd();
                            ExitCode = process.ExitCode;
                            process.Close();

                            //Inject Title ID directly in CIA
                            temp = File.ReadAllBytes(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.cia");
                            temp = ReplaceByOffset(temp, productCode, 0x2C20);
                            temp = ReplaceByOffset(temp, productCode, 0x2F50);

                            File.WriteAllBytes(fileFolder + safeFileNameWithOutExension + ".cia", temp);

                            File.Delete(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.nds");
                            File.Delete(Application.StartupPath + "\\temp\\" + safeFileNameWithOutExension + "_injected.cia");
                            File.Delete(Application.StartupPath + "\\temp\\base.nds");
                            File.Delete(Application.StartupPath + "\\temp\\make_cia.exe");
                            Directory.Delete(Application.StartupPath + "\\temp");

                        }


                        doneCIAs.Add(fileFolder + safeFileNameWithOutExension + ".cia");
                        progressBar1.Value += 1;
                        Application.DoEvents();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                    }
                }

                label2.Text = "";

                DialogResult dialogResult = MessageBox.Show("CIA(s) created at " + fileFolder + "\n"
                            + "Would you like to open the output folder?", "Done!", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    NativeMethods.OpenFolderAndSelectFiles(fileFolder, doneCIAs.ToArray());
                }
            }
                
        }

        public static byte[] FillWithZeroUntil(byte[] arr, int until)
        {
            byte[] res = new byte[until];

            for (int i = 0; i < until; ++i)
            {
                if (i < arr.Length)
                {
                    res[i] = arr[i];
                }
                else
                {
                    res[i] = 0;
                }

            }
            return res;
        }

        public byte[] ReplaceByOffset(byte[] dataToModify, byte[] dataWithReplacer, int startIndex)
        {
            long newSize = Math.Max(dataToModify.Length, (startIndex + dataWithReplacer.Length));
            List<byte> res = new List<byte>();

            for (int i = 0; i < newSize; ++i)
            {
                if (i < startIndex || i >= startIndex + dataWithReplacer.Length) { 
                    res.Add(dataToModify[i]);
                }
                else
                {
                    res.Add(dataWithReplacer[i - startIndex]);
                }
            }

            /*
            // asumo dataWithReplacer < dataToModify
            byte[] res = new byte[dataToModify.Length];

            Array.Copy(dataToModify, res, dataToModify.Length);

            for (int i = 0; i < dataWithReplacer.Length; ++i)
            {
                res[startIndex + i] = dataWithReplacer[i];
            }
            */

            byte[] res2 = new byte[newSize];

            res2 = res.ToArray();

            return res2;
        }

        public byte[] SubArray(byte[] data, long index, long length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://gbatemp.net/members/asdolo.389539/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://gbatemp.net/members/robz8.263852/");
        }


        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
