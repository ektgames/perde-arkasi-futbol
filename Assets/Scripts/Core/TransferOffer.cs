namespace BehindTheScenesFootball.Core
{
    public enum OfferStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    [System.Serializable]
    public class TransferOffer
    {
        public string Id;
        public string PlayerId;
        public string PlayerName;
        public string BidderClubId;
        public string BidderClubName;
        public string CurrentClubId;
        public string CurrentClubName;
        public int TransferFee;
        public int OfferedWeeklyWage;
        public int ContractLengthYears;
        public OfferStatus Status;
        public bool IsLoanOffer;

        public TransferOffer(string id, string playerId, string playerName, string bidderClubId, string bidderClubName, string currentClubId, string currentClubName, int transferFee, int offeredWeeklyWage, int contractLengthYears)
        {
            Id = id;
            PlayerId = playerId;
            PlayerName = playerName;
            BidderClubId = bidderClubId;
            BidderClubName = bidderClubName;
            CurrentClubId = currentClubId;
            CurrentClubName = currentClubName;
            TransferFee = transferFee;
            OfferedWeeklyWage = offeredWeeklyWage;
            ContractLengthYears = contractLengthYears;
            Status = OfferStatus.Pending;
        }
    }
}
