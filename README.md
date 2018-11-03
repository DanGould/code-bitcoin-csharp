# Building Blocks
######BostonHacks 2018 Stratis x FinTechBU Workshop: Writing a Transaction
#### Writing directly to chain


## Background
This workshop is designed for anyone with limited technical background. Your hand will be held through
* hosting your own lite node
* validating the blockchain
* writing custom transactions
* mining
Our ultimate goal is to team with partners and write a 2-of-3 multisignature transaction. We will discuss the scenarios where such a transaction and others may benefit organizations' financing

## What I'm doing:
* Create a new testnet BosStrat $BOSS
* Open Swagger
	1. GET:api/mnemonic
	2. mnemonic >> POST:api/Wallet/Create 
	3. POST:api/Wallet/account
	4. GET:api/Wallet/unusedaddress
* Mine 98mil to my address:SfgvvZrp4fq2xHrUVWExY9wF6XY9pAf6gM. Make sure I have enough UTXOs past the POW era
* Run sufficientr network infrastructure to supoort AT LEAST 50 nodes on the network. We will probably have more like 100 on the real deal

## Prequisites
* Ubuntu, MacOS, Windows machine. Other OS: YMMV

.NET Core SDK 2.1

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



