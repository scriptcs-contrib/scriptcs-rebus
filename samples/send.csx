#load "message.csx"

Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<Message>(new Message {Content = "Hello from Message!"});

