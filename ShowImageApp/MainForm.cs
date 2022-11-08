using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShowImageApp.Models;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Threading;
using System.Runtime.InteropServices;

namespace ShowImageApp
{
    public partial class MainForm : Form
    {
        int TimeInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimeInterval"]);
        string IPAddress = ConfigurationManager.AppSettings["IPAddress"];
        int PortNo = Convert.ToInt32(ConfigurationManager.AppSettings["PortNo"]);
        Socket listener;
        List<ClientInfo> clientlist = new List<ClientInfo>();
        public static List<ClientInfo> chooseList = new List<ClientInfo>();
        ClientInfo selectClient = new ClientInfo();
        List<PictureBox> picboxList = new List<PictureBox>();
        List<Socket> _clientSockets = new List<Socket>();
        List<Form> formList = new List<Form>();
        bool existFlag = false;
        Socket client = null;
        System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        public MainForm()
        {
            InitializeComponent();

            var config = (ClientConfigSection)System.Configuration.ConfigurationManager.GetSection("ClientListConfig");
            if (config != null)
            {
                foreach (Client clientItem in config.Clients)
                {
                    clientlist.Add(new ClientInfo { name = clientItem.Name, ipaddress = clientItem.IpAddress});
                }
                cboClientList.DataSource = clientlist;
                cboClientList.DisplayMember = "name";
                cboClientList.ValueMember = "ipaddress";
            }
            IPHostEntry host = Dns.GetHostEntry(IPAddress);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PortNo);
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(clientlist.Count);            
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            selectClient = (ClientInfo)cboClientList.SelectedItem;
            ClientInfo existData = chooseList.Find(x => x.ipaddress == selectClient.ipaddress);
           
            if (existData == null)
            {
                foreach (Socket existSocket in _clientSockets)
                {
                    IPEndPoint endpoint = (IPEndPoint)existSocket.RemoteEndPoint;
                    if (endpoint.Address.ToString().Split('%')[0] == selectClient.ipaddress.ToString())
                    {
                        existFlag = true;
                        break;
                    }
                    else
                    {
                        existFlag = false;
                    }
                }
                if(!existFlag)
                {
                    try
                    {
                        listener.BeginAccept(new AsyncCallback(AcceptCallBack), null);
                    }                   
                    catch (ArgumentNullException ane)
                    {
                        MessageBox.Show("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("ArgumentNullException1111 : {0}", se.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unexpected exception : {0}", ex.ToString());
                    }
            }
                foreach (Socket existSocket in _clientSockets)
                {
                    IPEndPoint endpoint = (IPEndPoint)existSocket.RemoteEndPoint;
                    if (endpoint.Address.ToString().Split('%')[0] == selectClient.ipaddress.ToString())
                    {
                        byte[] message = Encoding.ASCII.GetBytes("Please Send Data");
                        client.Send(message);
                        chooseList.Add(new ClientInfo { ipaddress = selectClient.ipaddress, name = selectClient.name });
                        if (!(client.Poll(1, SelectMode.SelectRead) && client.Available == 0))
                            Process(client, selectClient);

                    }
                }
            }
        }

        private void AcceptCallBack(IAsyncResult AR)
        {
             client = listener.EndAccept(AR);
            _clientSockets.Add(client);
        }


        public void Process(Socket client, ClientInfo chooseInfo)
        {
            Socket s = client;
            Form form = new Form();
            form.Size = new Size(1366, 768);
            form.Name = "frm_" + selectClient.name;
            PictureBox pictureBox1 = new System.Windows.Forms.PictureBox();
            pictureBox1.Size = new Size(1079, 442);
            pictureBox1.Name = "pic_" + selectClient.name;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            picboxList.Add(pictureBox1);
            Panel panel1 = new System.Windows.Forms.Panel();
            panel1.Dock = DockStyle.Fill;
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox1);
            timer1.Interval = (TimeInterval);
            timer1.Enabled = true;
            timer1.Tick += new EventHandler((sender, e) =>
            {
                ReceiveData(sender, e, chooseInfo);
            });
            timer1.Start();
            form.Controls.Add(panel1);
            formList.Add(form);
            form.Show();
        }
      
        private void ReceiveData(object sender, EventArgs e,ClientInfo chooseInfo)
        {
            try
            {
                foreach (Socket connClient in _clientSockets)
                {
                    IPEndPoint endpoint = (IPEndPoint)connClient.RemoteEndPoint;
                    if (endpoint.Address.ToString().Split('%')[0] == chooseInfo.ipaddress)
                    {
                        client = connClient;
                        PictureBox picBox = picboxList.Where(x => x.Name == "pic_" + chooseInfo.name).SingleOrDefault();
                        string data = string.Empty;
                        Thread.Sleep(500);
                        if (client.Available > 0)
                        {
                            byte[] bytes = null;
                            bytes = new byte[client.Available];
                            int bytesRec = client.Receive(bytes);
                            picBox.Image = Image.FromStream(new MemoryStream(bytes));
                            byte[] message = Encoding.ASCII.GetBytes("Continue");
                            client.Send(message);
                        }
                    }
                }
            }
            catch (ArgumentNullException ane)
            {
                MessageBox.Show("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show("ArgumentNullException1111 : {0}", se.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected exception : {0}", ex.ToString());
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            selectClient = (ClientInfo)cboClientList.SelectedItem;
            ClientInfo existData = chooseList.Find(x => x.ipaddress == selectClient.ipaddress);
            PictureBox picBox = picboxList.Find(x => x.Name == "pic_" + selectClient.name);
            Form frm = formList.Find(x => x.Name == "frm_" + selectClient.name);
            if (existData != null)
            {
                foreach(Socket client in _clientSockets)
                {
                    IPEndPoint endpoint = (IPEndPoint)client.RemoteEndPoint;
                    string address = endpoint.Address.ToString().Split('%')[0];
                    if(existData.ipaddress.ToString()== address)
                    {
                        byte[] message = Encoding.ASCII.GetBytes("Please Stop Sending Data");
                        client.Send(message);                       
                        client.Close();
                        chooseList.Remove(existData);
                        _clientSockets.Remove(client);
                        picboxList.Remove(picBox);
                        formList.Remove(frm);
                        frm.Close();
                        break;
                    }
                }
                if (_clientSockets.Count == 0)
                {
                    client.Close();
                    timer1.Stop();
                }
            }
            else
            {
                MessageBox.Show("This Client is DisConnected.");
            }
        }        
    }
}
