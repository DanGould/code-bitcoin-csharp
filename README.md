# Building Blocks
######BostonHacks 2018 Stratis x FinTechBU Workshop: Writing a Transaction
#### Writing directly to chain


### Background
This workshop is designed for anyone with limited technical background. Your hand will be held through
* hosting your own lite node
* validating the blockchain
* writing custom transactions
* mining
Our ultimate goal is to team with partners and write a 2-of-3 multisignature transaction. We will discuss the scenarios where such a transaction and others may benefit organizations' financing

Huge thanks to everyone who contributed to [Programming the Blockchain in C#](https://github.com/ProgrammingBlockchain/ProgrammingBlockchain). Much of this workshop comes from that text.

# Let's get started

If you want to use .NET Core, first install .NET Core [as documented here](https://www.microsoft.com/net/core#windowsvs2017).

Then:
```
mkdir MyProject
cd MyProject
dotnet new console
dotnet add package NBitcoin
dotnet restore
```
Then edit your Program.cs:
```
using System;
using NBitcoin;

namespace _125350929
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World! " + new Key().GetWif(Network.Main));
        }
    }
}
```
You can then run with
```
dotnet run
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

