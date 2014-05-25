# [![ScriptCs](https://secure.gravatar.com/avatar/5c754f646971d8bc800b9d4057931938?s=200)](http://scriptcs.net/).[![Bedford OB](http://mookid.dk/oncode/wp-content/2011/10/logo400x150.png)](https://github.com/rebus-org/Rebus/)

## What is it?

A [ScriptCs](https://github.com/scriptcs/scriptcs) script pack for [Rebus](https://github.com/rebus-org/Rebus).

Wouldn't it just be nice if those lovely scripts of yours could talk to other scripts? You're up for a treat. With this script pack, it is possible to write scripts that can communicate via messaging to other scripts.

Get it on [Nuget](https://www.nuget.org/packages/ScriptCs.Rebus/0.1.0).

## Installation
Prior to using this script pack you need to [install ScriptCs](https://github.com/scriptcs/scriptcs#getting-scriptcs). Then open a command prompt as administrator. Now create a folder for the scripted messaging project. Navigate to this folder and install the Nuget package by running this command:

    scriptcs -install ScriptCs.Rebus

You're now ready to do some serious scripted messaging.

## Basic usage from REPL
To enter the REPL, run `scriptcs`. Send a message of type string to MyMessageQueue:

    Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<string>("My first scripted message!")

Now open another command prompt or use the same, and enter the following to receive the message of type string from MyMessageQueue:

    Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Receive<string>(x => Console.WriteLine(x)).Start()

The command prompt should output the following:

    > From MyMessageQueue > My first scripted message!

## Basic usage from script
The examples from above apply to scripts. Put the send and receive code into two `.csx` files, lets call them `send.csx` and `receive.csx`, and execute them by typing the following from a command prompt:

    scriptcs send.csx

and

    scriptcs receive.csx

In the [`\samples`](https://github.com/madstt/ScriptCs.Rebus/tree/master/samples) folder there is some samples to get you started.

## Advanced usage
At this point you should be able to apply these more advanced usages to both the REPL and from scripts. 

### Custom types
It is possible to use custom types in your messages. Consider the following class:

    public class Message
    {
		public string Content { get; set; }
	}

You enter this class definition in the REPL or create a `message.csx` script file. If defined in a script file, load this class definition into your script by using `#load` keyword,

    #load "message.csx"

Now you can send and receive messages of this type by using the following syntax:

    Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<Message>(new Message {Content = "Hello from custom type!"})

and

    Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Receive<Message>(x => Console.WriteLine(x.Content)).Start()

This should output `> From MyMessageQueue > Hello from custom type`.

### Multiple handlers
In some cases you might be expecting to handle different kinds of messages. This can be achieved by adding multiple receive commands fluently:

    Require<RebusScriptBus>().ConfigureBus("MyMessageQueue")
		.Receive<string>(x => Console.WriteLine(x))
		.Receive<Message>(x => Console.WriteLine(x.Content))
		.Start()

Applying multiple handlers of the same type, will give the possibility to handle message of the same type in different ways.






