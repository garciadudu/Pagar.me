using PagarMe.Mpos;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagar.me
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            var payment = new PaymentProcessor("COM1");

            payment.Pay(1000).Wait();
        }
        
    }
}
