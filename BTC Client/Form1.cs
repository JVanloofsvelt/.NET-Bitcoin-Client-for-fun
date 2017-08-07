using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace BTCClient
{
    using Messages;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public partial class Form1 : Form
    {
        static readonly ReadOnlyCollection<string> DNSSeedHosts = new ReadOnlyCollection<string>(new[]
        {
            "bitseed.xf2.org",
            "dnsseed.bluematt.me",
            "seed.bitcoin.sipa.be",
            "dnsseed.bitcoin.dashjr.org",
            "seed.bitcoinstats.com"
        });

        readonly HashSet<IPEndPoint> Nodes = new HashSet<IPEndPoint>();

        int ConnectionCount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadDNSSeedsAsync().ContinueWith(t =>
                Log($"{Nodes.Count} DNS seeds found")
            );
        }

        void Log(object log)
        {
            if (textBox1.InvokeRequired)
                textBox1.Invoke(new Action<object>(Log), log);
            else
                textBox1.AppendText(log.ToString() + Environment.NewLine);
        }

        void UpdateTitle(object title)
        {
            try
            {

                if (this.InvokeRequired)
                    this.Invoke(new Action<object>(UpdateTitle), title);
                else
                    this.Text = title.ToString();
            }
            catch (ObjectDisposedException)
            { }
        }

        async void OnNewNodeAdded(IPEndPoint node)
        {
            using (var client = new TcpClient(node.AddressFamily))
            {
                try
                {
                    await client.ConnectAsync(node.Address, node.Port).ConfigureAwait(false);
                }
                catch (SocketException)
                {
                    // TODO: Track source of unreachable nodes, keep in mind IPv6 nodes may be unroutable
                    // Log($"{node.Address}:\t Failed to connect");
                    lock(Nodes)
                        Nodes.Remove(node);
                    return;  // Shit happens
                }
                catch (Exception exception)
                {
                    Log($"Error: {exception.Message}");
                    lock (Nodes)
                        Nodes.Remove(node);
                    return;
                }

                Interlocked.Increment(ref ConnectionCount);
                
                UpdateTitle($"Connected to {ConnectionCount} peers");

                VersionMessage versionMessage;

                using (var stream = client.GetStream())
                {
                    

                    try
                    {
                        await SendVersionAsync(stream, node).ConfigureAwait(false);

                        while (true)
                        {
                            Message message;

                            try
                            {
                                message = await Message.ReadMessageAsync(stream).ConfigureAwait(false);
                            }
                            catch (Exception exception) when (exception.Message.Contains("unknown type"))
                            {
                                continue;
                            }

                            if (message is VersionMessage)
                            {
                                versionMessage = (message as VersionMessage);

                                if (!versionMessage.UserAgent.Contains("/Satoshi:"))
                                    Log($"{node.Address}:\tnon-satoshi version:\t{versionMessage.UserAgent}");

                                await SendVerAckAsync(stream, node).ConfigureAwait(false);
                            }

                            if (message is VerAckMessage)
                            {
                                //Log($"{node.Address}:\t{message}");
                                await SendGetAddrAsync(stream).ConfigureAwait(false);
                            }

                            if (message is PingMessage)
                            {
                                //Log($"{node.Address}:\t{message}");
                                await SendPongAsync(stream, node, message as PingMessage).ConfigureAwait(false);
                            }

                            if (message is AddrMessage)
                            {
                                var nodeInfos = (message as AddrMessage).NodeInfos;
                                bool isOwnAddress = nodeInfos.Length == 1 && (nodeInfos.First().EndPoint.Equals(node) || nodeInfos.First().EndPoint.AddressFamily == AddressFamily.InterNetworkV6);

                                if (!isOwnAddress)  // TODO: keep track of nodes that do not send their own address
                                    Log($"{node.Address}:\t{message} containing {nodeInfos.Length} records (First is {nodeInfos.First().EndPoint.Address})");


                                foreach (var endpoint in nodeInfos.Select(i => i.EndPoint))
                                {
                                    TryAddPeer(endpoint);
                                }
                            }

                            if (message is AlertMessage)
                            {
                                var alertMessage = (message as AlertMessage);

                                if (!alertMessage.IsFinalAlert)
                                {
                                    Log($"{node.Address}:\t{alertMessage.StatusBar}");
                                    MessageBox.Show(alertMessage.StatusBar, node.Address.ToString());
                                }
                            }

                            if (message is InventoryMessage)
                            {
                                // TODO: Why are they sending me unsolicited block hashes, are these new blocks? 
                                Log($"{node.Address}:\t{message} containing {(message as InventoryMessage).Objects.Length} object hashes");
                            }
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        Log($"{node.Address}:\tRemote closed connection");
                    }
                    catch (IOException exception)
                    {
                        Log($"{node.Address}:\tIOException: {exception.Message}");
                    }
                    catch  (Exception exception)
                    {
                        Log($"{node.Address}:\t{exception.Message}");
                    }

                    Interlocked.Decrement(ref ConnectionCount);
                    UpdateTitle($"Connected to {ConnectionCount} peers");

                    lock (Nodes)
                        Nodes.Remove(node);
                }
            }
        }

        Task SendGetAddrAsync(NetworkStream stream)
        {
            var message = new GetAddrMessage().Serialize();
            return stream.WriteAsync(message, 0, message.Length);
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="address">Address of node receiving this message.</param>
        Task SendVersionAsync(NetworkStream stream, IPEndPoint remote)
        {
            var message = new VersionMessage(
                protocolVersion: 70014,
                timestamp: DateTime.Now,
                from: new IPEndPoint(IPAddress.Parse("194.78.240.98"), 8333),  // TODO: Get own IP dynamically
                to: remote,
                nonce: 0,
                userAgent: "/Satoshi:0.13.1/",
                startHeight: 443112,
                relay: false
            ).Serialize();

            return stream.WriteAsync(message, 0, message.Length);
        }

        Task SendPongAsync(NetworkStream stream, IPEndPoint remote, PingMessage ping)
        {
            var message = new PongMessage(ping.Nonce).Serialize();
            return stream.WriteAsync(message, 0, message.Length);
        }

        Task SendVerAckAsync(NetworkStream stream, IPEndPoint remote)
        {
            var message = new VerAckMessage().Serialize();
            return stream.WriteAsync(message, 0, message.Length);
        }

        Task LoadDNSSeedsAsync()
        {
            var dnsRequests = new List<Task>(DNSSeedHosts.Count);

            foreach (var hostname in DNSSeedHosts)
            {
                var request = Dns.GetHostAddressesAsync(hostname).ContinueWith(t =>
                {
                    IPAddress[] addresses;

                    try
                    {
                        addresses = t.Result;
                    }
                    catch (Exception exception)
                    {
                        if (exception is AggregateException)
                            exception = exception.InnerException;

                        Log($"Failed to get host addresses for '{hostname}': {exception.Message}");
                        return;
                    }

                    foreach (var address in addresses)
                    {
                        var endpoint = new IPEndPoint(address, 8333);
                        TryAddPeer(endpoint);
                    }
                });

                dnsRequests.Add(request);
            }

            return Task.WhenAll(dnsRequests);
        }

        void TryAddPeer(IPEndPoint endpoint)
        {
            try
            {
                lock (Nodes)
                {
                    if (!Nodes.Contains(endpoint))
                    {
                        Nodes.Add(endpoint);
                        OnNewNodeAdded(endpoint);
                        Debug.WriteLine($"Connected to {Nodes.Count} nodes");
                    }
                }
            }
            catch (Exception exception)
            {
                ;
            }
        }
    }
}
