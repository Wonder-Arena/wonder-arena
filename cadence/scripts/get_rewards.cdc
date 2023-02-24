import WonderArenaRewards_BasicBeasts1 from "../contracts/WonderArenaRewards_BasicBeasts1.cdc"

pub fun main(address: Address): {UInt64: &{WonderArenaRewards_BasicBeasts1.IRewardPublic}}  {
    let account = getAccount(address)
    let rewardCollection = account
        .getCapability(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath)
        .borrow<&{WonderArenaRewards_BasicBeasts1.IRewardCollectionPublic}>()
        ?? panic("Could not borrow reward collection")

    return rewardCollection.getAllRewards()
}