# Building Blocks
###### BostonHacks 2018 Stratis x FinTechBU Workshop: Writing a Transaction
#### Writing directly to chain


### Background
This workshop is designed for anyone with limited technical background. Your hand will be held through
* writing a custom multi-signature transaction
* broadcasting it to blockchain
Our ultimate goal is to team with partners and write a 2-of-3 multisignature transaction. We will discuss the scenarios where such a transaction and others may benefit organizations' financing

Huge thanks to everyone who contributed to [Programming the Blockchain in C#](https://github.com/ProgrammingBlockchain/ProgrammingBlockchain). Much of this workshop comes from that text.

# Let's get started

Who's familiar with `Terminal`? 

[Install .NET Core as documented here](https://www.microsoft.com/net/core#windowsvs2017).

[Install VSCode](https://code.visualstudio.com/download), or use Visual Studio, or your favorite text editor.
If you choose to use another editor, I highly recommned you use code completion or IDE features like [omnisharp for sublime](https://github.com/OmniSharp/omnisharp-sublime). Without it, it's going to be easy to make mistakes

**I highly recommend you type all of the commands and code rather than copy + paste. You'll more quickly get a feel for the library and be able to figure out where your mistakes are. Remember Learn Python the Hard Way.**

# Create a new project

Enter the following commands in your command line:
```console
mkdir MyProject
cd MyProject
dotnet new console
dotnet add package NBitcoin
dotnet restore
```

Then edit Program.cs:
```cs
// Program.cs

using System;
using NBitcoin;

namespace
{
    class Program
    {
        static void Main(string[] args)
        {
	    Network network = Network.TestNet;

	    var treasurer = new Key();
		// Fun Fact: WIF is Wallet Imput Format - a base58 representation of your key
            Console.WriteLine("treasurer key: " +  treasurer.GetWif(network));
            var alice = new Key();
            var bob = new Key();
            Console.WriteLine("Alice     key: " + alice.GetWif(network));
            Console.WriteLine("Bob       key: " + bob.GetWif(network));
        }
    }
}
```
Run it in console:
```console
dotnet run
```
This program generates a 3 bitcoin secret keys ("sk") on our the defined network. Open a new file & **write them down**. We'll need them in the next steps.

```
// Keys.txt

// Just example keys. Yours will of course be different.

T sk: cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctVAg5iy

A sk: cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctzAg5iy

B sk: cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctqAg5iy
```

We're going to need to record that key so we it can sign for coins. Replace your existing Main method with the following. Substitute your keys **These keys won't work for you**.

```cs
// Program.cs

static void Main(string[] args)
{
    Network network = Network.TestNet;
    // Replace the key constructor with your own secret from the last step here. This is the treasurer's sk
    var treasurer = new BitcoinSecret("cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctVAg5iy");
    Console.WriteLine("treasurer key: " + treasurer.GetWif(network));
    var alice = new BitcoinSecret("cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctVAg5iy");
    var bob = new BitcoinSecret("cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctVAg5iy");
    Console.WriteLine("Alice     key: " + alice.GetWif(network));
    Console.WriteLine("Bob       key: " + bob.GetWif(network));
    // I like big bits and I cannot lie
    Console.WriteLine("Address: " + treasurer.GetAddress());
}
```

```console
dotnet run
```

>❔: When you run this program, is the WalletInputFormat sk logged the same? Why?

# Writing a Multisig Transaction
Bitcoin allows us to have shared ownership over coins with multi-signature transactions or multisig for short. Here we demo how bitcoin transactions are really smart contracts executed on a distributed computer.

In order to demonstrate this shared ownership we will create a ```ScriptPubKey``` that represents an **m-of-n multisig**. This means that in order to spend the coins, **m** number of private keys will be needed to sign the spending transaction out of the **n** number of different public keys provided.

Let’s create a multi-sig tx with Bob, Alice and our treasurer, where 2 of the 3 of them need to sign a transaction in order to spend a coin.  

Add at the top:
```cs
// Program.cs

using System.Linq;

// ... After what we already wrote

var scriptPubKey = PayToMultiSigTemplate
    .Instance
    .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey });

Console.WriteLine(scriptPubKey);
```  

Run it:
```console
dotnet run
```

The program now generates this script which you can use as a public key (coin destination address):
```
2 0282213c7172e9dff8a852b436a957c1f55aa1a947f2571585870bfb12c0c15d61 036e9f73ca6929dec6926d8e319506cc4370914cd13d300e83fd9c3dfca3970efb 0324b9185ec3db2f209b620657ce0e9a792472d89911e0ac3fc1e5b5fc2ca7683d 3 OP_CHECKMULTISIG
```  

As you can see, the ```scriptPubkey``` has the following form: ```<sigsRequired> <pubkeys…> <pubKeysCount> OP_CHECKMULTISIG```  

[scriptPubKey](https://bitcoin.org/en/glossary/pubkey-script) is a Tx output which specifies conditions that must be satisfied in order to spend the value of the output. In this multi-sig transaction, we require 2-of-3 specified pubkeys to sign as condition to spend. 

# P2SH 2-of-3 Transaction

This PubKey Script (scriptPubKey), though valid, doesn't look very much like a typical wallet-friendly base58 address. Luckily, we can run it through a function so your wallet can't tell it apart from any other transaction.

```cs
// Program.cs (cont.)

var paymentScript = PayToMultiSigTemplate
    .Instance
    .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey }).PaymentScript;

Console.WriteLine(paymentScript);
```

Run it:
```console
dotnet run
```

And view the new output.
```
OP_HASH160 57b4162e00341af0ffc5d5fab468d738b3234190 OP_EQUAL
```  

This output hash represents the hash of the previous multi-sig script (containing `OP_CHECKMULTISIG`)

Since it's a hash, we can easily convert it to a base58 bitcoin address with the following snippet:

```cs
// Program.cs (cont.)

Console.WriteLine(paymentScript.Hash.GetAddress(network);
```

Excellent! Now we can load it up the same we would any wallet.

---------------------------------

The process for signing it (in order to be able to spend it) is a thus little more complicated than just calling ```Transaction.Sign```.

Later we will talk more deeply about the subject but for now let’s use the ```TransactionBuilder``` to sign the transaction.

Imagine the multisig ```scriptPubKey``` received a coin in a transaction called ```received```:

```cs
// Program.cs (cont.)

var received = Transaction.Create(network)
received.Outputs.Add(new TxOut(Money.Coins(1.0m), scriptPubKey));
```  

Bob and Alice agree to pay Nico 0.005 TBTC for his services.
First they get the ```Coin``` they received from the transaction:  

```cs
Coin coin = received.Outputs.AsCoins().First();
```  

![](../assets/coin.png)  



# Load up this "wallet" 🤑

> ‼️ : The following parts of this guide include live transactions on the bitcoin network. If you send funds to the wrong place, you will have to backtrack. **Don't run the program unless you understand where funds are going.** Ask for help.

Copy down your newly found address. Enter it on [this bitcoin faucet](https://coinfaucet.eu/en/btc-testnet/) to get free bits for testing. Probably around 0.005TBTC depending on the day. [backup faucet](http://bitcoinfaucet.uo1.net/)

Search your **receive address** or **transactionId** (txId) on a [block explorer](https://testnet.smartbit.com.au/) to view the tx network status.
Now we're going to need some network connectivity.

> Who has a bitcoin node installed on their machine?

Add `QBitNinja.Client` to your project:
```console
dotnet add package QBitNinja.Client
```
reference this package at the top of `Program.cs` and use it'
```cs
using QBitNinja.Client
// ...
// ... Append to your `Program.cs` `main`
var client = new QBitNinjaClient(network);

// replace "0acb..." with the txId from the block explorer
// If the faucet doesn't work, look up the address on the explorer. If no tx, try another faucet

var transactionId = uint256.Parse("0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a");
var transactionResponse = client.GetTransaction(transactionId).Result;

Console.WriteLine(transactionResponse.TransactionId); // 0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a
Console.WriteLine(transactionResponse.Block.Confirmations); // 91
```

# Now send 'em!

### From where?

Let's see which output of our transaction we can spend.
>❔: What does it mean to "spend" cryptocurrency


```cs
var receivedCoins = transactionResponse.ReceivedCoins;
OutPoint outPointToSpend = null;
foreach (var c in receivedCoins)
{
    if (c.TxOut.ScriptPubKey == treasurer.ScriptPubKey)
    {
        outPointToSpend = c.Outpoint;
    }
}
if (outPointToSpend == null)
	throw new Exception("TxOut doesn't contain any our ScriptPubKey");
Console.WriteLine("We want to spend outpoint #{0}", outPointToSpend.N + 1);
```

Probably the second outPoint. This is the most regular transaction. We're designing this.

```cs
// Program.cs

// ... After what we have already in main

var tx = Transaction.Create(network);
tx.Inputs.Add(new TxIn()
{
    PrevOut = outPointToSpend
});
```

# To who?
Let's set up some 朋友s. We need friends to send to. Remember how we did this before?

```cs
// Program.cs (cont.)

var alice = new Key();
var bob = new Key();
Console.WriteLine("alice: " + alice.GetWif(network));
Console.WriteLine("bob: " +  bob.GetWif(network));
```
```console
dotnet run
```

Note the addresses
```
// notes.txt

sk: cPaLw36GPtbfiq5rrEWsQLFn1oatdDmj8VRonnveEbFDctVAg5iy
ad: mi9KunGhzEkN7cRy8dzA3XMhQw1tgt5xAC

Alice sk: cW2ZL5hQsYMgC9yuZycY8FsSht7WuVwfqT4XNEiAzskHrwVDKUuY
bob sk: cUfgszwWKyCah2SVe6Xik4pRgLDQKiZmnNAaJRL6WQfStokQAYLQ
```


Like before, save this output and replace the old variables **With your own sk"

```cs
var alice = new BitcoinSecret("cW2ZL5hQsYMgC9yuZycY8FsSht7WuVwfqT4XNEiAzskHrwVDKUuY");
var bob = new BitcoinSecret("cUfgszwWKyCah2SVe6Xik4pRgLDQKiZmnNAaJRL6WQfStokQAYLQ");
```

```cs
var toAlice = new TxOut()
{
    Value = new Money(0.00m, MoneyUnit.BTC),
    ScriptPubKey = alice.ScriptPubKey
};

var toBob = new TxOut()
{
    Value = new Money(0.001m, MoneyUnit.BTC),
    ScriptPubKey = bob.ScriptPubKey
};

transaction.Outputs.Add(toAlice);
transaction.Outputs.Add(toBob);
```

> ‼️: We could sign & broadcast this transaction now. What's missing? What's the problem?

![](../assets/doh.png)  

We must calculate the change output
```cs
var minerFee = new Money(0.0002m, MoneyUnit.BTC);
var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
var changeAmount = txInAmount - toBob.Value - toAlice.Value - minerFee;

var change = new TxOut()
{
    Value = changeAmount,
    ScriptPubKey = treasurer.ScriptPubKey
};

// The first time I tried this I forgot to use a change output and paid the entirety of my main balance to the miners. Whoops
transaction.Outputs.Add(change);
```

# Signing our transaction

```cs
// Program.cs (cont.)

transaction.Inputs[0].ScriptSig = treasurer.ScriptPubKey;
transaction.Sign(treasurer, receivedCoins.ToArray())
```

# Broadcast it

Finally, let's send it to bitcoin nodes and get it in the blockchain

```cs
// Program.cs (cont.)

var broadcastResponse = client.Broadcast(tx).Result;
if (!broadcastResponse.Success)
{
    Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
    Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
}
else
{
    Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
    Console.WriteLine(transaction.GetHash());
}
```

```console
dotnet run
```

Congrats! Bask in your newfound glory 🥳🎉. You just deployed raw Bitcoin Script to the blockchain. It will be there forever!

>‼️ : At this point, assuming Success, coin is spent. set `broadcastResponse = null` or comment how the last block of code we wrote. We can't double-spend in bitcoin. If we could it would be worthless. Running this again will send more money from the treasurer to Alice and Bob

# Writing a Multisig Transaction
Bitcoin allows us to have shared ownership over coins with multi-signature transactions or multisig for short. These, and the transactions you wrote before are simple smart-contracts.

In order to demonstrate this shared ownership we will create a ```ScriptPubKey``` that represents an **m-of-n multisig**. This means that in order to spend the coins, **m** number of private keys will be needed to sign the spending transaction out of the **n** number of different public keys provided.

Let’s create a multi-sig tx with Bob, Alice and our treasurer, where 2 of the 3 of them need to sign a transaction in order to spend a coin.  

At the top:
```cs
using System.Linq;

// ... After what we wrote

var scriptPubKey = PayToMultiSigTemplate
    .Instance
    .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey });

Console.WriteLine(scriptPubKey);
```  
Generates this script which you can use as a public key (coin destination address):
```
2 0282213c7172e9dff8a852b436a957c1f55aa1a947f2571585870bfb12c0c15d61 036e9f73ca6929dec6926d8e319506cc4370914cd13d300e83fd9c3dfca3970efb 0324b9185ec3db2f209b620657ce0e9a792472d89911e0ac3fc1e5b5fc2ca7683d 3 OP_CHECKMULTISIG
```  

As you can see, the ```scriptPubkey``` has the following form: ```<sigsRequired> <pubkeys…> <pubKeysCount> OP_CHECKMULTISIG```  

The process for signing it (in order to be able to spend it) is a little more complicated than just calling ```Transaction.Sign```, which does not work for multisig.

Later we will talk more deeply about the subject but for now let’s use the ```TransactionBuilder``` for signing the transaction.

Imagine the multisig ```scriptPubKey``` received a coin in a transaction called ```received```:

```cs
var received = Transaction.Create(network)
received.Outputs.Add(new TxOut(Money.Coins(1.0m), scriptPubKey));
```  

Bob and Alice agree to pay Nico 0.005 TBTC for his services.
First they get the ```Coin``` they received from the transaction:  

```cs
Coin coin = received.Outputs.AsCoins().First();
```  

![](../assets/coin.png)  

Then, with the ```TransactionBuilder```, they create an **unsigned transaction**.  

```cs
BitcoinAddress nico = new Key().PubKey.GetAddress(network);
TransactionBuilder builder = network.CreateTransactionBuilder();
Transaction unsigned = 
    builder
      .AddCoins(coin)
      .Send(nico, Money.Coins(0.005m))
      .BuildTransaction(sign: false);
```  

The transaction is not yet signed. Here is how Alice signs it:  

```cs
Transaction aliceSigned =
    builder
        .AddCoins(coin)
        .AddKeys(alice)
        .SignTransaction(unsigned);
```  

![](../assets/aliceSigned.png)  

And then Bob:  

```cs
Transaction bobSigned =
    builder
        .AddCoins(coin)
        .AddKeys(bob)
        //At this line, SignTransaction(unSigned) has the identical functionality with the SignTransaction(aliceSigned).
        //It's because unsigned transaction has already been signed by Alice privateKey from above.
        .SignTransaction(aliceSigned);
```  

![](../assets/bobSigned.png)  

Now, Bob and Alice can combine their signatures into one transaction. This transaction will then be valid, because two (Bob and Alice) signatures were used from the three (Bob, Alice and Satoshi) signatures that were initially provided. The requirements of the 'two-of-three' multisig have therefore been met. If this wasn't the case, the network would not accept this transaction, because the nodes reject all unsigned or partially signed transactions.

```cs
Transaction fullySigned =
    builder
        .AddCoins(coin)
        .CombineSignatures(aliceSigned, bobSigned);
```  

![](../assets/fullySigned.png)  

```cs
Console.WriteLine(fullySigned);
```  

```json
{
  ...
  "in": [
    {
      "prev_out": {
        "hash": "9df1e011984305b78210229a86b6ade9546dc69c4d25a6bee472ee7d62ea3c16",
        "n": 0
      },
      "scriptSig": "0 3045022100a14d47c762fe7c04b4382f736c5de0b038b8de92649987bc59bca83ea307b1a202203e38dcc9b0b7f0556a5138fd316cd28639243f05f5ca1afc254b883482ddb91f01 3044022044c9f6818078887587cac126c3c2047b6e5425758e67df64e8d682dfbe373a2902204ae7fda6ada9b7a11c4e362a0389b1bf90abc1f3488fe21041a4f7f14f1d856201"
    }
  ],
  "out": [
    {
      "value": "1.00000000",
      "scriptPubKey": "OP_DUP OP_HASH160 d4a0f6c5b4bcbf2f5830eabed3daa7304fb794d6 OP_EQUALVERIFY OP_CHECKSIG"
    }
  ]
}

```
Before sending the transaction to the network, examine the CombineSignatures() type annotation: compare the two transactions 'bobSigned' and 'fullySigned' thoroughly. It will seem like they are identical. It seems like the CombineSignatures() method is needless in this case because the transaction got signed properly without the CombineSignatures() method.

The transaction is now ready to be sent to the network, but notice that the CombineSignatures() method was critical here, because both the aliceSigned and the bobSigned transactions were only partially signed, therefore not acceptable by the network. CombineSignatures() combined the two partially signed transactions into one fully signed transaction.  

> Sidenote: there is an inherent difficulty which arises from this situation. You need to send the newly created, unsigned multi-sig transaction to every signer and after their signed it, you also need to collect the partially signed transactions from them and combine them into one, so that you can publish that on the network. This problem is partially solved by the [BIP-0174](https://github.com/bitcoin/bips/blob/master/bip-0174.mediawiki), because it at least standardizes the data format, but you still need to implement your own way to distribute the data between the signing parties.  
> NBitcoin doesn't have an implementation for BIP-0174 or for the off-chain data distribution _yet_.

Although the Bitcoin network supports multisig as explained above, the one question worth asking is: How can you expect a user who has no clue about Bitcoin to pay to a complicated multisig script address containing Alice's, Bob's Satoshi's public keys as we have done?

Don’t you think it would be cool if we could represent such a ```scriptPubKey``` as easily and concisely as a regular Bitcoin Address?

Well, this is possible using something called a **Bitcoin Script Address** (also called Pay to Script Hash or P2SH for short).

Nowadays, **native Pay To Multi-Sig** (as you have seen above) and **native P2PK** are never used directly. Instead they are wrapped into something called a **Pay To Script Hash** payment. We will look at this type of payment in the next section.

# P2SH 2-of-3 Transaction

Remember writing `scriptPubKey`? Let's write a paymentScript instead:
```cs
var paymentScript = PayToMultiSigTemplate
    .Instance
    .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey }).PaymentScript;

Console.WriteLine(paymentScript);
```

```
OP_HASH160 57b4162e00341af0ffc5d5fab468d738b3234190 OP_EQUAL
```  

The output hash represents the hash of the previous multi-sig script (containing `OP_CHECKMULTISIG`)

Since it's a hash, we can easily convert it to a base58 bitcoin address

```cs
Script redeemScript = PayToMultiSigTemplate
    .Instance
    .GenerateScriptPubKey(2, new[] { bob.PubKey, alice.PubKey, treasurer.PubKey }).PaymentScript;

Console.WriteLine(redeemScript.Hash.GetAddress(network);
```

Let's pay this multi-sig script
```cs
Transaction received = Transaction.Create(network)
//Pay to the script hash
received.Outputs.Add(new TxOut(Money.Coins(0.005m), redeemScript.Hash));
```

Instead of creating a `Coin`, because this asset is shared, we have to create a `ScriptCoin`
```cs
ScriptCoin coin = received.Outputs.AsCoins().First()
    .ToScriptCoin(redeemScript);
```

### What I'm doing:
* Create a new testnet BosStrat $BOSS
* Open Swagger
	1. GET:api/mnemonic
	2. mnemonic >> POST:api/Wallet/Create 
	3. POST:api/Wallet/account
	4. GET:api/Wallet/unusedaddress
* Mine 98mil to my address:TRbJSTpYvK1dDes3EACYxKx9jXeF7cjufY. Make sure I have enough UTXOs past the POW era
* Run sufficientr network infrastructure to supoort AT LEAST 50 nodes on the network. We will probably have more like 100 on the real deal

## Prequisites
* Ubuntu, MacOS, Windows machine. Other OS: YMMV

.NET Core SDK 2.1 https://www.microsoft.com/net/learn/dotnet/hello-world-tutorial#windowsvs2017

SBFN - AzureMaster
* Running BosStrat network | port: 16777 | RPC: 16888
* BosStrat Swagger
	1. GET:api/mnemonic
	2. mnemonic >> POST:api/Wallet/Create 
	3. POST:api/Wallet/account
	4. GET:api/Wallet/unusedaddress

* Make A request to my faucet TODO: block the same address from more than one request

* Write multisig tx with new address input
* Post / fund your tx

* Hidden prize
 
## Workshop "design document"
###### Write here when hitting head on table
Fuck it's difficult to generate a wallet. This is going to have to be a very easy thing for people to do


Thu Nov  1 23:13:19 2018
I have to generate a wallet and mnemonic for everybody to draw from. Maybe there's some way to sign a tx so everyone has my pkey from premine?
* Only works if nobody cheats

Ideally set up wallet in some command. This has to exist already.....

The swagger API works pretty well for the stratis net. Going to have to set up a webserver for BOSS? I think Swagger is too complex but we'll see. Finally generated a wallet if not on our network.


Sat Nov  3 12:22:17 2018
We're at Tufts. I really doubt we will be able to get long work going but I may be able to generate addresses and make incremental changes to my product.


Sat Nov  3 23:30:50 2018
Just realize I need to get rid of Checkpoints because otherwise we can't get valid blocks. Still not sure how to generate Genesis....


Sun Nov  4 00:21:46 2018
Refuled - updated network rules by replacing timestamp, removing checkpoints, null assumeDefault


Sun Nov  4 13:59:29 2018
People are totally capable of following the tut. Needs to be more comprehensive.
