using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PagarMe;
using PagarMe.Mpos;

namespace Pagar.me
{
    public class PaymentProcessor
    {
        private readonly SerialPort _port;
        private readonly Mpos _mpos;

        public PaymentProcessor(string device)
        {
            _port = new SerialPort(device, 140000, Parity.None, 8, StopBits.One);
            _port.Open();

            /* Guardamos arquivos de cache no diretório C:\\Storage\\. É *obrigatório* colocar a última barra ao final do path! */
            _mpos = new Mpos(_port.BaseStream, "SUA ENCRYPTION KEY", "C:\\Storage\\");

            /* Os eventos abaixo podem ser usados para controlar o fluxo de 
               sua aplicação
            */
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
            _mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
            _mpos.Errored += (sender, e) => Console.WriteLine("I GOT ERROR {0}", e);
            _mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
            _mpos.FinishedTransaction += (sender, e) => Console.WriteLine("FINISHED TRANSACTION!");

        }


        public async Task Initialize()
        {
            await _mpos.Initialize();

            // Você pode solicitar o download das tableas EMV neste momento
            await _mpos.SynchronizeTables(false);
        }



        public async Task Pay(int amount)
        {
            var result = await _mpos.ProcessPayment(amount, null, PagarMe.Mpos.PaymentMethod.Credit);

            Console.WriteLine("CARD HASH = " + result.CardHash);

            var transaction = new Transaction
            {
                CardHash = result.CardHash,
                Amount = amount
            };

            await transaction.SaveAsync();

            Console.WriteLine("Transaction ARC = " + transaction.AcquirerResponseCode + ", Id = " + transaction.Id);
            Console.WriteLine("ACQUIRER RESPONSE CODE = " + transaction.AcquirerResponseCode);
            Console.WriteLine("EMV RESPONSE = " + transaction["card_emv_response"]);

            int x = Int32.Parse(transaction.AcquirerResponseCode);
            object obj = transaction["card_emv_response"];
            string response = obj == null ? null : obj.ToString();
            await _mpos.FinishTransaction(true, x, (string)obj);
            await _mpos.Close();
        }
    }
}
