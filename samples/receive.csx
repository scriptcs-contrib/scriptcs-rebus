
Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Receive<string>(x => Console.WriteLine(x)).Start();