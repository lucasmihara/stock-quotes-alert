# Stock Quotes Alert
This console application alerts via email when a chosen stock is under a buy price or over a sell price.

## Usage

### Setting up

You will need an email account open to sending emails from external programs, I recommend using Gmail services with the following configurations:
```
host: "smtp.gmail.com"
port: 587
```
You may also need to set your gmail account to [allow less secure apps](https://myaccount.google.com/lesssecureapps).

*Important: check the credentials as the program don't check it before trying to send.

You will also need a [HG Brasil Finance API Key](https://hgbrasil.com/status/finance), you need to create an account, but the API Key is free.

All these settings must be written in the  ***config.json*** file in the same directory of the executable file generated after running the program one time or running it with the ***config*** argument. For example:
```
stock-quote-alert.exe config
```

The program can email to many recipients, just insert the recipient email address in the emails.txt file in the same directory of the executable file generated after running the program one time or running it with the ***emails*** argument. For example:
```
stock-quote-alert.exe emails
```

### Using
The program take three arguments: the stock symbol, the sell reference price and the buy reference price, like this:
```
stock-quote-alert [symbol] [sell price] [buy price]
```
Note that the symbol must be a valid B3 stock symbol. Usage example:
```
stock-quote-alert.exe PETR4 22.67 22.59
```
## How it works
The program calls the API for the selected stock every 15 minutes (this is the update time of the API) and check if the price is under the buy price or over the sell price and if this condition is true it will send an email to every email address in the ***emails.txt*** file.