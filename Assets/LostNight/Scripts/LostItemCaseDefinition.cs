namespace LostNight
{
    public sealed class LostItemCaseDefinition
    {
        public LostItemModelKind ModelKind { get; }
        public string ItemName { get; }
        public string[] Clues { get; }
        public string[] ClaimantNames { get; }
        public string[] Claims { get; }
        public int OwnerIndex { get; }
        public string SuccessReason { get; }
        public string FailureReason { get; }
        public string Memory { get; }

        public LostItemCaseDefinition(LostItemModelKind modelKind, string itemName, string[] clues, string[] claimantNames, string[] claims,
            int ownerIndex, string successReason, string failureReason, string memory)
        {
            ModelKind = modelKind;
            ItemName = itemName;
            Clues = clues;
            ClaimantNames = claimantNames;
            Claims = claims;
            OwnerIndex = ownerIndex;
            SuccessReason = successReason;
            FailureReason = failureReason;
            Memory = memory;
        }
    }
}
