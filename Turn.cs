using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Turn
{
    public class Turn
    {
        public Action<object>? Log;

        protected List<Tuple<IPEndPoint, IPEndPoint>> Rules { get; }

        protected List<TcpListener> TcpListeners { get; } = new();

        protected bool Started { get; set; }

        public Turn(IEnumerable<Tuple<IPEndPoint, IPEndPoint>> rules)
        {
            Rules = new List<Tuple<IPEndPoint, IPEndPoint>>(rules);
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start()
        {
            if (Started) return;

            var dict = new Dictionary<TcpListener, IPEndPoint>();

            TcpListeners.Clear();

            foreach (var turn in Rules)
            {
                var tl = new TcpListener(turn.Item1);
                TcpListeners.Add(tl);
                dict.Add(tl, turn.Item2);
                tl.Start();
            }

            Started = true;

            foreach (var tl in TcpListeners)
            {
                Loop(tl, dict[tl]);
            }
        }

        /// <summary>
        /// Close All Connections And Stop
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Stop()
        {
            if (!Started) return;

            foreach (var tl in TcpListeners)
            {
                try
                {
                    tl.Stop();
                }
                catch
                {
                    // ignored
                }
            }

            Started = false;
        }

        /// <summary>
        /// Accept Connections To tl, And Redirect Data To target
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="target"></param>
        protected async void Loop(TcpListener tl, IPEndPoint target)
        {
            while (Started)
            {
                TcpClient? tc0 = null;
                TcpClient? tc1 = null;

                try
                {
                    tc0 = await tl.AcceptTcpClientAsync().ConfigureAwait(false);
                    
                    tc1 = new TcpClient();
                    await tc1.ConnectAsync(target.Address, target.Port).ConfigureAwait(false);

                    Connect(tc0, tc1);
                    Connect(tc1, tc0);

                    Log?.Invoke(new
                    {
                        Connection = "Established",
                        IPAddress = (tl.LocalEndpoint as IPEndPoint)?.ToString(),
                        target,
                    });
                }
                catch
                {
                    try
                    {
                        tc0?.Close();
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        tc1?.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// Read Data From tc0 And Write To tc1
        /// </summary>
        /// <param name="tc0"></param>
        /// <param name="tc1"></param>
        protected async void Connect(TcpClient tc0, TcpClient tc1)
        {
            Stream s0 = tc0.GetStream();
            Stream s1 = tc1.GetStream();

            var buffer = new byte[ushort.MaxValue];

            try
            {
                while (Started)
                {
                    var count = await s0.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    await s1.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                }
            }
            catch
            {
                try
                {
                    tc0.Close();
                    Log?.Invoke(new { Closed = tc0.Client.LocalEndPoint });
                }
                catch
                {
                    // ignored
                }

                try
                {
                    tc1.Close();
                    Log?.Invoke(new { Closed = tc1.Client.LocalEndPoint });
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}