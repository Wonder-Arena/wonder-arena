import WonderArenaReward_BasicBeasts1 from "./WonderArenaReward_BasicBeasts1.cdc"

pub fun main(address: Address): {UInt64: &{WonderArenaReward_BasicBeasts1.IRewardPublic}}  {
    let account = getAccount(address)
    let rewardCollection = account
        .getCapability(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
        .borrow<&{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>()
        ?? panic("Could not borrow reward collection")

    return rewardCollection.getAllRewards()
}