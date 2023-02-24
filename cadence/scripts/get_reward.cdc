import WonderArenaRewards_BasicBeasts1 from 0x2432e062f9f14295

pub struct Reward {
    pub let name: String
    pub let description: String
    pub var scoreThreshold: Int64
    pub var isEnabled: Bool
    pub let host: Address
    pub let claimedCount: UInt64
    pub let availableCount: UInt64
    
    init(
        name: String,
        description: String,
        scoreThreshold: Int64,
        isEnabled: Bool,
        host: Address,
        claimedCount: UInt64,
        availableCount: UInt64
    ) {
        self.name = name
        self.description = description
        self.scoreThreshold = scoreThreshold
        self.isEnabled = isEnabled
        self.host = host
        self.claimedCount = claimedCount
        self.availableCount = availableCount
    }
}

pub fun main(): Reward  {
    let account = getAccount(0x2432e062f9f14295)
    let rewardCollection = account
        .getCapability(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath)
        .borrow<&{WonderArenaRewards_BasicBeasts1.IRewardCollectionPublic}>()
        ?? panic("Could not borrow reward collection")

    let reward = rewardCollection.getAllRewards().values[0]
    let availableCount = reward.getAvailableRewards()
    return Reward(
        name: reward.name,
        description: reward.description,
        scoreThreshold: reward.scoreThreshold,
        isEnabled: reward.isEnabled,
        host: reward.host,
        claimedCount: reward.claimedCount,
        availableCount: availableCount
    )
}