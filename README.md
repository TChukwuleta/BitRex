# BitRex
BitRex is an Exchange and Remittance service written in C# programming language.


### BitRex as an Exchange service

It would be good to point out that the exchange bit of this project seem to mock [Boltz](https://boltz.exchange/)

The service currently exchanges between on-chain payment and off-chain payment and also leverages submarine swaps to enable trustless exchange.

For a user who wants to exchange off-chain for on-chain bitcoin, he confirms how much he wants to exchange for. A summary of the exchange is revealed to the user which would include the return amount, service charges as well as the miner's fee.

The user then goes ahead to input the address he would like to receive the money in then the swaps occurs.

The same flow happens for when the user wants to exchange for lightning sats.

The service works with real time bitcoin pricing which is gotten from [Galoy API](https://api.staging.galoy.io/graphql)


### BitRex as an Exchange service

This service enables user to be able to buy on-chain or off-chain bitcoin using fiat (currently limited to just Naira).

Currently integrated with [Paystack](https://paystack.com/) a fiat payment process, which would enable you pay using either your card, transfer.

A user comes on the platform, specifies how much of sats in naira he wants to purchase. Likewise a summary detail is made available to the user which he then proceeds to input his destination address/ invoice.

The payment page is prompted to the user and transaction is expected to be made to the user. Once the money is confirmed, the equivalent value is paied to the destination address provided.

Good to note also that this makes use of Galoy API to fetch realtime bitcoin prices.


## Installation of dependencies

This application makes use of Bitcoin Core and Polar for its lightning testing node environment.
Regarding database, this application also makes use of Mysql DB, so if you would love to run this application you can go ahead and download SSMS



## Configuration

For the required environment file, please see the 'requirements.txt' file


The following are a list of currently available configuration options and a 
short explanation of what each does.

`MinimumAmountBtc`
This specifies the minimum BTC value that can be transacted on the exchange

`MaximumAmountBtc`
This specifies the maximum BTC value that can be transacted on the exchange

`ServiceChargeBtc`
This specifies the minimum value in percentage that would be charged by the syste for all Bitcoin transactions 


`ServiceChargeLnBtc`
This specifies the minimum value in percentage that would be charged by the syste for all Bitcoin transactions 

`MinimumAmountBtc`
This specifies the minimum BTC value that can be transacted on the exchange

`DustValue`
Very tiny amount of bitcoin which is lower than a bitcoin transaction limit

`DefaultConnection`
Connection string to your sql database server




`Bitcoin` (required)
All connection necessary to connect to your bitcoin node, which includes bitcoin url, username, password as well as wallet name

`Lightning` (required)
All connection necessary to connect to your bitcoin node, which includes macaroon path, ssl cert path, grpc host name

`NODE_URI` (required; in the form *pubkey@host/ip:port*)
The node uri is an identifier for your lightning network node, you can obtain this 
by running the command `lncli getinfo` and copying any of the values in `uris`.


## Initializing the database

To initialize the database which would create the database file and all the 
necessary tables, open your package manager console and run the commands:

```
$ Add-Migration migration_name
$ Update-Database

```

Please ensure you already have your sql server setup.


## Running the application server

After installing the dependencies, configuring the application, initializing the database, you can start the application backend by 
running the command:

```
$ dotnet run
```
