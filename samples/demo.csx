#r "DC.Positions.Shared"

Require<RebusScriptBus>().Send("dc.positions.trades.input", new CalculateTrade<PowerFinancialTransaction> {TradeId = 5});
