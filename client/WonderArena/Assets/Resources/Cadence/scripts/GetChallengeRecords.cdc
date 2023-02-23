 // import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

pub fun main(attacker: Address, defender: Address): [WonderArenaBattleField_BasicBeasts1.ChallengeRecord] {
    if let attackerRecords = WonderArenaBattleField_BasicBeasts1.challenges[attacker] {
        if let records = attackerRecords[defender] {
            return records
        }
    }
    return []
}