# [![ScriptCs](https://secure.gravatar.com/avatar/5c754f646971d8bc800b9d4057931938?s=200)](http://scriptcs.net/).[![Bedford OB](http://mookid.dk/oncode/wp-content/2011/10/logo400x150.png)](https://github.com/rebus-org/Rebus/)

## What is it? [![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg?style=flat)](https://www.nuget.org/packages/ScriptCs.Rebus/)

A [scriptcs](https://github.com/scriptcs/scriptcs) script pack for [Rebus](https://github.com/rebus-org/Rebus).

Wouldn't it just be nice if those lovely scripts of yours could talk to other scripts? You're up for a treat. With this script pack, it is possible to write scripts that can communicate via messaging to other scripts.

Get it on [NuGet](https://www.nuget.org/packages/ScriptCs.Rebus).

## Installation
Prior to using this script pack you need to [install scriptcs](https://github.com/scriptcs/scriptcs#getting-scriptcs). Then open a command prompt as administrator. Now create a folder for the scripted messaging project. Navigate to this folder and install the Nuget package by running this command:

    scriptcs -install ScriptCs.Rebus

You're now ready to do some serious scripted messaging.

## Basic Usage from REPL
To enter the REPL, run `scriptcs`. Send a message of type string to MyMessageQueue:

    Require<RebusScriptBus>()
    	.ConfigureBus("MyMessageQueue")
		.Send<string>("My first scripted message!")

Now open another command prompt or use the same, and enter the following to receive the message of type string from MyMessageQueue:

    Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
		.Receive<string>(x => Console.WriteLine(x))
		.Start()

The command prompt should output the following:

    > My first scripted message!

### Using RabbitMQ
By default scriptcs.rebus uses MSMQ as transport layer, but it is possible to use RabbitMQ instead. Using RabbitMQ requires a separate dependency:

    scriptcs -install ScriptCs.Rebus.RabbitMQ


Then configure the bus like this:
    
    Require<RebusScriptBus>()
		.ConfigureRabbitBus("MyMessageQueue")
		.Send<string>("Message from RabbitMQ")

This would use a local RabbitMQ instance and the default RabbitMQ port, `ampq://localhost:5672`. To use another port or a remote server, you can supply a connectionstring like this:

    Require<RebusScriptBus>()
		.ConfigureRabbitBus("MyMessageQueue", "ampq://remoteserver")
		.Send<string>("Message from RabbitMQ")

Receiving messages over RabbitMQ, is similar to MSMQ, with the same exceptions as sending, i.e.:

	    Require<RebusScriptBus>()
		.ConfigureRabbitBus("MyMessageQueue", "ampq://remoteserver")
		.Receive<string>(x => Console.WriteLine(x))
		.Start()

### Using Azure Service Bus
The third, and last option, is to use the Azure Service Bus as transport layer. Like the RabbitMQ option, the Azure Service Bus option requires a separate dependency:

    scriptcs -install ScriptCs.Rebus.AzureServiceBus

Configuration is similar to MSMQ and RabbitMQ, with a few exceptions:

    Require<RebusScriptBus>()
		.ConfigureAzureBus("MyMessageQueue", "Endpoint=sb://someConnectionString")
		.Send<string>("Message from Azure Service Bus")

Receiving messages is similar to MSMQ and RabbitMQ.

To use the Azure Service Bus option you are required to create an Azure Service Bus yourself. scriptCs.rebus will create the queue.

## Basic Usage from Script
The examples from above apply to scripts. Put the send and receive code into two `.csx` files, lets call them `send.csx` and `receive.csx`, and execute them by typing the following from a command prompt:

    scriptcs send.csx

and

    scriptcs receive.csx

In the [`\samples`](https://github.com/madstt/ScriptCs.Rebus/tree/master/samples) folder there is some samples to get you started.

## Advanced Usage
At this point you should be able to apply these more advanced usages to both the REPL and from scripts. 

### Custom Types
It is possible to use custom types in your messages. Consider the following class:

    public class Message
    {
		public string Content { get; set; }
	}

You enter this class definition in the REPL or create a `message.csx` script file. If defined in a script file, load this class definition into your script by using `#load` keyword,

    #load "message.csx"

Now you can send and receive messages of this type by using the following syntax:

    Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
		.Send<Message>(new Message {Content = "Hello from custom type!"})

and

    Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
		.Receive<Message>(x => Console.WriteLine(x.Content))
		.Start()

This should output `> From MyMessageQueue > Hello from custom type`.

### Multiple Handlers
In some cases you might be expecting to handle different kinds of messages. This can be achieved by adding multiple receive commands fluently:

    Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
		.Receive<string>(x => Console.WriteLine(x))
		.Receive<Message>(x => Console.WriteLine(x.Content))
		.Start()

Applying multiple handlers of the same type, will give the possibility to handle message of the same type in different ways.

### Using Console Logging
Working with messaging and message buses can often be a very complex task. It can therefore be of great help to supply the script author with some additional information about what's going on. Rebus has some very nice console logging features, which is made available in scriptcs.rebus by adding `.UseLogging()` to your scripts:

    Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
			.UseLogging()	
		.Send<string>("A string is sent!")

This will produce an output like this:

    Rebus.Configuration.RebusConfigurer DEBUG (): Defaulting to 'throwing endpoint mapper' - i.e. the bus will throw an exception when you send a message that is not explicitly routed
	Rebus.Configuration.RebusConfigurer DEBUG (): Defaulting to in-memory saga persister (should probably not be used for real)
	Rebus.Configuration.RebusConfigurer DEBUG (): Defaulting to in-memory subscription storage (should probably not be used for real)
	Rebus.Bus.RebusBus INFO (): Rebus bus 1 created
	Rebus.Bus.RebusBus INFO (): Using external timeout manager with input queue 'rebus.timeout'
	Rebus.Bus.RebusBus INFO (): Initializing bus with 1 workers
	Rebus.Bus.Worker INFO (): Worker Rebus 1 worker 1 created and inner thread started
	Rebus.Bus.Worker INFO (): Starting worker thread Rebus 1 worker 1
	Rebus.Bus.RebusBus INFO (): Bus started
	Sending message of type String...
	Rebus.Logging.MessageLogger DEBUG (): Sending Foo to MyMessageQueue
	... message sent.

### Sending Scripts to a Host
To be able to send complete scripts or script files using one the above mentioned transports to a host application for execution, use the following basic API for sending a small script:

	Require<RebusScriptBus>()
		.ConfigureBus("MyMessageQueue")
		.WithAScript("Console.WriteLine(\"Hello from a host!\")")
		.Send()

And a script file:

	Require<RebusScriptBus>()
		.Configure("MyMessageQueue")
		.WithAScriptFile("start.csx")
		.Send()

The script or script file is now sent to a host application that can execute this. Now there is a number of options to go for configuring the execution inside the host application:

#### Add a Local Reference
If you want to reference a local assembly to be used in your script, this could be an assembly that is part of the host application, you can add the following option:

	Require<RebusScriptBus>()
		.ConfigureBus("myOwnBus")
		.WithAScriptFile("start.csx")
			.AddLocal("someLocal.dll")
		.Send()

Now, you can reference more local assemblies, just add another `AddLocal()`.

#### Download and Reference a NuGet Package
So if your script needs a NuGet package to extend the host, you can specify these using the `AddFromNuGet()` like this:

	Require<RebusScriptBus>()
		.ConfigureBus("myOwnBus")
		.WithAScriptFile("start.csx")
			.AddFromNuGet("mongocsharpdriver")
		.Send()

Again, you can add multiple `AddFromNuGet()` to the configuration.

#### Importing a Namespace
So, imagine we now have downloaded a NuGet package or referenced a local assembly, then to make it available for the script, you need to specify in which namespace to find the type you wanna use:

	Require<RebusScriptBus>()
		.ConfigureBus("myOwnBus")
		.WithAScriptFile("start.csx")
			.AddFromNuGet("mongocsharpdriver")
			.ImportNamespace("MongoDB.Bson")
		.Send()

This would download and reference the Mongo C# driver and make the MongoDB.Bson namespace available to the script for execution.

#### Using the Mono Compiler
Since the current release of Roslyn, does not support things like the `dynamic` keyword or `async/await`, it might be handy to be able to use the Mono compiler on the host:

	Require<RebusScriptBus>()
		.ConfigureBus("myOwnBus")
		.WithScriptFile("start.csx")
			.UseMono()
		.Send()

Now the Mono compiler is set as execution engine, the script will be compiled. A scenario like this could also be relevant when doing cross-platform development.

#### Logging
In some cases it might be great to see some debugging output in the host console.  

	Require<RebusScriptBus>()
		.ConfigureBus("myOwnBus")
		.WithScriptFile("start.csx")
			.UseLogging()
		.Send()

This sets the loglevel to Debug, instead of Info which is default. This feature is only relevant in a limited number of cases where the host is a console application.

#### Using Script Packs
Script packs are really just NuGet packages used for hiding boilerplate code of common frameworks. Therefore, if you download and reference a script pack, it will be made available to the script without any further.



