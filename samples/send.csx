#load "message.csx"

Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<Message>(new Message {Content = "Hello from Message!"});

//Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<string>("This is awesome");

