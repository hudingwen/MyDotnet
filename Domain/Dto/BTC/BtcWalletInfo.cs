 

namespace MyDotnet.Domain.Dto.BTC
{
    public class BtcWalletInfo
    {
        public string address { get; set; }
        public long total_received { get; set; }
        public long total_sent { get; set; }
        public long balance { get; set; }
        public long unconfirmed_balance { get; set; }
        public long final_balance { get; set; }
        public int n_tx { get; set; }
        public int unconfirmed_n_tx { get; set; }
        public int final_n_tx { get; set; }
    }
}
