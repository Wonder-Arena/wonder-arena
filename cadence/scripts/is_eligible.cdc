import WonderArenaRewards_BasicBeasts1 from "../contracts/WonderArenaRewards_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(host: Address, rewardID: UInt64, claimer: Address): Bool {
    if let rewardCollectionRef = getAccount(host)
        .getCapability(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath)
        .borrow<&{WonderArenaRewards_BasicBeasts1.IRewardCollectionPublic}>() {

        if let reward = rewardCollectionRef.borrowPublicReward(id: rewardID) {
            if let score = WonderArenaBattleField_BasicBeasts1.scores[claimer] {
                return (score >= reward.scoreThreshold) && (reward.claimed[claimer] == nil)
            }
        }
    }

    return false
}