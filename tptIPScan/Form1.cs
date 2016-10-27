using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace tptIPScan
{
    public partial class Form1 : Form
    {
        public string[] ipList = new string[254];
        public Dictionary<int, Thread> ipThreads;
        public int ipsUp = 0;
        public int threadsDone = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            generateIPList();
            generateGridList();
            startIPScan();
        }

        public void startIPScan()
        {
            int i = 0;
            threadsDone = 0;
            ipsUp = 0;
            ipThreads = new Dictionary<int, Thread>();
            foreach (string ip in ipList)
            {
                ParameterizedThreadStart tStart = new ParameterizedThreadStart(scanThread);
                ipThreads[i] = new Thread(tStart);
                object objarr = new object[2] { i, ip };
                ipThreads[i].Start(objarr);
                i++;
            }
        }

        public void scanThread(object args)
        {
            Array argArray = new object[2];
            argArray = (Array)args;

            int id = (int)argArray.GetValue(0);
            string ipa = (string)argArray.GetValue(1);
            
            IPAddress ipaddr = IPAddress.Parse(ipa);
            bool pingable = false;
            long delay;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(ipaddr);
                pingable = reply.Status == IPStatus.Success;
                if (pingable) {
                    delay = reply.RoundtripTime;
                    listView1.Items[id].Text = "UP!";
                    ipsUp++;
                    listView1.Items[id].SubItems[3].Text = Convert.ToString(delay) + "ms";
                    try
                    {
                        IPHostEntry hostInfo = Dns.GetHostEntry(ipa);
                        if (hostInfo != null)
                        {
                            listView1.Items[id].SubItems[2].Text = hostInfo.HostName;
                        }
                        else
                        {
                            listView1.Items[id].SubItems[2].Text = "N/A";
                        }
                    }
                    catch(Exception)
                    {
                        listView1.Items[id].SubItems[2].Text = "N/A";
                    }
                }
                else
                {
                    listView1.Items[id].Text = "DOWN";

                }
            } catch (PingException)
            {
                listView1.Items[id].Text = "DOWN";
            }
            threadsDone++;
            if (threadsDone > 252)
            {
                progressBar1.Value = 254;
                label1.Text = Convert.ToString(ipsUp) + " Addresses UP";
            }
            else
            {
                label1.Text = Convert.ToString(threadsDone) + " of 254 Addresses Scanned";
                progressBar1.Maximum = 254;
                progressBar1.Value = threadsDone;
            }
        }

        public void generateIPList()
        {
            Array.Clear(ipList, 0, ipList.Length);
            for (int i = 0; i < 254; i++)
            {
                ipList[i] = ipRange.Text + "." + Convert.ToString(i+1);
            }
        }

        public void generateGridList()
        {
            listView1.Items.Clear();
            foreach (string ip in ipList)
            {
                ListViewItem itm = new ListViewItem();
                itm.Text = "Trying";
                itm.SubItems.Add(ip);
                itm.SubItems.Add("");
                itm.SubItems.Add("");
                listView1.Items.Add(itm);
            }
        }
    }
}
