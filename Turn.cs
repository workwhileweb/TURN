using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Turn
{
    class Turn
    {
        protected List<Tuple<IPEndPoint, IPEndPoint>> Rules { get; }

        protected List<TcpListener> TcpListeners { get; } = new List<TcpListener>();

        protected bool Started { get; set; } = false;

        public Turn(IEnumerable<Tuple<IPEndPoint, IPEndPoint>> rules)
        {
            Rules = new List<Tuple<IPEndPoint, IPEndPoint>>(rules);
        }

        /// <summary>
        /// Start.
        /// </summary>
        public void Start()
        {
            if (!Started)
            {
                Dictionary<TcpListener, IPEndPoint> dict = new Dictionary<TcpListener, IPEndPoint>();

                TcpListeners.Clear();
                foreach (var turn in Rules)
                {
                    TcpListener tl = new TcpListener(turn.Item1);
                    tl.Start();
                    TcpListeners.Add(tl);
                    dict.Add(tl, turn.Item2);
                }

                Started = true;
                foreach (var tl in TcpListeners)
                {
                    Loop(tl, dict[tl]);
                }
            }
        }

        /// <summary>
        /// Close All Connections And Stop.
        /// </summary>
        public void Stop()
        {
            if (Started)
            {
                foreach (var tl in TcpListeners)
                {
                    try
                    {
                        tl.Stop();
                    }
                    catch { }
                }

                Started = false;
            }
        }

        /// <summary>
        /// Accept Connections To tl, And Redirect Data To target.
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="target"></param>
        protected async void Loop(TcpListener tl, IPEndPoint target)
        {
            while (Started)
            {
                TcpClient tc0 = null;
                TcpClient tc1 = null;
                try
                {
                    tc0 = await tl.AcceptTcpClientAsync().ConfigureAwait(false);
                    tc1 = new TcpClient();
                    await tc1.ConnectAsync(target.Address, target.Port).ConfigureAwait(false);

                    Connect(tc0, tc1);
                    Connect(tc1, tc0);

#if DEBUG
                    Console.Write((tl.LocalEndpoint as IPEndPoint).ToString());
                    Console.Write(" ");
                    Console.Write(target.ToString());
                    Console.Write(" ");
                    Console.WriteLine("Connection Established.");
#endif
                }
                catch
                {
                    if (tc0 != null)
                    {
                        try
                        {
                            tc0.Close();
                        }
                        catch { }
                    }
                    if (tc1 != null)
                    {
                        try
                        {
                            tc1.Close();
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Read Data From tc0 And Write To tc1.
        /// </summary>
        /// <param name="tc0"></param>
        /// <param name="tc1"></param>
        protected async void Connect(TcpClient tc0, TcpClient tc1)
        {
            Stream s0 = tc0.GetStream();
            Stream s1 = tc1.GetStream();
            byte[] buffer = new byte[65536];
            try
            {
                while (true)
                {
                    int count = await s0.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    await s1.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                }
            }
            catch
            {
                try
                {
                    tc0.Close();
#if DEBUG
                    Console.Write(tc0.Client.LocalEndPoint.ToString());
                    Console.Write(" ");
                    Console.WriteLine("Connection Closed.");
#endif
                }
                catch { }

                try
                {
                    tc1.Close();
#if DEBUG
                    Console.Write(tc1.Client.LocalEndPoint.ToString());
                    Console.Write(" ");
                    Console.WriteLine("Connection Closed.");
#endif
                }
                catch { }
            }
        }
    }
}
