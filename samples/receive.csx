#load "message.csx"

//Require<RebusScriptBus>().Receive<Message>("MyMessageQueue", x => Console.WriteLine(x.Content));


Require<RebusScriptBus>()
	.ConfigureBus("MyMessageQueue")
	.Receive<string>(x => Console.WriteLine(x))
//	.Receive<Message>(x => Console.WriteLine(x.Content))
	.Start();