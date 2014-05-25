#load "message.csx"
#load "customer.csx"

Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<string>("Hello from Message!");
Require<RebusScriptBus>().ConfigureBus("MyMessageQueue").Send<string>("Foo Baa");

