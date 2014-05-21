#load "message.csx"

//Require<RebusScriptBus>().Receive<Message>("MyMessageQueue", x => Console.WriteLine(x.Content));


Require<RebusScriptBus>()
	.ConfigureBus("MyMessageQueue")
	.Receive<Message>(x => Console.WriteLine(x.Content))
	.Start();