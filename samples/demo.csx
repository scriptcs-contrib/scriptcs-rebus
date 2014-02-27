Require<RebusScriptBus>().Send("dc.positions.trades.input", new CalculateTrade<PowerFinancialTransaction> {TradeId = 5});

public class CalculateTrade<T>
{
        public int TradeId { get; set; }
}

public class PowerFinancialTransaction
{}
