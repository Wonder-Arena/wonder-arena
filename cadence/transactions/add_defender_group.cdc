import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(userName: String, groupName: String, beastIDs: [UInt64]) {
    let playerRef: &WonderArenaBattleField_BasicBeasts1.Player

    prepare(acct: AuthAccount) {
        self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("borrow player failed")
    }

    execute {
        let group = WonderArenaBattleField_BasicBeasts1.BeastGroup(
            name: groupName,
            beastIDs: beastIDs
        )
        self.playerRef.addDefenderGroup(group: group)
        if self.playerRef.name != userName {
            self.playerRef.updateName(userName) 
        }
    }
}
