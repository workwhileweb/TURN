using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Turn
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Usage : exe [<IP:Port> <TurnedIP:TurnedPort>]...");
                return;
            }

            var rules = new List<Tuple<IPEndPoint, IPEndPoint>>();
            for (var i = 0; i < args.Length -1; i += 2)
            {
                var split0 = args[i + 0].Split(':');
                var split1 = args[i + 1].Split(':');
                var addr0 = IPAddress.Parse(split0[0]);
                var addr1 = IPAddress.Parse(split1[0]);
                var port0 = int.Parse(split0[1]);
                var port1 = int.Parse(split1[1]);
                var end0 = new IPEndPoint(addr0, port0);
                var end1 = new IPEndPoint(addr1, port1);
                var rule = new Tuple<IPEndPoint, IPEndPoint>(end0, end1);
                rules.Add(rule);
            }

            var turn = new Turn(rules);
            turn.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}