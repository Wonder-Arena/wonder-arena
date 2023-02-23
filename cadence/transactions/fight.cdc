import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

transaction(
    attackerAddress: Address,
    attackerIDs: [UInt64],
    defenderAddress: Address
) {
    prepare(acct: AuthAccount) {}

    execute {
        let attackerGroup = WonderArenaBattleField_BasicBeasts1.BeastGroup(
            name: "AttackerGroup",
            beastIDs: attackerIDs
        )

        WonderArenaBattleField_BasicBeasts1.fight(
            attackerAddress: attackerAddress,
            attackerGroup: attackerGroup,
            defenderAddress: defenderAddress
        )
    }
}
