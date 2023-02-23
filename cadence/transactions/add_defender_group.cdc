import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(name: String, beastIDs: [UInt64]) {
    let playerRef: &WonderArenaBattleField_BasicBeasts1.Player

    prepare(acct: AuthAccount) {
        self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("borrow player failed")
    }

    execute {
        let group = WonderArenaBattleField_BasicBeasts1.BeastGroup(
            name: name,
            beastIDs: beastIDs
        )
        self.playerRef.addDefenderGroup(group: group)
    }
}
