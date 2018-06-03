using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Turn
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("    exe [<IP:Port> <TurnedIP:TurnedPort>]...");
            }

            List<Tuple<IPEndPoint, IPEndPoint>> rules = new List<Tuple<IPEndPoint, IPEndPoint>>();
            for (int i = 0; i + 1 < args.Length; i += 2)
            {
                string[] split0 = args[i + 0].Split(':');
                string[] split1 = args[i + 1].Split(':');
                IPAddress addr0 = IPAddress.Parse(split0[0]);
                IPAddress addr1 = IPAddress.Parse(split1[0]);
                int port0 = int.Parse(split0[1]);
                int port1 = int.Parse(split1[1]);

                rules.Add(new Tuple<IPEndPoint, IPEndPoint>(
                    new IPEndPoint(addr0, port0),
                    new IPEndPoint(addr1, port1)
                    ));

            }

            Turn turn = new Turn(rules);
            turn.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
