import WonderArenaRewards_BasicBeasts1 from "../contracts/WonderArenaRewards_BasicBeasts1.cdc"

pub fun hasRewardCollection(_ address: Address): Bool {
  return getAccount(address)
    .getCapability<&WonderArenaRewards_BasicBeasts1.RewardCollection{WonderArenaRewards_BasicBeasts1.IRewardCollectionPublic}>(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath)
    .check()
}

transaction {
  prepare(acct: AuthAccount) {
    if !hasRewardCollection(acct.address) {
      if acct.borrow<&WonderArenaRewards_BasicBeasts1.RewardCollection>(from: WonderArenaRewards_BasicBeasts1.RewardCollectionStoragePath) == nil {
        acct.save(<-WonderArenaRewards_BasicBeasts1.createEmptyRewardCollection(), to: WonderArenaRewards_BasicBeasts1.RewardCollectionStoragePath)
      }

      acct.unlink(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath)
      acct.link<&WonderArenaRewards_BasicBeasts1.RewardCollection{WonderArenaRewards_BasicBeasts1.IRewardCollectionPublic}>(WonderArenaRewards_BasicBeasts1.RewardCollectionPublicPath, target: WonderArenaRewards_BasicBeasts1.RewardCollectionStoragePath)
    }
  }
}