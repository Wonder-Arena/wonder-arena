import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(
    attackerAddress: Address,
    attackerIDs: [UInt64],
    defenderAddress: Address
) {
    let adminRef: &WonderArenaBattleField_BasicBeasts1.Admin
    prepare(acct: AuthAccount) {
        self.adminRef = acct
            .borrow<&WonderArenaBattleField_BasicBeasts1.Admin>(from: WonderArenaBattleField_BasicBeasts1.AdminStoragePath)
            ?? panic("borrow battle field admin failed")
    }

    execute {
        let attackerGroup = WonderArenaBattleField_BasicBeasts1.BeastGroup(
            name: "AttackerGroup",
            beastIDs: attackerIDs
        )

        self.adminRef.fight(
            attackerAddress: attackerAddress,
            attackerGroup: attackerGroup,
            defenderAddress: defenderAddress
        )
    }
}
