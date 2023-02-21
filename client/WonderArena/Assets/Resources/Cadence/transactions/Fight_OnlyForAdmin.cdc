// import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

transaction(
    attackerAddress: Address,
    attackerIDs: [UInt64],
    defenderAddress: Address
) {
    prepare(acct: AuthAccount) {}

    execute {
        WonderArenaBattleField_BasicBeasts1.fight(
            attackerAddress: attackerAddress,
            attackerIDs: attackerIDs,
            defenderAddress: defenderAddress
        )
    }
}