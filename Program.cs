// Program.cs

using System;
using NBitcoin;
using QBitNinja.Client;

namespace StratisProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Network network = Network.TestNet;

            /*
            Console.WriteLine("treasurer key: " +  treasurer.GetWif(network));

            var alice = new Key();
            var bob = new Key();

            Console.WriteLine("Alice     key: " + alice.GetWif(network));
            Console.WriteLine("Bob       key: " + bob.GetWif(network));
             */

            // Replace the constructor argument with your own noted secrets from the last step
            var treasurer = new BitcoinSecret("cSMW1AvufDX5NG3Gy4ktxx1yBWEKGU5r3p6NbXuCK6LkjtH12FLe");
            var alice = new BitcoinSecret("cSWyMTFYWBVjv7tzvpM1rQgMUwccs5yGgi9ZiHzP978nohWEDH9w");
            var bob = new BitcoinSecret("cUWedW9rd7HnsTrks1HDwUTTWRDsMvHz2kY6dbv5pH4jgHU1pN9H");

            Console.WriteLine("treasurer key: " + treasurer.PrivateKey.GetWif(network));
            Console.WriteLine("Alice     key: " + alice.PrivateKey.GetWif(network));
            Console.WriteLine("Bob       key: " + bob.PrivateKey.GetWif(network));

            var scriptPubKey = PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey });

            Console.WriteLine("PubKey script: " + scriptPubKey);

            var redeemScript = PayToMultiSigTemplate
            .Instance
            .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey });

            var paymentScript = redeemScript.PaymentScript;

            Console.WriteLine("paymentScript: " + paymentScript);

            Console.WriteLine("multi-sig address: " + redeemScript.Hash.GetAddress(network));

            var client = new QBitNinjaClient(network);

            // Replace "0acb..." with the txId from the block explorer from the faucet
            // If the faucet doesn't work, look up the redeemScript.Hash address on the explorer
            // You will find a txId related to that address. If no tx appears, try another faucet.

            var receiveTransactionId = uint256.Parse("0e57dc8dce735ce5fdb4401ecf59f9470a555e14470a5bba7ff99a7c89b68000");
            var receiveTransactionResponse = client.GetTransaction(receiveTransactionId).Result;

            Console.WriteLine(receiveTransactionResponse.TransactionId);
            Console.WriteLine(receiveTransactionResponse.Block.Confirmations);


            var receivedCoins = receiveTransactionResponse.ReceivedCoins;
            OutPoint outpointToSpend = null;
            ScriptCoin coinToSpend = null;
            foreach (var c in receivedCoins)
            {
                try
                {
                    coinToSpend = new ScriptCoin(c.Outpoint, c.TxOut, redeemScript);
                    outpointToSpend = c.Outpoint;
                    break;
                }
                catch { }
            }
            if (outpointToSpend == null)
                throw new Exception("TxOut doesn't contain any our ScriptPubKey");
            Console.WriteLine("We want to spend outpoint #{0}", outpointToSpend.N + 1);

            var lucasAddress = BitcoinAddress.Create("mv4rnyY3Su5gjcDNzbMLKBQkBicCtHUtFB", network);

            TransactionBuilder builder = network.CreateTransactionBuilder();

            var minerFee = new Money(0.0005m, MoneyUnit.BTC);
            var txInAmount = (Money)receivedCoins[(int)outpointToSpend.N].Amount;
            var sendAmount = txInAmount - minerFee;

            Transaction unsigned =
                builder
                    .AddCoins(coinToSpend)
                    .Send(lucasAddress, sendAmount)
                    .SetChange(lucasAddress, ChangeType.Uncolored)
                    .BuildTransaction(sign: false);

            // Alice signs it
            Transaction aliceSigned =
                builder
                    .AddCoins(coinToSpend)
                    .AddKeys(alice)
                    .SignTransaction(unsigned);

            Transaction bobSigned =
                builder
                    .AddCoins(coinToSpend)
                    .AddKeys(bob)
                    .SignTransaction(aliceSigned);

            Transaction fullySigned =
                builder
                    .AddCoins(coinToSpend)
                    .CombineSignatures(aliceSigned, bobSigned);

            Console.WriteLine(fullySigned);

            var broadcastResponse = client.Broadcast(fullySigned).Result;
            if (!broadcastResponse.Success)
            {
                Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                Console.WriteLine(fullySigned.GetHash());
            }
        }
    }
}